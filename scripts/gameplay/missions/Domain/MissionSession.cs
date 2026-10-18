namespace NeighborhoodWatch.Gameplay.Missions.Domain;

public sealed class MissionSession
{
    private readonly Dictionary<string, MissionDefinition> _assigned;
    private readonly HashSet<string> _completed = new(StringComparer.Ordinal);
    private readonly List<InfractionEvent> _events = [];
    private readonly HashSet<int> _consumedEventIds = [];

    public string ActorId { get; }
    public IReadOnlyList<MissionDefinition> Assigned { get; }
    public IReadOnlyList<InfractionEvent> Events => _events;
    public int CompletedCount => _completed.Count;
    public int ProgressTarget => Assigned.Count;
    public int CompletionPoints { get; private set; }

    public MissionSession(string actorId, MissionCatalog catalog)
        : this(actorId, catalog?.Definitions ?? throw new ArgumentNullException(nameof(catalog)))
    {
    }

    public MissionSession(string actorId, IEnumerable<MissionDefinition> assigned)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actorId);
        ArgumentNullException.ThrowIfNull(assigned);
        ActorId = actorId;
        Assigned = assigned.ToArray();
        _assigned = Assigned.ToDictionary(definition => definition.Id, StringComparer.Ordinal);
    }

    public bool IsCompleted(string missionId) => _completed.Contains(missionId);

    /// <summary>Applies an event already validated and recorded by the authoritative action owner.</summary>
    public MissionAttempt ConsumeValidatedInfraction(InfractionEvent infraction)
    {
        ArgumentNullException.ThrowIfNull(infraction);
        if (infraction.Sequence <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(infraction), "Event sequence must be positive.");
        }
        if (!StringComparer.Ordinal.Equals(infraction.ActorId, ActorId))
        {
            return new MissionAttempt(MissionActionStatus.WrongActor);
        }
        if (_consumedEventIds.Contains(infraction.Sequence))
        {
            return new MissionAttempt(MissionActionStatus.AlreadyCompleted);
        }

        MissionDefinition? matching = Assigned.FirstOrDefault(mission =>
            (string.IsNullOrEmpty(infraction.MissionId) || mission.Id == infraction.MissionId) &&
            mission.LocationId == infraction.LocationId &&
            mission.InfractionCategory == infraction.Category &&
            mission.OffenseActions.Contains(infraction.ActionId, StringComparer.Ordinal));
        if (matching is null)
        {
            return new MissionAttempt(MissionActionStatus.UnknownMission);
        }
        if (_completed.Contains(matching.Id))
        {
            return new MissionAttempt(MissionActionStatus.AlreadyCompleted);
        }

        _consumedEventIds.Add(infraction.Sequence);
        _completed.Add(matching.Id);
        _events.Add(infraction);
        CompletionPoints += matching.CompletionPoints;
        return new MissionAttempt(MissionActionStatus.Completed, infraction, matching.Id);
    }

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
        return new MissionAttempt(MissionActionStatus.Completed, infraction, mission.Id);
    }
}
