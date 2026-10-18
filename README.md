![neighborhood-watch](./neighborhood-watch-logo.svg)

## Brief

Neighborhood Watch is a planned 3D multiplayer social stealth game set in an ordinary U.S. neighborhood with houses and gardens, some shops, a gas station, and a park. The setting does not associate income level with wrongdoing. Players complete missions through minor reportable infractions, ranging from petty crimes to rude behavior, while watching and reporting neighbors who do the same. A player may refuse a mission's infraction, but then does not complete that mission or receive its points. The intended tone is silly and friendly, without grave crime or severe violence. **M04 currently implements a development multiplayer slice.** Reporting and match rules remain proposals.

**M04 is in progress:** the three M03 exterior interactions passed a visible playtest. A development-only dedicated server can now connect two or more clients and validate movement and a sample mission interaction. Reporting and match flow are not implemented.

## Requirements

- Windows PC for the current development workflow.
- Godot **4.7.2 stable .NET**. A local copy lives in ignored `tools/godot/`. If missing, put `Godot_v4.7.2-stable_mono_win64.zip` in Downloads and the validation script extracts it there. An existing executable can be supplied with `-GodotPath` or `GODOT_EXE`.
- .NET SDK 10.0.401 is verified. The game project targets `net8.0`, which is supported by the installed SDK and runtime.
- Visual Studio Code with the recommended C# and Godot extensions.

Godot's .NET edition is required. The standard Godot executable cannot compile C# scripts. Keep the Godot executable and its `GodotSharp` directory together after extraction.

## Build and run

From PowerShell in the project root:

```powershell
.\scripts\dev\validate.ps1
.\scripts\dev\validate.ps1 -Play
```

The first command validates; the second repeats validation and opens the game for a visual playtest. An explicit `-GodotPath` takes priority; otherwise the script checks `tools/godot/`, `GODOT_EXE`, its temporary cache, PATH, and the named Downloads ZIP. It restores packages from Godot's bundled NuGet feed when available, builds C#, runs dependency-free .NET mission rule checks, imports scenes, launches the main scene headlessly, and checks movement, neighborhood routes, missions, and one server with two network clients. To build only, run `dotnet build NeighborhoodWatch.csproj`. On first restore without the bundled packages, NuGet network access may be needed.

The game now opens a small connection menu by default. Choose **Play offline**, **Host locally**, or **Join server**. A local host starts a separate headless Godot server and joins it automatically. A second game window can join at `127.0.0.1` on UDP port `24567`. Press **Tab** to reopen the menu and leave a session. A failed connection returns to the menu after a five-second timeout. The address and port fields are development controls, not a matchmaking service.

The command-line flow remains available for repeatable development sessions. Use three PowerShell terminals:

```powershell
.\scripts\dev\run-network.ps1 -Server
.\scripts\dev\run-network.ps1
.\scripts\dev\run-network.ps1
```

Run one command per terminal, in the order shown. The script finds the local Godot 4.7.2 .NET executable or accepts `-GodotPath` or `GODOT_EXE`. The server is headless; the two clients open separate visible windows. The network mode currently starts players near the park.

Open this folder in VS Code. Edit C# and text scene files there; Godot's editor is only needed to inspect scene composition or test the controls visually.

## GitHub automation

Pull requests targeting `master` run two workflows. **CI** verifies C# formatting, builds with warnings as errors, runs mission rule checks, then imports and launches the project in headless Godot 4.7.2 .NET and runs movement, neighborhood, mission, and two-client network smoke tests. **Security** audits the game and domain-check projects for known NuGet vulnerabilities of moderate severity or higher. Public repositories also run C# CodeQL; GitHub requires an eligible Code Security plan for private repositories, so that job is skipped there. The Godot download is pinned to the official 4.7.2 release and checked against its SHA-256 digest.

Dependabot checks GitHub Actions and NuGet weekly. `Godot.NET.Sdk` updates are excluded because the SDK and installed Godot editor must be upgraded together and validated locally. GitHub issue forms and the pull request template live in `.github/`. Recommended branch protection checks are **Code quality**, **Game smoke**, and **NuGet audit**; do not require **CodeQL C#** while the repository is private. CI on GitHub starts only after this branch is committed and pushed. The local Windows equivalent remains `./scripts/dev/validate.ps1`.

## Controls

| Input | Action |
|---|---|
| W, A, S, D | Move relative to camera |
| Mouse | Orbit camera |
| Space | Jump |
| Escape | Release or capture mouse |
| Left click | Recapture mouse |
| E | Commit the prompted infraction at a mission location |
| Tab | Show or hide the connection menu |

The scene has a capsule player, ground collision, lighting, a following camera, and a placeholder neighborhood with five houses, a market, a gas station, and a park. Prototype missions let the player litter in the park, read a neighbor's mail, or take an unpaid snack from a market display. Walking away completes nothing. Each successful action records one local infraction event and awards the mission's points once. Building interiors are closed.

## Project map

- `scenes/gameplay/Main.tscn`: scene entry point, lighting, neighborhood, and player.
- `scenes/environments/Neighborhood.tscn`: walkable block, road, sidewalks, sites, and boundary collision.
- `scenes/environments/`: reusable building, park, car, and tree scenes.
- `scenes/characters/Player.tscn`: player body, collision, and camera hierarchy.
- `scripts/characters/PlayerController.cs`: local prototype movement and camera input.
- `resources/missions/initial.json`: editable sample mission definitions.
- `scripts/gameplay/missions/Domain/`: scene-independent catalog, validation, progress, and infraction events.
- `scripts/gameplay/missions/LocalMissionController.cs`: offline scene adapter for interaction range, sightline, and UI.
- `scripts/networking/NetworkSession.cs`: development ENet session with server-owned movement, mission validation, and infraction records.
- `tests/`: .NET rule checks and Godot scene smoke checks.
- `docs/design/MISSIONS_AND_INFRACTIONS.md`: confirmed v0.2 mission, evidence, and consequence rules with provisional values.
- `scripts/dev/validate.ps1`: repeatable local CLI validation.
- `docs/design/GDD.md`: confirmed game concept and proposed mechanics.
- `docs/architecture/ARCHITECTURE.md`: current structure and planned authority boundaries.
- `docs/architecture/DECISIONS.md`: architecture decisions and open questions.
- `docs/ROADMAP.md`: detailed implementation checklist, acceptance criteria, validation, and milestone approval gates.
- `docs/design/M02_BLOCK_PLAN.md`: prototype layout, routes, asset assessment, and visual review points.

## Current limits

The network slice offers a development connection menu and immediate exterior interactions. Local hosting was confirmed in a visible playtest on 2026-10-18. It has server-owned movement and mission validation, one unassigned sample infraction, and a replicated park paper state. It has no prediction, gameplay-relevant world systems, multi-step actions, matchmaking lobby, evidence, reports, or match results. Remote movement is interpolated; local movement can feel rough under latency. The visual relevance gate is provisional and does not count as evidence. Mission points are prototype completion rewards, not final Score Attack scoring. M04 still requires a visible multi-client playtest and simulated latency tests.
