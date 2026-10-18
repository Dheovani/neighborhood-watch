using System.Diagnostics;
using Godot;
using NeighborhoodWatch.Characters;
using NeighborhoodWatch.Gameplay.Missions;
using NeighborhoodWatch.Gameplay.Missions.Domain;
using NeighborhoodWatch.Ui;

namespace NeighborhoodWatch.Networking;

/// <summary>Development ENet session. The server owns movement, mission state, and interaction results.</summary>
public partial class NetworkSession : Node
{
    private const int DefaultPort = 24567;
    private const int MaxClients = 8;
    private readonly Dictionary<int, ServerActor> _serverActors = [];
    private readonly Dictionary<(int Observer, int Target), bool> _knownVisibility = [];
    private readonly Dictionary<(int Peer, string Spot), int> _knownSpotCounts = [];
    private readonly Dictionary<string, int> _spotUses = new(StringComparer.Ordinal);
    private readonly Dictionary<int, PlayerController> _clientActors = [];
    private readonly Dictionary<int, Vector3> _clientTargets = [];
    private readonly Queue<(int PeerId, string SpotName)> _pendingInteractions = new();
    private readonly HashSet<string> _localCompleted = new(StringComparer.Ordinal);
    private readonly List<InfractionEvent> _serverInfractions = [];
    private ENetMultiplayerPeer? _peer;
    private Process? _localServerProcess;
    private volatile bool _localServerReady;
    private ulong _localServerStartMs;
    private int _localServerPort;
    private ulong _connectionStartMs;
    private bool _connected;
    private bool _startedFromCommandLine;
    private bool _endingSession;
    private PackedScene _playerScene = null!;
    private Node3D _playersRoot = null!;
    private MissionHud _hud = null!;
    private MissionCatalog _catalog = null!;
    private IReadOnlyList<MissionDefinition> _clientAssigned = [];
    private MissionSpot[] _spots = [];
    private bool _networked;
    private bool _server;
    private int _snapshotCounter;
    private int _nextInfractionId;
    private long _tick;
    private static string? _pendingNotice;
    public bool IsNetworked => _networked;
    public bool HasLocalPlayer => _connected && !_server && _clientActors.ContainsKey(Multiplayer.GetUniqueId());
    public bool IsStartingLocalHost => _localServerProcess is not null && !_networked;
    public string SessionStatus { get; private set; } = "Offline. Choose how to play.";

    private sealed class ServerActor(PlayerController player, MissionSession mission)
    {
        public PlayerController Player { get; } = player;
        public MissionSession Mission { get; } = mission;
        public Vector2 Input { get; set; }
        public bool JumpPending { get; set; }
        public float Yaw { get; set; }
        public long LastInputTick { get; set; }
        public HashSet<string> UsedSpots { get; } = new(StringComparer.Ordinal);
    }

    public override void _Ready()
    {
        if (_pendingNotice is not null)
        {
            SessionStatus = _pendingNotice;
            _pendingNotice = null;
        }
        string[] args = OS.GetCmdlineUserArgs();
        bool startServer = args.Contains("--server", StringComparer.Ordinal);
        string? connectArg = args.FirstOrDefault(arg => arg.StartsWith("--connect=", StringComparison.Ordinal));
        _startedFromCommandLine = startServer || connectArg is not null;
        if (!startServer && connectArg is null)
        {
            SetPhysicsProcess(false);
            return;
        }

        if (startServer && connectArg is not null)
        {
            GD.PushError("Use either --server or --connect, not both.");
            SetPhysicsProcess(false);
            return;
        }

        int port = DefaultPort;
        string? portArg = args.FirstOrDefault(arg => arg.StartsWith("--port=", StringComparison.Ordinal));
        if (portArg is not null && (!int.TryParse(portArg[7..], out port) || port is < 1024 or > 65535))
        {
            GD.PushError("Network port must be between 1024 and 65535.");
            SetPhysicsProcess(false);
            return;
        }

        if (!StartNetwork(startServer, connectArg is null ? "" : connectArg[10..], port))
        {
            GD.PushError(SessionStatus);
            GetTree().Quit(1);
        }
    }

