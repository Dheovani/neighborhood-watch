# Neighborhood Watch

Neighborhood Watch is a planned 3D multiplayer social stealth game set in an ordinary U.S. neighborhood with houses and gardens, some shops, a gas station, and a park. The setting does not associate income level with wrongdoing. Players complete missions through minor reportable infractions, ranging from petty crimes to rude behavior, while watching and reporting neighbors who do the same. A player may refuse a mission's infraction, but then does not complete that mission or receive its points. The intended tone is silly and friendly, without grave crime or severe violence. **M02 currently implements a single player neighborhood prototype.** Gameplay rules and networking are documented proposals, not implemented features.

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

The first command validates; the second repeats validation and opens the game for a visual playtest. An explicit `-GodotPath` takes priority; otherwise the script checks `tools/godot/`, `GODOT_EXE`, its temporary cache, PATH, and the named Downloads ZIP. It restores packages from Godot's bundled NuGet feed when available, builds C#, imports scenes, runs the main scene headlessly for 20 frames, and checks movement, camera rotation, neighborhood landmarks, routes, and collisions. To build only, run `dotnet build NeighborhoodWatch.csproj`. On first restore without the bundled packages, NuGet network access may be needed.

Open this folder in VS Code. Edit C# and text scene files there; Godot's editor is only needed to inspect scene composition or test the controls visually.

## GitHub automation

Pull requests targeting `master` run two workflows. **CI** verifies C# formatting, builds with warnings as errors, then imports and launches the project in headless Godot 4.7.2 .NET and runs movement and neighborhood smoke tests. **Security** audits NuGet dependencies for known vulnerabilities of moderate severity or higher. Public repositories also run C# CodeQL; GitHub requires an eligible Code Security plan for private repositories, so that job is skipped there. The Godot download is pinned to the official 4.7.2 release and checked against its SHA-256 digest.

Dependabot checks GitHub Actions and NuGet weekly. `Godot.NET.Sdk` updates are excluded because the SDK and installed Godot editor must be upgraded together and validated locally. GitHub issue forms and the pull request template live in `.github/`. Recommended branch protection checks are **Code quality**, **Game smoke**, and **NuGet audit**; do not require **CodeQL C#** while the repository is private. CI on GitHub starts only after this branch is committed and pushed. The local Windows equivalent remains `./scripts/dev/validate.ps1`.

## Controls

| Input | Action |
|---|---|
| W, A, S, D | Move relative to camera |
| Mouse | Orbit camera |
| Space | Jump |
| Escape | Release or capture mouse |
| Left click | Recapture mouse |

The scene has a capsule player, ground collision, lighting, a following camera, and a placeholder neighborhood with five houses, a market, a gas station, and a park. There is no objective or match flow yet. Building interiors are closed.

## Project map

- `scenes/gameplay/Main.tscn`: scene entry point, lighting, neighborhood, and player.
- `scenes/environments/Neighborhood.tscn`: walkable block, road, sidewalks, sites, and boundary collision.
- `scenes/environments/`: reusable building, park, car, and tree scenes.
- `scenes/characters/Player.tscn`: player body, collision, and camera hierarchy.
- `scripts/characters/PlayerController.cs`: local prototype movement and camera input.
- `scripts/dev/validate.ps1`: repeatable M01 CLI validation.
- `docs/design/GDD.md`: confirmed game concept and proposed mechanics.
- `docs/architecture/ARCHITECTURE.md`: current structure and planned authority boundaries.
- `docs/architecture/DECISIONS.md`: architecture decisions and open questions.
- `docs/ROADMAP.md`: detailed implementation checklist, acceptance criteria, validation, and milestone approval gates.
- `docs/design/M02_BLOCK_PLAN.md`: prototype layout, routes, asset assessment, and visual review points.

## Current limits

M02 still uses local input and movement. It does not implement missions, multiplayer, or server authority. Headless validation proves scene loading and sampled collision/route checks. The first visible playtest worked; detailed camera and route coverage remain open for refinement.
