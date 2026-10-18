# Architecture Decisions

## Accepted for M01

1. **Godot 4.7.2 .NET, C#, and `net8.0`.** The available Godot ZIP reports `4.7.2.stable.mono`; the installed .NET 10 SDK and .NET 8 runtime support this target. Recheck compatibility when upgrading Godot.
2. **Text scenes and a small C# controller.** This supports the VS Code-first workflow and keeps the foundation inspectable without editor-only work.
3. **GL Compatibility renderer.** The original project shell forced Windows D3D12. Removing that override avoids a platform-specific setting and is sufficient for primitive M01 visuals. Revisit renderer choice for art and performance testing.
4. **Local movement only in M01.** It allows a playable controller without pretending it is network authoritative. M04 replaces this ownership with server-validated movement.
5. **No extra infrastructure.** No database, backend, dependency injection framework, asset pipeline, or networking code exists before a concrete need.

## Proposed for later milestones

- Data-driven infraction missions and configurable match mode policies. A responsible alternate completion route is excluded by the player's 2026-10-17 clarification, which supersedes contrary examples in earlier specifications.
- The setting is an ordinary U.S. neighborhood with houses and gardens plus a few shops, a gas station, and a park. Avoid a visual link between low income and wrongdoing. The city, density, and architectural references remain open for M02.
- Server-owned offense events, evidence evaluation, reporting outcomes, and balances.
- Godot high-level multiplayer API, ENet, and a dedicated headless server.

## Accepted for the M02 prototype

1. **One compact block built from text scenes and primitive meshes.** The 112 m by 88 m footprint tests traversal and observation sightlines before selecting final art assets or a city reference.
2. **Closed building shells with exterior location markers.** This establishes readable places and collision without implementing interiors or pretending the example missions already exist.
3. **One broad ground collider and visual flat paths.** This keeps the current controller's routes walkable while solid buildings, cars, trees, benches, pumps, and the perimeter have separate collision.
4. **No purchased art dependency.** The Synty pack remains a candidate; its Godot 4.5.1 project and FBX files need a small import trial in Godot 4.7.2 before adoption.

## Confirmed gameplay boundary

The player's 2026-10-17 clarification includes both petty crimes and rude or inconsiderate acts as reportable in-game infractions. "Offense" in planned code is a gameplay label, not a real-world legal finding. Keep missions silly and friendly; exclude killing, severe violence, violent robbery, corruption, and comparable grave wrongdoing. Exact fictional report outcomes remain open.

## Accepted for the M03 prototype

1. **JSON mission definitions.** Three immediate exterior actions need stable IDs and small amounts of text and reward data; JSON is easy to edit and validate without Godot editor work. Reconsider the format if future missions need complex conditions.
2. **Plain C# domain session with a local Godot adapter.** Completion, points, and infraction events share one validated outcome. Godot provides range, sightline, input, and presentation; the session remains testable with `dotnet run` and must move to server ownership in M04.
3. **Exterior sample missions pending content review.** Park litter, neighbor mail, and an unpaid market snack fit existing locations. Dog walking and bathroom examples require systems or interiors the map does not yet contain. These samples demonstrate mechanics and are not final mission or reward tuning.
4. **No external test framework.** The domain checks use a small .NET console project linked to the pure domain files, avoiding new NuGet dependencies for the first rule tests.

## Unresolved

- Movement replication and prediction model for acceptable latency.
- Evidence scoring and observation rules.
- Exact mode tie breakers and scoring numbers.
- Fictional report outcomes and penalties for different infraction categories.

## Confirmed by design specification v0.2

1. **Shared actions can create infractions without missions.** Missions require their specific infraction and any additional conditions. M03's direct action-to-completion flow is a prototype, not the intended world interaction model.
2. **World effects and penalties are separate.** Validated actions may immediately change the environment. Only a later voluntary report supported by sufficient server-validated proof can impose a penalty. Cameras record without reporting; traces alone do not identify an actor.
3. **Server-owned truth is selectively replicated.** The server assigns infraction IDs and retains hidden facts. Rendering visibility does not itself create evidence. Previously recorded evidence survives a power cut.
4. **Consequence levels are provisional.** Incivility, intrusion, and sabotage establish an initial order. Amounts, durations, and evidence thresholds require playtesting.

See `docs/design/MISSIONS_AND_INFRACTIONS.md` for the twelve initial mission concepts and M04/M05 contract. "Host" in that document means the planned authoritative Godot server, not peer authority.

## Accepted for the first M04 slice

1. **ENet plus CLI launch arguments.** `--server`, `--connect=<address>`, and `--port=<number>` provide repeatable local sessions without adding accounts, discovery services, or a launcher.
2. **Server-simulated movement from client input.** Clients submit axes, jump, and look yaw. The server uses the existing character movement code; local clients use confirmed positions without prediction, while remote avatars interpolate. Latency testing is still required.
3. **Server-side mission validation and a match infraction journal.** The server resolves the RPC sender, target, range, and line of sight. It owns each mission session and records one infraction for each successful use of a sample spot. One-time spots can be used without an assigned mission. A replicated use count and park paper provide minimal shared world state; more general and gameplay-relevant interactions remain pending.
4. **Provisional visual relevance gate.** Remote positions are sent only within 40 m and with an unobstructed scene ray. This is a disclosure limit for the prototype, not the observation or evidence model for M05.
