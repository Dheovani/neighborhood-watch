using Godot;

namespace NeighborhoodWatch.Characters;

/// <summary>Local M01 movement prototype. Authority moves to the server in M04.</summary>
public partial class PlayerController : CharacterBody3D
{
    [Export] public float MoveSpeed { get; set; } = 5.0f;
    [Export] public float JumpSpeed { get; set; } = 4.5f;
    [Export] public float MouseSensitivity { get; set; } = 0.12f;

    private Node3D _cameraYaw = null!;
    private Node3D _cameraPitch = null!;
    private float _pitchDegrees;

    public override void _Ready()
    {
        _cameraYaw = GetNode<Node3D>("CameraYaw");
        _cameraPitch = GetNode<Node3D>("CameraYaw/CameraPitch");
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
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
        Vector2 input = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        Vector3 direction = _cameraYaw.GlobalBasis * new Vector3(input.X, 0f, input.Y);
        direction.Y = 0f;
        direction = direction.Normalized();

        Vector3 velocity = Velocity;
        velocity.X = direction.X * MoveSpeed;
        velocity.Z = direction.Z * MoveSpeed;

        if (IsOnFloor())
        {
            velocity.Y = Input.IsActionJustPressed("jump") ? JumpSpeed : 0f;
        }
        else
        {
            velocity.Y -= 9.8f * (float)delta;
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}
