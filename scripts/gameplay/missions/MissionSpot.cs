using Godot;

namespace NeighborhoodWatch.Gameplay.Missions;

/// <summary>A scene location where one configured infraction can be performed.</summary>
public partial class MissionSpot : Node3D
{
    [Export] public string MissionId { get; set; } = string.Empty;
    [Export] public string LocationId { get; set; } = string.Empty;
    [Export] public string ActionId { get; set; } = string.Empty;
    [Export] public string InfractionCategory { get; set; } = string.Empty;
    [Export] public string ActionLabel { get; set; } = string.Empty;
    [Export] public float InteractionDistance { get; set; } = 2.6f;
    [Export] public int MaxUses { get; set; }
    [Export] public string EffectNodePath { get; set; } = string.Empty;
    [Export] public bool HideEffectWhenDepleted { get; set; }
    public int UseCount { get; private set; }
    public bool IsDepleted => MaxUses > 0 && UseCount >= MaxUses;

    private Label3D? _label;
    private string _baseLabel = string.Empty;
    private Node3D? _effect;

    public override void _Ready()
    {
        _label = GetChildren().OfType<Label3D>().FirstOrDefault();
        _baseLabel = _label?.Text ?? string.Empty;
        _effect = string.IsNullOrEmpty(EffectNodePath) ? null : GetNodeOrNull<Node3D>(EffectNodePath);
        SetUseCount(0);
    }

    public void SetUseCount(int count)
    {
        UseCount = count;
        if (_label is not null)
        {
            _label.Text = MaxUses > 0
                ? $"{_baseLabel} ({Math.Max(0, MaxUses - count)} left)"
                : count == 0 ? _baseLabel : $"{_baseLabel} ({count})";
        }
        if (_effect is not null)
        {
            _effect.Visible = HideEffectWhenDepleted ? !IsDepleted : count > 0;
        }
    }
}