    public void SetLocalStatus(string status) => SessionStatus = status;

    public bool StartClient(string address, int port)
    {
        if (string.IsNullOrWhiteSpace(address) || address.Contains(' ') || port is < 1024 or > 65535)
        {
            SessionStatus = "Enter a server address and a UDP port from 1024 to 65535.";
            return false;
        }
        return StartNetwork(false, address, port);
    }

    public bool StartLocalHost(int port)
    {
        if (_networked || _localServerProcess is not null || port is < 1024 or > 65535)
        {
            return false;
        }

        try
        {
            ProcessStartInfo start = new(OS.GetExecutablePath())
            {
                WorkingDirectory = ProjectSettings.GlobalizePath("res://"),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            foreach (string argument in new[] { "--headless", "--path", start.WorkingDirectory, "--", "--server", $"--port={port}" })
            {
                start.ArgumentList.Add(argument);
            }
            _localServerReady = false;
            _localServerProcess = new Process { StartInfo = start };
            _localServerProcess.OutputDataReceived += (_, args) =>
            {
                if (args.Data?.Contains("Network server ready", StringComparison.Ordinal) == true)
                {
                    _localServerReady = true;
                }
            };
            _localServerProcess.Start();
            _localServerProcess.BeginOutputReadLine();
            _localServerProcess.BeginErrorReadLine();
            _localServerStartMs = Time.GetTicksMsec();
            _localServerPort = port;
            SessionStatus = $"Starting local server on UDP {port}...";
            SetProcess(true);
            return true;
        }
        catch (Exception error)
        {
            StopLocalServer();
            SessionStatus = $"Local server could not start: {error.Message}";
            return false;
        }
    }

    public void LeaveSession()
    {
        if (!_networked && _localServerProcess is null)
        {
            return;
        }
        GetTree().ReloadCurrentScene();
    }

    private bool StartNetwork(bool startServer, string address, int port)
    {
        if (_networked)
        {
            SessionStatus = "Already connected to a session.";
            return false;
        }

        ENetMultiplayerPeer peer = new();
        Error error = startServer ? peer.CreateServer(port, MaxClients) : peer.CreateClient(address, port);
        if (error != Error.Ok)
        {
            SessionStatus = $"ENet startup failed: {error}";
            peer.Close();
            return false;
        }

        _peer = peer;
        _networked = true;
        _server = startServer;
        _connected = startServer;
        _connectionStartMs = Time.GetTicksMsec();
        _playerScene = GD.Load<PackedScene>("res://scenes/characters/Player.tscn");
        _catalog = MissionCatalog.FromJson(Godot.FileAccess.GetFileAsString("res://resources/missions/initial.json"));
        _clientAssigned = _catalog.Definitions;
        _spots = GetNode("../MissionSpots").GetChildren().OfType<MissionSpot>().ToArray();
        _hud = GetNode<MissionHud>("../MissionHud");
        GetNode<LocalMissionController>("../MissionRuntime").ProcessMode = ProcessModeEnum.Disabled;
        GetNode<PlayerController>("../Player").QueueFree();
        _playersRoot = GetNode<Node3D>("../NetworkPlayers");

        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.ConnectionFailed += OnConnectionFailed;
        Multiplayer.ServerDisconnected += OnServerDisconnected;

        Multiplayer.MultiplayerPeer = _peer;
        _hud.SetPrompt("");
        SessionStatus = startServer ? $"Server listening on UDP {port}" : $"Connecting to {address}:{port}";
        _hud.SetFeedback(SessionStatus);
        _hud.RenderNetwork(_clientAssigned, _localCompleted, 0);
        GD.Print(startServer ? $"Network server ready on UDP {port}" : $"Network client connecting to {address}:{port}");
        SetPhysicsProcess(true);
        return true;
    }

    public override void _ExitTree()
    {
        if (_networked)
        {
            Multiplayer.MultiplayerPeer = null;
            _peer?.Close();
        }
        StopLocalServer();
    }

    private void StopLocalServer()
    {
        if (_localServerProcess is null)
        {
            return;
        }
        try
        {
            if (!_localServerProcess.HasExited)
            {
                _localServerProcess.Kill();
            }
        }
        catch (InvalidOperationException)
        {
            // Process.Start may fail before a process ID exists.
        }
        _localServerProcess.Dispose();
        _localServerProcess = null;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_networked || _peer is null)
        {
            return;
        }

        _tick++;
        if (_server)
        {
            foreach (ServerActor actor in _serverActors.Values)
            {
                if (_tick - actor.LastInputTick > 15)
                {
                    actor.Input = Vector2.Zero;
                }
                actor.Player.SimulateMovement(actor.Input, actor.JumpPending, actor.Yaw, delta);
                actor.JumpPending = false;
            }

            while (_pendingInteractions.TryDequeue(out (int PeerId, string SpotName) request))
            {
                ResolveInteraction(request.PeerId, request.SpotName);
            }

            if (++_snapshotCounter >= 3)
            {
                _snapshotCounter = 0;
                foreach ((int observerId, ServerActor observer) in _serverActors)
                {
                    foreach ((int targetId, ServerActor target) in _serverActors)
                    {
                        bool visible = observerId == targetId || CanSee(observer.Player, target.Player);
                        (int Observer, int Target) key = (observerId, targetId);
                        if (!_knownVisibility.TryGetValue(key, out bool previous) || previous != visible)
                        {
                            _knownVisibility[key] = visible;
                            RpcId(observerId, MethodName.SetActorVisible, targetId, visible,
                                visible ? target.Player.GlobalPosition : Vector3.Zero);
                        }
                        if (visible)
                        {
                            RpcId(observerId, MethodName.ReceivePosition, targetId, target.Player.GlobalPosition);
                        }
                    }
                    foreach (MissionSpot spot in _spots)
                    {
                        if (observer.Player.GlobalPosition.DistanceTo(spot.GlobalPosition) > 30f ||
                            !HasLineOfSight(observer.Player, spot))
                        {
                            continue;
                        }
                        int count = _spotUses.GetValueOrDefault(spot.Name);
                        (int Peer, string Spot) key = (observerId, spot.Name);
                        if (!_knownSpotCounts.TryGetValue(key, out int previous) || previous != count)
                        {
                            _knownSpotCounts[key] = count;
                            RpcId(observerId, MethodName.SyncSpotState, spot.Name, count);
                        }
                    }
                }
            }
            return;
        }

        int localId = Multiplayer.GetUniqueId();
        if (!_clientActors.TryGetValue(localId, out PlayerController? localPlayer))
        {
            return;
        }

        Vector2 input = localPlayer.InputEnabled
            ? Input.GetVector("move_left", "move_right", "move_forward", "move_back")
            : Vector2.Zero;
        RpcId(1, MethodName.SubmitMovement, input, localPlayer.InputEnabled && Input.IsActionJustPressed("jump"), localPlayer.LookYaw);

        if (!localPlayer.InputEnabled)
        {
            _hud.SetPrompt("");
            return;
        }

        MissionSpot? nearest = _spots
            .Where(spot => localPlayer.GlobalPosition.DistanceTo(spot.GlobalPosition) <= spot.InteractionDistance)
            .OrderBy(spot => localPlayer.GlobalPosition.DistanceTo(spot.GlobalPosition))
            .FirstOrDefault();
        MissionDefinition? assigned = nearest is null ? null : _clientAssigned.FirstOrDefault(definition => definition.Id == nearest.MissionId);
        _hud.SetPrompt(nearest is null ? "" : $"E: {assigned?.Instruction ?? nearest.ActionLabel}");
        if (nearest is not null && Input.IsActionJustPressed("interact"))
        {
            RpcId(1, MethodName.SubmitInteraction, nearest.Name);
        }
    }

