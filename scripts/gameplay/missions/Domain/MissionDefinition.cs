namespace NeighborhoodWatch.Gameplay.Missions.Domain;

public sealed record MissionDefinition(
    string Id,
    string Title,
    string Instruction,
    string LocationId,
    IReadOnlyList<string> OffenseActions,
    string InfractionCategory,
    int CompletionPoints);

public sealed record MissionAction(
    string ActorId,
    string MissionId,
    string LocationId,
    string ActionId,
    long Tick);

public sealed record InteractionValidation(
    float DistanceMeters,
    float MaximumDistanceMeters,
    bool HasLineOfSight);

public sealed record InfractionEvent(
    int Sequence,
    string ActorId,
    string MissionId,
    string Category,
    string LocationId,
    string ActionId,
    long Tick);

public enum MissionActionStatus
{
    Completed,
    AlreadyCompleted,
    UnknownMission,
    WrongActor,
    WrongLocation,
    WrongAction,
    TooFar,
    Obstructed,
    Depleted
}

public sealed record MissionAttempt(MissionActionStatus Status, InfractionEvent? Event = null, string? MissionId = null)
{
    public bool Succeeded => Status == MissionActionStatus.Completed;
}
