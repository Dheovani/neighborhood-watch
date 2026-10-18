# Technical Architecture

## M01 concrete structure

`project.godot` owns input actions and the main scene. M01's original `Main.tscn` composed a flat ground, environment light, and `Player.tscn`; M02 replaces the flat ground with a neighborhood instance. `Player.tscn` owns the `CharacterBody3D`, collision shape, visible placeholder mesh, yaw and pitch pivots, spring arm, and camera. `PlayerController.cs` owns local movement and camera input. Scene composition and tuning values are text based and editable in VS Code.

## M02 environment composition

`Main.tscn` now instances `Neighborhood.tscn` alongside the player and lighting. The neighborhood scene owns one broad ground collider, boundary colliders, visual road and sidewalks, and instances of reusable house, market, gas station, park, tree, and parked-car scenes. Building shells and solid props own their own `StaticBody3D` collisions. Flat path and road meshes are visual surfaces over the ground collider, avoiding small steps that the current controller cannot climb. Named `Marker3D` nodes are location references only; they do not implement interaction rules. The block coordinates and entry markers are recorded in `docs/design/M02_BLOCK_PLAN.md`.

The dependency flow is currently input actions → player controller → Godot physics and scene nodes. There is no global manager or service layer. The local controller is explicitly a prototype and is **not** a future source of authoritative multiplayer state.

## M03 local mission prototype

`resources/missions/initial.json` contains the sample IDs, text, locations, required infraction action IDs, categories, and prototype completion points. `MissionCatalog` validates this data. `MissionSession` owns assigned missions, completed IDs, points, and an ordered infraction journal; its plain C# domain code has no Godot dependency. A validated `MissionAction` and `InteractionValidation` produce one `MissionAttempt`. Success records completion, points, and an `InfractionEvent` together. Ignoring a mission produces no state change. Events contain actor, mission, category, location, action, tick, and sequence. Offline ticks are local; network ticks and the match-wide event sequence come from the server.

`MissionSpots.tscn` supplies exterior interaction locations and simple props. `LocalMissionController` is the **offline adapter**: it loads the catalog, checks that every definition has a scene spot, selects a nearby visible target, and passes measured distance and line of sight into the domain session when E is pressed. `MissionHud` displays the assignment, prompt, completion, and prototype points. Network mode disables this adapter and uses `NetworkSession` for server validation and state ownership. The current actions are immediate and have no interruption state.

## M04 authoritative network slice

`NetworkSession` activates only with `--server` or `--connect=<address>`. Offline play retains the M03 controller. A dedicated headless server uses Godot's high-level multiplayer API and ENet. Clients send movement axes, jump intent, look yaw, and a selected interaction target. They never submit a position, mission result, reward, or infraction record. The server resolves peer identity from the RPC sender, simulates `PlayerController` movement, checks target identity, distance, and line of sight, owns one `MissionSession` per peer, and stores a match-wide infraction journal with unique IDs. Provisional assignments give the first client three missions and the second two, leaving one park action available without a mission. The current sample spots are one-time actions per player; repeated use creates neither mission progress nor another infraction. Disconnected peers are removed from the scene and match state.

Clients receive server positions for their own avatar. Remote avatars receive positions only while within a provisional 40 m view range and unobstructed by the scene; they interpolate between snapshots. This gate limits unnecessary hidden-position disclosure but is not an evidence rule. It needs adversarial and latency testing before production use. The local avatar currently snaps to server positions, so movement may look rough under latency. The server replicates a persistent use count for visible spots; the park paper appears after a validated drop. This is placeholder world state, not a full waste, mail, or inventory system. Context-dependent multi-step missions are still absent. The M03 session still couples completion to its own event; M04's separate match journal stores successful actions, and the mission evaluator will need a cleaner event-consumption contract as more action types arrive.

The first network slice starts peers near the park to exercise one real validated interaction. CLI arguments remain available for dedicated server automation. `ConnectionMenu` provides a development host/join/leave interface. Local hosting starts a separate headless Godot process, waits for its ready message, then joins through the same ENet client path; leaving or closing the game terminates that child process. A connection attempt times out after five seconds and returns to the menu. The menu blocks local gameplay input while open. `scripts/dev/network-smoke.ps1` tests two clients, validated and rejected interactions, an unassigned infraction, duplicate protection, server movement, menu hosting, joining, leaving, and recovery from an unavailable server. `validate.ps1` and CI call that smoke test.

## Planned boundaries, not implemented

In this design, an "offense event" is a gameplay term that may describe a petty crime or an inconsiderate act, not a real-world legal classification. Future multi-step missions may need additional context and action-specific components; add those when a concrete approved mission needs them.

The remaining M04 work includes gameplay-relevant shared world effects, interactions independent of mission spots, stronger state reconciliation, latency testing, and a reconnect policy after an established session drops. The current connection menu is for local development and does not provide discovery or matchmaking. The requirements and initial catalog are in `docs/design/MISSIONS_AND_INFRACTIONS.md`.

At M05, the server derives evidence from server-known positions, visibility, equipment, and time, and resolves voluntary reports. A trace may show that an act occurred without identifying its actor. A camera recording does not automatically submit a report. Only a validated report with sufficient evidence can cause a penalty; a single infraction cannot be punished twice. A client media file or claimed observation is never proof by itself. At M06, configurable mode policies resolve victory and scoring while reusing the same gameplay systems. Match currency, progress, and reputation remain distinct fields.

## Testing and validation

M01 validation uses `dotnet build`, Godot headless import, a headless 20-frame scene launch, and a scene-level smoke check for movement, ground contact, and camera rotation. These checks catch C# compile errors, malformed scenes, startup errors, and broken control wiring. Headless Godot cannot capture the mouse, so the smoke check calls the look method directly. M01's controls were also checked visually. When rule-heavy systems arrive, test their pure C# logic with automated .NET tests, keeping scene-level smoke checks for integration.

M02 adds a scene-level smoke check for the neighborhood's named landmarks, sampled routes, walkable ground, building collision, and perimeter collision. It complements rather than replaces a visible traversal of the full level.

M03 adds a dependency-free .NET executable that tests catalog validation, abstention, invalid context, atomic completion, and duplicate protection. A Godot scene smoke check moves the player to each spot and triggers the actual E input. The local validation script and GitHub CI run both checks. Visible mission playtesting remains necessary to judge readability and control feel.

## Compatibility

The project is pinned to Godot 4.7.2 .NET and targets .NET 8. The installed .NET 10 SDK builds `net8.0`. The GL Compatibility renderer avoids a Windows-specific D3D12 setting during M01; graphics settings can change when visual requirements justify it. The M01 scene uses built-in Godot physics and primitive meshes with no external assets or plugins.