    public override void _Process(double delta)
    {
        if (_networked && !_server && !_connected && Time.GetTicksMsec() - _connectionStartMs > 5000)
        {
            HandleConnectionEnd("Connection timed out after 5 seconds. Choose Host or Join to retry.");
            SetProcess(false);
            return;
        }

        if (IsStartingLocalHost)
        {
            if (_localServerProcess!.HasExited || Time.GetTicksMsec() - _localServerStartMs > 10000)
            {
                StopLocalServer();
                SessionStatus = "Local server did not become ready within 10 seconds.";
            }
            else if (_localServerReady && !StartClient("127.0.0.1", _localServerPort))
            {
                StopLocalServer();
            }
        }

        if (!_networked || _server)
        {
            return;
        }

        foreach ((int id, Vector3 target) in _clientTargets)
        {
            if (id == Multiplayer.GetUniqueId() || !_clientActors.TryGetValue(id, out PlayerController? player))
            {
                continue;
            }

            player.GlobalPosition = player.GlobalPosition.DistanceTo(target) > 5f
                ? target
                : player.GlobalPosition.Lerp(target, Mathf.Clamp((float)delta * 12f, 0f, 1f));
        }
    }

    private void OnConnectedToServer()
    {
        _connected = true;
        SessionStatus = $"Connected as peer {Multiplayer.GetUniqueId()}";
        _hud.SetFeedback("Connected. Waiting for player spawn.");
        GD.Print($"Connected as peer {Multiplayer.GetUniqueId()}");
    }

