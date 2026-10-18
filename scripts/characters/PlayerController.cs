using Godot;

namespace NeighborhoodWatch.Characters;

/// <summary>Character movement shared by offline play and the authoritative server.</summary>
public partial class PlayerController : CharacterBody3D
{
    [Export] public float MoveSpeed { get; set; } = 5.0f;
    [Export] public float JumpSpeed { get; set; } = 4.5f;
    [Export] public float MouseSensitivity { get; set; } = 0.12f;
    public bool IsLocallyControlled { get; set; } = true;
    public bool InputEnabled { get; set; } = true;
    public bool ExternalMovement { get; set; }
    public float LookYaw => _cameraYaw.Rotation.Y;

    private Node3D _cameraYaw = null!;
    private Node3D _cameraPitch = null!;
    private float _pitchDegrees;

    public override void _Ready()
    {
        _cameraYaw = GetNode<Node3D>("CameraYaw");
        _cameraPitch = GetNode<Node3D>("CameraYaw/CameraPitch");
        GetNode<Camera3D>("CameraYaw/CameraPitch/SpringArm3D/Camera3D").Current = IsLocallyControlled;
        if (IsLocallyControlled)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!IsLocallyControlled || !InputEnabled)
        {
            return;
        }
        if (@event.IsActionPressed("toggle_mouse"))
        {
            Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
                ? Input.MouseModeEnum.Visible
                : Input.MouseModeEnum.Captured;
            GetViewport().SetInputAsHandled();
            return;
        }

        if (@event is InputEventMouseButton button && button.Pressed &&
            Input.MouseMode == Input.MouseModeEnum.Visible)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }

        if (@event is not InputEventMouseMotion motion ||
            Input.MouseMode != Input.MouseModeEnum.Captured)
        {
            return;
        }

        ApplyMouseLook(motion.Relative);
    }

    public void ApplyMouseLook(Vector2 relative)
    {
        _cameraYaw.RotateY(Mathf.DegToRad(-relative.X * MouseSensitivity));
        _pitchDegrees = Mathf.Clamp(_pitchDegrees - relative.Y * MouseSensitivity, -65f, 55f);
        _cameraPitch.RotationDegrees = new Vector3(_pitchDegrees, 0f, 0f);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (ExternalMovement || !InputEnabled)
        {
            return;
        }

        Vector2 input = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        SimulateMovement(input, Input.IsActionJustPressed("jump"), _cameraYaw.Rotation.Y, delta);
    }

    public void SimulateMovement(Vector2 input, bool jump, float yaw, double delta)
    {
        _cameraYaw.Rotation = new Vector3(0f, yaw, 0f);
        Vector3 direction = _cameraYaw.GlobalBasis * new Vector3(input.X, 0f, input.Y);
        direction.Y = 0f;
        direction = direction.Normalized();

        Vector3 velocity = Velocity;
        velocity.X = direction.X * MoveSpeed;
        velocity.Z = direction.Z * MoveSpeed;

        if (IsOnFloor())
        {
            velocity.Y = jump ? JumpSpeed : 0f;
        }
        else
        {
            velocity.Y -= 9.8f * (float)delta;
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}
