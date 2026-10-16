# Neighborhood Watch

Neighborhood Watch is a planned 3D multiplayer social stealth game about finishing ordinary errands while watching neighbors and hiding your own rule breaking. **M01 currently implements a single player movement sandbox only.** Gameplay rules and networking are documented proposals, not implemented features.

## Requirements

- Windows PC for the current development workflow.
- Godot **4.7.2 stable .NET** executable. The installed ZIP is `Godot_v4.7.2-stable_mono_win64.zip`; extract it outside the repository.
- .NET SDK 10.0.401 is verified. The game project targets `net8.0`, which is supported by the installed SDK and runtime.
- Visual Studio Code with the recommended C# and Godot extensions.

Godot's .NET edition is required. The standard Godot executable cannot compile C# scripts. Keep the Godot executable and its `GodotSharp` directory together after extraction.

## Build and run

From PowerShell in the project root:

```powershell
$env:GODOT_EXE = 'C:\path\to\Godot_v4.7.2-stable_mono_win64_console.exe'
.\scripts\dev\validate.ps1
& $env:GODOT_EXE --path .
```

The validation script restores packages from the Godot ZIP's bundled NuGet feed when available, builds C#, imports scenes, runs the main scene headlessly for 20 frames, and checks movement, ground contact, and camera rotation. To build only, run `dotnet build NeighborhoodWatch.csproj`. On first restore without the bundled packages, NuGet network access may be needed.

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
- `docs/ROADMAP.md`: milestones, acceptance criteria, and validation.

## Current limits

M01 uses local input and movement. It does not implement multiplayer or server authority. Headless validation proves scene loading and script execution, while movement and camera feel still require a short visual playtest in Godot.