    private void OnConnectionFailed()
    {
        HandleConnectionEnd("Connection failed. Choose Host or Join to try again.");
    }

    private void OnServerDisconnected()
    {
        HandleConnectionEnd("Server disconnected. Choose Host or Join to play again.");
    }

    private void HandleConnectionEnd(string reason)
    {
        if (_endingSession)
        {
            return;
        }
        _endingSession = true;
        if (_startedFromCommandLine)
        {
            GD.PushError(reason);
            GetTree().Quit(1);
            return;
        }
        _pendingNotice = reason;
        GetTree().CallDeferred(SceneTree.MethodName.ReloadCurrentScene);
    }

    private void OnPeerConnected(long peerId)
    {
        if (!_server)
        {
            return;
        }

        int id = checked((int)peerId);
        Vector3 spawn = new(35f - _serverActors.Count * 2.5f, 0.05f, 25.7f);
        PlayerController player = CreateAvatar(id, spawn, false);
        int slot = _serverActors.Count;
        MissionDefinition[] assigned = slot == 0
            ? _catalog.Definitions.ToArray()
            : _catalog.Definitions.Where((_, index) => index != (slot - 1) % _catalog.Definitions.Count).ToArray();
        _serverActors[id] = new ServerActor(player, new MissionSession(id.ToString(), assigned));
        foreach ((int existingId, ServerActor existing) in _serverActors)
        {
            bool visible = existingId == id || CanSee(player, existing.Player);
            _knownVisibility[(id, existingId)] = visible;
            RpcId(id, MethodName.SpawnActor, existingId,
                visible ? existing.Player.GlobalPosition : Vector3.Zero, visible);
            if (existingId != id)
            {
                visible = CanSee(existing.Player, player);
                _knownVisibility[(existingId, id)] = visible;
                RpcId(existingId, MethodName.SpawnActor, id,
                    visible ? player.GlobalPosition : Vector3.Zero, visible);
            }
        }
        Godot.Collections.Array<string> assignedIds = [];
        foreach (MissionDefinition definition in assigned)
        {
            assignedIds.Add(definition.Id);
        }
        RpcId(id, MethodName.SetAssignments, assignedIds);
        GD.Print($"Server spawned peer {id}");
    }

