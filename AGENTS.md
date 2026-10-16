# Agent instructions

## Scope and workflow

- Read `README.md`, `docs/design/GDD.md`, `docs/architecture/ARCHITECTURE.md`, `docs/architecture/DECISIONS.md`, and `docs/ROADMAP.md` before changing gameplay architecture.
- Implement only the approved milestone. Never proceed automatically to the next milestone.
- Prefer C#, text `.tscn` files, CLI builds, and headless Godot checks. Document any unavoidable editor action precisely.
- Keep gameplay rules separate from presentation and input. At M04 and beyond, the server owns authoritative match state and validates client intents.
- Keep progress, match currency, and reputation independent. Do not treat client screenshots or video as authoritative evidence.
- Add abstractions and dependencies only for concrete needs. Do not add a database, web backend, authentication service, or editor plugin without approval.
- Update documentation when a decision becomes confirmed, changes, or is implemented. Mark speculative mechanics as proposed or unresolved.

## Output for this project

- Lead with the answer or next action. Number multi-step work with one bounded action per step; cap visible lists at five items.
- State current progress, concrete validation results, and remaining limitations. Give times in concrete units.
- End with one action doable in under two minutes when user action remains.
- Confirm before destructive actions. After three failed fixes, stop and identify the doubtful assumption.
