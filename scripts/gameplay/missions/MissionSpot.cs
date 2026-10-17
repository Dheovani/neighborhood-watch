using Godot;

namespace NeighborhoodWatch.Gameplay.Missions;

/// <summary>A scene location where one configured infraction can be performed.</summary>
public partial class MissionSpot : Node3D
{
    [Export] public string MissionId { get; set; } = string.Empty;
    [Export] public string LocationId { get; set; } = string.Empty;
    [Export] public string ActionId { get; set; } = string.Empty;
    [Export] public float InteractionDistance { get; set; } = 2.6f;
}
