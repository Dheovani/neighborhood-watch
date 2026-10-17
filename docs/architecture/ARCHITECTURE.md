# Technical Architecture

## M01 concrete structure

`project.godot` owns input actions and the main scene. M01's original `Main.tscn` composed a flat ground, environment light, and `Player.tscn`; M02 replaces the flat ground with a neighborhood instance. `Player.tscn` owns the `CharacterBody3D`, collision shape, visible placeholder mesh, yaw and pitch pivots, spring arm, and camera. `PlayerController.cs` owns local movement and camera input. Scene composition and tuning values are text based and editable in VS Code.

## M02 environment composition

`Main.tscn` now instances `Neighborhood.tscn` alongside the player and lighting. The neighborhood scene owns one broad ground collider, boundary colliders, visual road and sidewalks, and instances of reusable house, market, gas station, park, tree, and parked-car scenes. Building shells and solid props own their own `StaticBody3D` collisions. Flat path and road meshes are visual surfaces over the ground collider, avoiding small steps that the current controller cannot climb. Named `Marker3D` nodes are location references only; they do not implement interaction rules. The block coordinates and entry markers are recorded in `docs/design/M02_BLOCK_PLAN.md`.

The dependency flow is currently input actions → player controller → Godot physics and scene nodes. There is no global manager or service layer. The local controller is explicitly a prototype and is **not** a future source of authoritative multiplayer state.

## Planned boundaries, not implemented

At M03, put mission definitions in Godot resources or simple data files and keep their rules separate from scene presentation. An authoritative infraction action should drive both mission completion and a reportable event; declining the action should leave the mission incomplete. In this design, an "offense event" is a gameplay term that may describe a petty crime or an inconsiderate act, not a real-world legal classification. Scenes supply validated interaction context. Introduce abstractions only when concrete missions reveal the needed variation.

At M04, use Godot's high-level multiplayer API over ENet with a dedicated headless server. The server owns match state and validates client action requests. Clients send intent and render replicated results. Movement authority and reconciliation need an explicit design and latency test before implementation. Do not assume the current controller can simply be exposed as an RPC.

At M05, the server records offense and observation events, derives evidence from server-known positions, visibility, equipment, and time, and resolves reports. A client media file or claimed observation is never proof by itself. At M06, configurable mode policies resolve victory and scoring while reusing the same gameplay systems. Match currency, progress, and reputation remain distinct fields.

## Testing and validation

M01 validation uses `dotnet build`, Godot headless import, a headless 20-frame scene launch, and a scene-level smoke check for movement, ground contact, and camera rotation. These checks catch C# compile errors, malformed scenes, startup errors, and broken control wiring. Headless Godot cannot capture the mouse, so the smoke check calls the look method directly. M01's controls were also checked visually. When rule-heavy systems arrive, test their pure C# logic with automated .NET tests, keeping scene-level smoke checks for integration.

M02 adds a scene-level smoke check for the neighborhood's named landmarks, sampled routes, walkable ground, building collision, and perimeter collision. It complements rather than replaces a visible traversal of the full level.

## Compatibility

The project is pinned to Godot 4.7.2 .NET and targets .NET 8. The installed .NET 10 SDK builds `net8.0`. The GL Compatibility renderer avoids a Windows-specific D3D12 setting during M01; graphics settings can change when visual requirements justify it. The M01 scene uses built-in Godot physics and primitive meshes with no external assets or plugins.
