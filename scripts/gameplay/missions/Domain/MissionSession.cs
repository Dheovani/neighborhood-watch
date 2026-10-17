namespace NeighborhoodWatch.Gameplay.Missions.Domain;

public sealed class MissionSession
{
    private readonly Dictionary<string, MissionDefinition> _assigned;
    private readonly HashSet<string> _completed = new(StringComparer.Ordinal);
    private readonly List<InfractionEvent> _events = [];

    public string ActorId { get; }
    public IReadOnlyList<MissionDefinition> Assigned { get; }
    public IReadOnlyList<InfractionEvent> Events => _events;
    public int CompletedCount => _completed.Count;
    public int ProgressTarget => Assigned.Count;
    public int CompletionPoints { get; private set; }

    public MissionSession(string actorId, MissionCatalog catalog)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actorId);
        ArgumentNullException.ThrowIfNull(catalog);
        ActorId = actorId;
        Assigned = catalog.Definitions;
        _assigned = Assigned.ToDictionary(definition => definition.Id, StringComparer.Ordinal);
    }

    public bool IsCompleted(string missionId) => _completed.Contains(missionId);

    public MissionAttempt TryComplete(MissionAction action, InteractionValidation validation)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(validation);

        if (!_assigned.TryGetValue(action.MissionId, out MissionDefinition? mission))
        {
            return new MissionAttempt(MissionActionStatus.UnknownMission);
        }
        if (_completed.Contains(action.MissionId))
        {
            return new MissionAttempt(MissionActionStatus.AlreadyCompleted);
        }
        if (!StringComparer.Ordinal.Equals(action.ActorId, ActorId))
        {
            return new MissionAttempt(MissionActionStatus.WrongActor);
        }
        if (!StringComparer.Ordinal.Equals(action.LocationId, mission.LocationId))
        {
            return new MissionAttempt(MissionActionStatus.WrongLocation);
        }
        if (!mission.OffenseActions.Contains(action.ActionId, StringComparer.Ordinal))
        {
            return new MissionAttempt(MissionActionStatus.WrongAction);
        }
        if (!float.IsFinite(validation.DistanceMeters) ||
            !float.IsFinite(validation.MaximumDistanceMeters) ||
            validation.DistanceMeters < 0 || validation.MaximumDistanceMeters <= 0 ||
            validation.DistanceMeters > validation.MaximumDistanceMeters)
        {
            return new MissionAttempt(MissionActionStatus.TooFar);
        }
        if (!validation.HasLineOfSight)
        {
            return new MissionAttempt(MissionActionStatus.Obstructed);
        }

        InfractionEvent infraction = new(
            _events.Count + 1,
            ActorId,
            mission.Id,
            mission.InfractionCategory,
            mission.LocationId,
            action.ActionId,
            action.Tick);
        _completed.Add(mission.Id);
        _events.Add(infraction);
        CompletionPoints += mission.CompletionPoints;
        return new MissionAttempt(MissionActionStatus.Completed, infraction);
    }
}
