# Agent instructions

## Scope and workflow

- Read `README.md`, `docs/design/GDD.md`, `docs/architecture/ARCHITECTURE.md`, `docs/architecture/DECISIONS.md`, and `docs/ROADMAP.md` before changing gameplay architecture.
- Implement only the approved milestone. Never proceed automatically to the next milestone.
- Name branches with a short, plain description of the work, such as `github-automation`. Do not use type prefixes (`chore`, `feat`, `fix`, etc.), slashes, or milestone identifiers.
- Before creating a task branch, fetch `origin/master`, update local `master` to it, and branch from that updated base. Do not branch from another task branch unless the user explicitly requests stacked work.
- Prefer C#, text `.tscn` files, CLI builds, and headless Godot checks. Document any unavoidable editor action precisely.
- Keep gameplay rules separate from presentation and input. At M04 and beyond, the server owns authoritative match state and validates client intents.
- Keep progress, match currency, and reputation independent. Do not treat client screenshots or video as authoritative evidence.
- Respect the player's 2026-10-17 clarification over conflicting pasted specifications: the setting is an ordinary U.S. neighborhood, not a specifically suburban one, and mission completion requires a minor in-game infraction. Declining that act leaves the mission incomplete and awards no completion points. Do not add a responsible alternate completion path.
- The neighborhood has houses and gardens alongside a few shops, a gas station, and a park. Do not visually associate low income with the game's rule-breaking behavior. Reportable in-game infractions include petty crimes and noncriminal incivilities; do not assume that every infraction is a real-world crime. Preserve a silly, friendly tone and exclude grave wrongdoing and severe violence.
- Add abstractions and dependencies only for concrete needs. Do not add a database, web backend, authentication service, or editor plugin without approval.
- Update documentation when a decision becomes confirmed, changes, or is implemented. Mark speculative mechanics as proposed or unresolved.
- Write all project documentation in English, including design documents, roadmap updates, and agent instructions. User-facing conversation may remain in Portuguese.

## Output for this project

- Lead with the answer or next action. Number multi-step work with one bounded action per step; cap visible lists at five items.
- State current progress, concrete validation results, and remaining limitations. Give times in concrete units.
- End with one action doable in under two minutes when user action remains.
- Confirm before destructive actions. After three failed fixes, stop and identify the doubtful assumption.
