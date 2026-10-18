using Godot;
using NeighborhoodWatch.Characters;
using NeighborhoodWatch.Networking;

namespace NeighborhoodWatch.Ui;

/// <summary>Small development menu for starting or joining a local network session.</summary>
public partial class ConnectionMenu : CanvasLayer
{
    private NetworkSession _session = null!;
    private Control _panel = null!;
    private LineEdit _address = null!;
    private LineEdit _port = null!;
    private Label _status = null!;
    private Button _offline = null!;
    private Button _host = null!;
    private Button _join = null!;
    private Button _leave = null!;
    private bool _hideWhenStarted;

    public override void _Ready()
    {
        _session = GetNode<NetworkSession>("../NetworkSession");
        _panel = GetNode<Control>("Panel");
        _address = GetNode<LineEdit>("Panel/Layout/Address");
        _port = GetNode<LineEdit>("Panel/Layout/Port");
        _status = GetNode<Label>("Panel/Layout/Status");
        _offline = GetNode<Button>("Panel/Layout/Offline");
        _host = GetNode<Button>("Panel/Layout/Host");
        _join = GetNode<Button>("Panel/Layout/Join");
        _leave = GetNode<Button>("Panel/Layout/Leave");

        _offline.Pressed += HideMenu;
        _host.Pressed += Host;
        _join.Pressed += Join;
        _leave.Pressed += () => _session.LeaveSession();
        if (OS.GetCmdlineUserArgs().Any(arg => arg == "--server" || arg.StartsWith("--connect=", StringComparison.Ordinal)))
        {
            _panel.Hide();
        }
        else
        {
            ShowMenu();
        }
    }

    public override void _Process(double delta)
    {
        _status.Text = _session.SessionStatus;
        _leave.Visible = _session.IsNetworked;
        _offline.Visible = !_session.IsNetworked;
        _host.Disabled = _session.IsNetworked || _session.IsStartingLocalHost;
        _join.Disabled = _session.IsNetworked || _session.IsStartingLocalHost;
        if (_hideWhenStarted && _session.HasLocalPlayer && _panel.Visible)
        {
            HideMenu();
            _hideWhenStarted = false;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Tab })
        {
            if (_panel.Visible)
            {
                HideMenu();
            }
            else
            {
                ShowMenu();
            }
            GetViewport().SetInputAsHandled();
        }
    }

    public void ShowMenu()
    {
        _panel.Show();
        SetPlayerInput(false);
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void HideMenu()
    {
        _panel.Hide();
        SetPlayerInput(true);
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    private void SetPlayerInput(bool enabled)
    {
        PlayerController? offlinePlayer = GetNodeOrNull<PlayerController>("../Player");
        if (offlinePlayer is not null)
        {
            offlinePlayer.InputEnabled = enabled;
        }
        foreach (PlayerController player in GetNode<Node3D>("../NetworkPlayers").GetChildren().OfType<PlayerController>())
        {
            if (player.IsLocallyControlled)
            {
                player.InputEnabled = enabled;
            }
        }
    }

    private bool TryPort(out int port)
    {
        if (int.TryParse(_port.Text, out port) && port is >= 1024 and <= 65535)
        {
            return true;
        }
        _session.SetLocalStatus("Port must be between 1024 and 65535.");
        return false;
    }

    private void Host()
    {
        if (TryPort(out int port) && _session.StartLocalHost(port))
        {
            _hideWhenStarted = true;
        }
    }

    private void Join()
    {
        if (TryPort(out int port) && _session.StartClient(_address.Text.Trim(), port))
        {
            _hideWhenStarted = true;
        }
    }
}
