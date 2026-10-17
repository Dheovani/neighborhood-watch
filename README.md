# Neighborhood Watch

Neighborhood Watch is a planned 3D multiplayer social stealth game set in an ordinary U.S. neighborhood with houses and gardens, some shops, a gas station, and a park. The setting does not associate income level with wrongdoing. Players complete missions through minor reportable infractions, ranging from petty crimes to rude behavior, while watching and reporting neighbors who do the same. A player may refuse a mission's infraction, but then does not complete that mission or receive its points. The intended tone is silly and friendly, without grave crime or severe violence. **M01 currently implements a single player movement sandbox only.** Gameplay rules and networking are documented proposals, not implemented features.

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

The first command validates; the second repeats validation and opens the game for a visual playtest. An explicit `-GodotPath` takes priority; otherwise the script checks `tools/godot/`, `GODOT_EXE`, its temporary cache, PATH, and the named Downloads ZIP. It restores packages from Godot's bundled NuGet feed when available, builds C#, imports scenes, runs the main scene headlessly for 20 frames, and checks movement, ground contact, and camera rotation. To build only, run `dotnet build NeighborhoodWatch.csproj`. On first restore without the bundled packages, NuGet network access may be needed.

Open this folder in VS Code. Edit C# and text scene files there; Godot's editor is only needed to inspect scene composition or test the controls visually.

## Controls

| Input | Action |
|---|---|
| W, A, S, D | Move relative to camera |
| Mouse | Orbit camera |
| Space | Jump |
| Escape | Release or capture mouse |
| Left click | Recapture mouse |

The test scene has a capsule player, ground collision, lighting, and a following camera. There is no objective or match flow yet.

## Project map

- `scenes/gameplay/Main.tscn`: minimal test environment and scene entry point.
- `scenes/characters/Player.tscn`: player body, collision, and camera hierarchy.
- `scripts/characters/PlayerController.cs`: local prototype movement and camera input.
- `scripts/dev/validate.ps1`: repeatable M01 CLI validation.
- `docs/design/GDD.md`: confirmed game concept and proposed mechanics.
- `docs/architecture/ARCHITECTURE.md`: current structure and planned authority boundaries.
- `docs/architecture/DECISIONS.md`: architecture decisions and open questions.
- `docs/ROADMAP.md`: detailed implementation checklist, acceptance criteria, validation, and milestone approval gates.

## Current limits

M01 uses local input and movement. It does not implement multiplayer or server authority. Headless validation proves scene loading and script execution, while movement and camera feel still require a short visual playtest in Godot.
