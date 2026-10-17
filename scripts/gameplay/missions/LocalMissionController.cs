using Godot;
using NeighborhoodWatch.Characters;
using NeighborhoodWatch.Gameplay.Missions.Domain;
using NeighborhoodWatch.Ui;

namespace NeighborhoodWatch.Gameplay.Missions;

/// <summary>Offline M03 adapter. M04 will move action validation and session ownership to the server.</summary>
public partial class LocalMissionController : Node
{
    private const string LocalActorId = "local-player";
    private PlayerController _player = null!;
    private MissionHud _hud = null!;
    private MissionSpot[] _spots = [];
    private MissionSpot? _selected;
    private MissionSession? _session;
    private long _tick;

    public MissionSession Session => _session ?? throw new InvalidOperationException("Mission session is not ready.");
    public bool IsReady => _session is not null;

    public override void _Ready()
    {
        try
        {
            _player = GetNode<PlayerController>("../Player");
            _hud = GetNode<MissionHud>("../MissionHud");
            Node spotsRoot = GetNode("../MissionSpots");
            _spots = spotsRoot.GetChildren().OfType<MissionSpot>().ToArray();

            string json = Godot.FileAccess.GetFileAsString("res://resources/missions/initial.json");
            MissionCatalog catalog = MissionCatalog.FromJson(json);
            foreach (MissionDefinition mission in catalog.Definitions)
            {
                if (!_spots.Any(spot => spot.MissionId == mission.Id &&
                    spot.LocationId == mission.LocationId &&
                    mission.OffenseActions.Contains(spot.ActionId, StringComparer.Ordinal)))
                {
                    throw new InvalidOperationException($"No matching scene spot for mission {mission.Id}.");
                }
            }
            _session = new MissionSession(LocalActorId, catalog);
            _hud.Render(_session);
            _hud.SetFeedback("Visit a marked place. You may leave any mission incomplete.");
        }
        catch (Exception error)
        {
            GD.PushError($"Mission setup failed: {error.Message}");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_session is null)
        {
            return;
        }

        _tick++;
        _selected = FindNearestAvailableSpot();
        _hud.SetPrompt(_selected is null
            ? ""
            : $"E — {_session.Assigned.First(mission => mission.Id == _selected.MissionId).Instruction}");

        if (_selected is not null && Input.IsActionJustPressed("interact"))
        {
            TryInteract(_selected);
        }
    }

    public MissionAttempt TryInteract(MissionSpot spot)
    {
        if (_session is null || !_spots.Contains(spot))
        {
            return new MissionAttempt(MissionActionStatus.UnknownMission);
        }

        float distance = _player.GlobalPosition.DistanceTo(spot.GlobalPosition);
        bool visible = HasLineOfSight(spot);
        MissionAction action = new(LocalActorId, spot.MissionId, spot.LocationId, spot.ActionId, _tick);
        MissionAttempt result = _session.TryComplete(
            action,
            new InteractionValidation(distance, spot.InteractionDistance, visible));

        if (result.Succeeded)
        {
            _hud.Render(_session);
            _hud.SetFeedback($"Mission complete: {_session.Assigned.First(mission => mission.Id == spot.MissionId).Title}");
            GD.Print($"Infraction #{result.Event!.Sequence}: {result.Event.Category} at {result.Event.LocationId}");
        }
        else if (result.Status != MissionActionStatus.AlreadyCompleted)
        {
            _hud.SetFeedback($"Action unavailable: {result.Status}");
        }

        return result;
    }

    private MissionSpot? FindNearestAvailableSpot()
    {
        MissionSpot? nearest = null;
        float closest = float.PositiveInfinity;
        foreach (MissionSpot spot in _spots)
        {
            if (Session.IsCompleted(spot.MissionId))
            {
                continue;
            }

            float distance = _player.GlobalPosition.DistanceTo(spot.GlobalPosition);
            if (distance <= spot.InteractionDistance && distance < closest && HasLineOfSight(spot))
            {
                nearest = spot;
                closest = distance;
            }
        }
        return nearest;
    }

    private bool HasLineOfSight(MissionSpot spot)
    {
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(
            _player.GlobalPosition + Vector3.Up * 1.1f,
            spot.GlobalPosition + Vector3.Up * 0.7f);
        query.Exclude = new Godot.Collections.Array<Rid> { _player.GetRid() };
        Godot.Collections.Dictionary result = _player.GetWorld3D().DirectSpaceState.IntersectRay(query);
        if (result.Count == 0)
        {
            return true;
        }

        GodotObject? collider = result["collider"].AsGodotObject();
        return collider is Node node && (node == spot || spot.IsAncestorOf(node));
    }
}