    private void OnPeerDisconnected(long peerId)
    {
        if (!_server)
        {
            return;
        }

        int id = checked((int)peerId);
        if (_serverActors.Remove(id, out ServerActor? actor))
        {
            actor.Player.QueueFree();
            Rpc(MethodName.RemoveActor, id);
            foreach ((int Observer, int Target) key in _knownVisibility.Keys.Where(key => key.Observer == id || key.Target == id).ToArray())
            {
                _knownVisibility.Remove(key);
            }
            foreach ((int Peer, string Spot) key in _knownSpotCounts.Keys.Where(key => key.Peer == id).ToArray())
            {
                _knownSpotCounts.Remove(key);
            }
            GD.Print($"Server removed peer {id}");
        }
    }

    private PlayerController CreateAvatar(int id, Vector3 position, bool local)
    {
        PlayerController player = _playerScene.Instantiate<PlayerController>();
        player.Name = $"Peer_{id}";
        player.IsLocallyControlled = local;
        player.ExternalMovement = true;
        _playersRoot.AddChild(player);
        player.GlobalPosition = position;
        return player;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SpawnActor(int id, Vector3 position, bool visible)
    {
        if (_server || _clientActors.ContainsKey(id))
        {
            return;
        }

        bool local = id == Multiplayer.GetUniqueId();
        _clientActors[id] = CreateAvatar(id, position, local);
        _clientActors[id].Visible = visible;
        if (visible)
        {
            _clientTargets[id] = position;
        }
        if (local)
        {
            _hud.SetFeedback($"Playing as peer {id}");
        }
        GD.Print($"Client spawned peer {id}{(local ? " (local)" : "")}");
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SetAssignments(Godot.Collections.Array<string> missionIds)
    {
        _clientAssigned = _catalog.Definitions.Where(definition => missionIds.Contains(definition.Id)).ToArray();
        _hud.RenderNetwork(_clientAssigned, _localCompleted, 0);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RemoveActor(int id)
    {
        if (_clientActors.Remove(id, out PlayerController? player))
        {
            player.QueueFree();
            _clientTargets.Remove(id);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SetActorVisible(int id, bool visible, Vector3 position)
    {
        if (!_clientActors.TryGetValue(id, out PlayerController? player))
        {
            return;
        }

        player.Visible = visible;
        if (visible)
        {
            player.GlobalPosition = position;
            _clientTargets[id] = position;
        }
        else
        {
            _clientTargets.Remove(id);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncSpotState(string spotName, int count)
    {
        _spots.FirstOrDefault(spot => spot.Name == spotName)?.SetUseCount(count);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
    private void SubmitMovement(Vector2 input, bool jump, float yaw)
    {
        if (!_server || !_serverActors.TryGetValue(Multiplayer.GetRemoteSenderId(), out ServerActor? actor) ||
            !float.IsFinite(input.X) || !float.IsFinite(input.Y) || !float.IsFinite(yaw))
        {
            return;
        }

        actor.Input = input.LimitLength(1f);
        actor.JumpPending |= jump;
        actor.Yaw = Mathf.Wrap(yaw, -Mathf.Pi, Mathf.Pi);
        actor.LastInputTick = _tick;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
    private void ReceivePosition(int id, Vector3 position)
    {
        if (!_clientActors.TryGetValue(id, out PlayerController? player))
        {
            return;
        }
        if (!player.Visible)
        {
            return;
        }

        if (id == Multiplayer.GetUniqueId())
        {
            // Local prediction awaits latency measurement; the server position is authoritative.
            player.GlobalPosition = position;
        }
        else
        {
            _clientTargets[id] = position;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SubmitInteraction(string spotName)
    {
        if (_server)
        {
            _pendingInteractions.Enqueue((Multiplayer.GetRemoteSenderId(), spotName));
        }
    }

    private void ResolveInteraction(int peerId, string spotName)
    {
        if (!_serverActors.TryGetValue(peerId, out ServerActor? actor))
        {
            return;
        }

        MissionSpot? spot = _spots.FirstOrDefault(candidate => candidate.Name == spotName);
        if (spot is null)
        {
            RpcId(peerId, MethodName.InteractionResult, spotName, (int)MissionActionStatus.UnknownMission, actor.Mission.CompletedCount, actor.Mission.CompletionPoints, 0);
            return;
        }

        float distance = actor.Player.GlobalPosition.DistanceTo(spot.GlobalPosition);
        bool visible = HasLineOfSight(actor.Player, spot);
        if (distance > spot.InteractionDistance || !visible)
        {
            MissionActionStatus status = distance > spot.InteractionDistance ? MissionActionStatus.TooFar : MissionActionStatus.Obstructed;
            RpcId(peerId, MethodName.InteractionResult, spot.MissionId, (int)status, actor.Mission.CompletedCount, actor.Mission.CompletionPoints, 0);
            return;
        }

        if (!actor.UsedSpots.Add(spotName))
        {
            RpcId(peerId, MethodName.InteractionResult, spot.MissionId,
                (int)MissionActionStatus.AlreadyCompleted, actor.Mission.CompletedCount, actor.Mission.CompletionPoints, 0);
            return;
        }

        MissionAction action = new(peerId.ToString(), spot.MissionId, spot.LocationId, spot.ActionId, _tick);
        MissionAttempt outcome = actor.Mission.TryComplete(action, new InteractionValidation(distance, spot.InteractionDistance, true));
        // The server journal owns the action record; the M03 session only grants mission progress.
        int eventId = ++_nextInfractionId;
        int count = _spotUses.GetValueOrDefault(spotName) + 1;
        _spotUses[spotName] = count;
        spot.SetUseCount(count);
        string category = _catalog.Definitions.First(definition => definition.Id == spot.MissionId).InfractionCategory;
        _serverInfractions.Add(new InfractionEvent(
            eventId, peerId.ToString(), outcome.Succeeded ? spot.MissionId : string.Empty,
            category, spot.LocationId, spot.ActionId, _tick));
        GD.Print($"Server infraction #{eventId}: peer {peerId}, {spot.ActionId}, mission {outcome.Status}");
        RpcId(peerId, MethodName.InteractionResult, spot.MissionId, (int)outcome.Status, actor.Mission.CompletedCount, actor.Mission.CompletionPoints, eventId);
    }

    private static bool HasLineOfSight(PlayerController player, MissionSpot spot)
    {
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(
            player.GlobalPosition + Vector3.Up * 1.1f,
            spot.GlobalPosition + Vector3.Up * 0.7f);
        query.Exclude = new Godot.Collections.Array<Rid> { player.GetRid() };
        Godot.Collections.Dictionary result = player.GetWorld3D().DirectSpaceState.IntersectRay(query);
        if (result.Count == 0)
        {
            return true;
        }

        GodotObject? collider = result["collider"].AsGodotObject();
        return collider is Node node && (node == spot || spot.IsAncestorOf(node));
    }

    private static bool CanSee(PlayerController observer, PlayerController target)
    {
        if (observer.GlobalPosition.DistanceTo(target.GlobalPosition) > 40f)
        {
            return false;
        }

        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(
            observer.GlobalPosition + Vector3.Up * 1.2f,
            target.GlobalPosition + Vector3.Up * 1.2f);
        query.Exclude = new Godot.Collections.Array<Rid> { observer.GetRid() };
        Godot.Collections.Dictionary result = observer.GetWorld3D().DirectSpaceState.IntersectRay(query);
        return result.Count == 0 || result["collider"].AsGodotObject() == target;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void InteractionResult(string missionId, int statusValue, int completed, int points, int eventId)
    {
        MissionActionStatus status = (MissionActionStatus)statusValue;
        if (status == MissionActionStatus.Completed)
        {
            _localCompleted.Add(missionId);
        }
        _hud.RenderNetwork(_clientAssigned, _localCompleted, points);
        _hud.SetFeedback(eventId == 0
            ? $"Action rejected: {status}"
            : status == MissionActionStatus.Completed
                ? $"Mission completed; infraction #{eventId} recorded."
                : $"Infraction #{eventId} recorded; mission unchanged ({status}).");
        GD.Print($"Peer {Multiplayer.GetUniqueId()} interaction result: {status}, progress {completed}, event {eventId}");
    }
}
