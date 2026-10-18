using NeighborhoodWatch.Gameplay.Missions.Domain;

try
{
    string json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "initial.json"));
    MissionCatalog catalog = MissionCatalog.FromJson(json);
    Check(catalog.Definitions.Count == 3, "The sample catalog must contain three missions.");

    MissionSession session = new("player-one", catalog);
    Check(session.CompletedCount == 0 && session.CompletionPoints == 0 && session.Events.Count == 0,
        "Abstaining must leave progress, points, and events empty.");

    MissionDefinition mission = catalog.Definitions[0];
    MissionAction validAction = new("player-one", mission.Id, mission.LocationId, mission.OffenseActions[0], 42);
    InteractionValidation validContext = new(1.5f, 2.6f, true);

    CheckStatus(session.TryComplete(validAction with { ActorId = "another-player" }, validContext), MissionActionStatus.WrongActor);
    CheckStatus(session.TryComplete(validAction with { LocationId = "another-place" }, validContext), MissionActionStatus.WrongLocation);
    CheckStatus(session.TryComplete(validAction with { ActionId = "do_nothing" }, validContext), MissionActionStatus.WrongAction);
    CheckStatus(session.TryComplete(validAction, validContext with { DistanceMeters = 4 }), MissionActionStatus.TooFar);
    CheckStatus(session.TryComplete(validAction, validContext with { HasLineOfSight = false }), MissionActionStatus.Obstructed);
    CheckStatus(session.TryComplete(validAction with { MissionId = "unknown" }, validContext), MissionActionStatus.UnknownMission);
    Check(session.CompletedCount == 0 && session.CompletionPoints == 0 && session.Events.Count == 0,
        "Rejected actions must not alter the session.");

    MissionAttempt result = session.TryComplete(validAction, validContext);
    Check(result.Succeeded && result.Event is not null, "A valid infraction must complete its mission and emit an event.");
    InfractionEvent infraction = result.Event ?? throw new InvalidOperationException("Completed action had no infraction event.");
    Check(infraction.ActorId == "player-one" && infraction.MissionId == mission.Id &&
        infraction.LocationId == mission.LocationId && infraction.ActionId == mission.OffenseActions[0] &&
        infraction.Category == mission.InfractionCategory && infraction.Tick == 42 && infraction.Sequence == 1,
        "The emitted infraction must retain the validated action context.");
    Check(session.CompletedCount == 1 && session.CompletionPoints == mission.CompletionPoints && session.Events.Count == 1,
        "Completion, reward, and infraction event must be one outcome.");
    CheckStatus(session.TryComplete(validAction, validContext), MissionActionStatus.AlreadyCompleted);
    Check(session.CompletedCount == 1 && session.CompletionPoints == mission.CompletionPoints && session.Events.Count == 1,
        "Repeated input must not duplicate completion, reward, or event.");

    foreach (MissionDefinition remaining in catalog.Definitions.Skip(1))
    {
        MissionAction action = new("player-one", remaining.Id, remaining.LocationId, remaining.OffenseActions[0], 43);
        CheckStatus(session.TryComplete(action, validContext), MissionActionStatus.Completed);
    }
    Check(session.CompletedCount == 3 && session.Events.Count == 3 && session.CompletionPoints == 40,
        "All three sample missions must complete once with the configured points.");
    Check(session.Events.Select(entry => entry.Sequence).SequenceEqual([1, 2, 3]),
        "Infraction events must have stable session order.");

    bool duplicateRejected = false;
    try
    {
        _ = new MissionCatalog([mission, mission]);
    }
    catch (ArgumentException)
    {
        duplicateRejected = true;
    }
    Check(duplicateRejected, "Duplicate mission IDs must be rejected when loading a catalog.");

    MissionSession partialAssignment = new("player-two", catalog.Definitions.Skip(1));
    MissionAction unassignedAction = new("player-two", mission.Id, mission.LocationId, mission.OffenseActions[0], 44);
    CheckStatus(partialAssignment.TryComplete(unassignedAction, validContext), MissionActionStatus.UnknownMission);
    Check(partialAssignment.CompletedCount == 0 && partialAssignment.CompletionPoints == 0,
        "An infraction outside the assignment must not grant mission progress.");

    MissionSession eventDriven = new("player-three", catalog);
    InfractionEvent validated = new(99, "player-three", string.Empty, mission.InfractionCategory,
        mission.LocationId, mission.OffenseActions[0], 45);
    CheckStatus(eventDriven.ConsumeValidatedInfraction(validated with { ActorId = "another-player" }), MissionActionStatus.WrongActor);
    CheckStatus(eventDriven.ConsumeValidatedInfraction(validated with { LocationId = "another-place" }), MissionActionStatus.UnknownMission);
    CheckStatus(eventDriven.ConsumeValidatedInfraction(validated with { ActionId = "do_nothing" }), MissionActionStatus.UnknownMission);
    CheckStatus(eventDriven.ConsumeValidatedInfraction(validated with { Category = "sabotage" }), MissionActionStatus.UnknownMission);
    CheckStatus(eventDriven.ConsumeValidatedInfraction(validated with { MissionId = "neighbor_mail" }), MissionActionStatus.UnknownMission);
    Check(eventDriven.CompletedCount == 0 && eventDriven.Events.Count == 0,
        "Unmatched validated events must not change mission progress.");
    MissionAttempt applied = eventDriven.ConsumeValidatedInfraction(validated);
    Check(applied.Status == MissionActionStatus.Completed && applied.MissionId == mission.Id &&
        ReferenceEquals(applied.Event, validated) && ReferenceEquals(eventDriven.Events[0], validated),
        "Mission progress must consume the authoritative event rather than create a second event.");
    CheckStatus(eventDriven.ConsumeValidatedInfraction(validated), MissionActionStatus.AlreadyCompleted);
    CheckStatus(eventDriven.ConsumeValidatedInfraction(validated with { Sequence = 100 }), MissionActionStatus.AlreadyCompleted);
    Check(eventDriven.CompletedCount == 1 && eventDriven.CompletionPoints == mission.CompletionPoints &&
        eventDriven.Events.Count == 1,
        "Replaying an event or repeating an action must not duplicate points or consumed events.");
    CheckStatus(partialAssignment.ConsumeValidatedInfraction(validated with { ActorId = "player-two" }), MissionActionStatus.UnknownMission);

    Console.WriteLine("M03/M04 domain checks passed: catalog, abstention, validation, assignments, authoritative event consumption, and duplicates.");
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine($"M03 domain checks failed: {error}");
    return 1;
}

static void CheckStatus(MissionAttempt attempt, MissionActionStatus expected)
{
    Check(attempt.Status == expected, $"Expected {expected}, got {attempt.Status}.");
}

static void Check(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}
