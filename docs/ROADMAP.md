# Neighborhood Watch — Implementation Roadmap

This is the working checklist, not a claim that future mechanics are final. `[x]` means implemented and checked; `[ ]` means pending. Check an item only after its stated result is observable. Update this file at the end of each work session. **Do not start a new milestone without the player's explicit approval.**

## Rules that every milestone must preserve

- The setting is an **ordinary U.S. neighborhood** with houses and gardens plus a few commercial and public places. Do not imply that low income causes the offenses. The exact city and density remain undecided.
- Every player can commit minor reportable infractions, observe others, gather evidence, and report. In-game infractions include petty crimes and rude or inconsiderate acts, regardless of whether they are crimes in real life. There are no fixed innocent, criminal, or detective roles.
- **Completing a mission requires its infraction.** Choosing not to do it leaves the mission incomplete and earns no completion points. Do not create a responsible alternate completion route.
- Preserve a silly, friendly tone; exclude killing, severe violence, violent robbery, corruption, and comparable grave wrongdoing from missions.
- Match progress, Score Attack score, match currency, and reporting credibility are distinct concepts. The exact role of credibility remains to be designed.
- M04 onward: the dedicated server decides authoritative gameplay outcomes. Client screenshots and video are not authoritative evidence.

Estimates below are planning ranges for one developer working roughly six focused hours per day. They exclude asset production beyond placeholders and extra playtest revisions. Re-estimate after each milestone.

## M01 — Project foundation

**State:** Automated validation complete; visual playtest pending. **Remaining estimate:** 30–60 minutes.

### Environment and project

- [x] Verify the available Godot executable is 4.7.2 stable .NET and record its version.
- [x] Verify the installed .NET SDK and choose a compatible target framework (`net8.0`).
- [x] Create the Godot C# project and set the main scene in `project.godot`.
- [x] Keep build output out of version control and initialize Git.
- [x] Recommend the VS Code C# and Godot extensions.

### Playable sandbox

- [x] Define movement, jump, and mouse capture input actions in `project.godot`.
- [x] Create a reusable player scene with capsule mesh and collision shape.
- [x] Implement camera-relative walking and jumping in C#.
- [x] Add yaw and pitch controls to a spring-arm follow camera.
- [x] Create a small ground plane with collision, sky color, and directional light.

### Validation and handoff

- [x] Document setup, controls, architecture, decisions, and future milestones.
- [x] Add one CLI command for restore, build, Godot import, scene startup, and smoke checks.
- [x] Verify C# builds with zero warnings and errors.
- [x] Verify the main scene starts headlessly and the smoke check covers walking, ground contact, and camera rotation.
- [x] Launch the scene visibly and confirm W/A/S/D motion follows camera direction.
- [x] Confirm mouse orbit, Escape release, left-click recapture, and camera clipping behavior visually.
- [x] Confirm Space jumps, landing works, and the player does not fall through the ground.

**Acceptance:** A developer can open the folder in VS Code, run `scripts/dev/validate.ps1`, launch the main scene, and use the listed controls. The three visual checks above remain the M01 completion gate.

## M02 — Ordinary U.S. neighborhood prototype

**State:** Planned; not approved for implementation. **Estimate:** 3–5 development days after visual direction is approved.

### Decide the small playable area

- [ ] Select visual references for U.S. houses with gardens, a small market, a gas station, and a park without coding the area as low income.
- [ ] Assess the Synty POLYGON Town Pack or alternatives for the chosen layout, Godot 4.7.2 import, licence, and missing pieces before purchase.
- [ ] Decide the prototype block's density, approximate walkable dimensions, and boundary treatment.
- [ ] Draw a simple top-down block plan showing homes, gardens, streets, the market, gas station, park, parking, and potential mission locations.
- [ ] Identify sightlines, hiding spots, and routes that support observation and evasion.
- [ ] Approve the block plan before producing detailed scene geometry.

### Build the environment

- [ ] Split the environment into reusable text scenes for a street segment, sidewalk, building shell, and props only where reuse is demonstrated.
- [ ] Replace the M01 flat test ground with one walkable block assembled from primitive or simple stylized meshes.
- [ ] Add recognizable U.S. street cues such as road markings, curb, parked vehicles, signs, and building entrances, without assuming a specific city.
- [ ] Place residential houses and gardens alongside a small market and gas station so the area reads as a lived-in neighborhood.
- [ ] Make the park a walkable public space with paths and sightlines suitable for future dog-walking interactions.
- [ ] Mark potential market and gas station interiors or entrances for M03; decide their accessible scope before adding detailed rooms.
- [ ] Add small landmarks that help players orient themselves without relying on a minimap.
- [ ] Use a restrained placeholder color palette with clear contrast between walkable ground, walls, and interactive objects.
- [ ] Add collision to buildings, props, street fixtures, and map boundaries.
- [ ] Verify the camera can navigate tight spaces without persistent wall clipping or losing the player.
- [ ] Mark possible future interaction locations for a market purchase, dog walk, and gas station bathroom, without implementing missions yet.
- [ ] Keep placeholder props and markers independent of mission rules so M03 can replace them without rebuilding the map.

### Validate and document

- [ ] Add a headless scene-load check for the neighborhood scene.
- [ ] Walk every intended route and verify there are no collision gaps, trapped areas, or inaccessible markers.
- [ ] Verify key observation positions have useful sightlines and at least one practical route around them.
- [ ] Check the map remains readable at the expected third-person camera distance.
- [ ] Record the chosen visual references, reusable scene pieces, and known map limitations.

**Acceptance:** One small U.S. neighborhood area with houses and gardens, a market, a gas station, and a park is readable, traversable, and supports future mission and surveillance interactions. M02 adds no mission, evidence, or multiplayer logic.

## M03 — Missions and offense events

**State:** Planned; requires M02 approval and completion. **Estimate:** 5–8 development days.

### Set the rule boundary

- [ ] Confirm the first sample missions and their exact in-world interactions; market unpaid item, uncollected dog waste, and gas station bathroom are player-provided candidates, not locked content.
- [ ] Define infraction categories for mission data without assuming that all reportable acts are real-world crimes.
- [ ] Review each sample mission against the silly, friendly tone and reject severe harm or grave wrongdoing.
- [ ] Make any mission dependent on an environmental or animal event achievable without unbounded random waiting.
- [ ] Decide whether mission definitions use Godot resources or a small text data format based on the concrete sample needs.
- [ ] Define stable IDs, display text, valid locations, action requirements, completion conditions, and rewards for each sample mission.
- [ ] Define one local owner of match state for this offline milestone; isolate it from UI and input so M04 can move ownership to the server.
- [ ] Define an offense event with actor, offense type, location, timestamp or match tick, and source mission.
- [ ] Make a successful offense action update mission progress and emit its offense event as one validated outcome.
- [ ] Ensure declining or ignoring an offense leaves its mission incomplete and emits no completion reward.

### Implement the first vertical slice

- [ ] Add interaction detection with clear range and target selection.
- [ ] Add one readable interaction prompt that states the offense before the player acts.
- [ ] Implement the first sample mission end to end from assignment through action, event, and completion display.
- [ ] Add the remaining approved sample missions through definitions and small action-specific components, without hardcoding them into a central switch statement.
- [ ] Show assigned, incomplete, and completed missions in a minimal UI.
- [ ] Show immediate feedback when an offense completes a mission.
- [ ] Prevent duplicate completion and duplicate reward events from repeated input.
- [ ] Handle invalid location, missing target, and interrupted interaction without awarding progress.
- [ ] Preserve completed missions after the offender is later observed or reported; any penalty is a separate outcome.

### Validate and document

- [ ] Add automated rule tests for valid completion, invalid context, duplicate input, and abstention.
- [ ] Add a scene smoke check that a player can reach and trigger each sample interaction.
- [ ] Verify every completed sample mission emits exactly one offense event.
- [ ] Verify no sample mission grants completion without its required infraction.
- [ ] Record mission data format and the event contract needed by M04 and M05.
- [ ] Run the full build and Godot import/startup checks.

**Acceptance:** At least three approved minor-infraction missions can be assigned and completed in the prototype map. The samples may include a petty crime or an inconsiderate act. Completion, reward eligibility, and infraction event creation agree; abstention yields none of them. Evidence, reports, and multiplayer remain out of scope.

## M04 — Authoritative multiplayer foundation

**State:** Planned; requires M03 approval and completion. **Estimate:** 8–12 development days.

### Decide authority and session behavior

- [ ] Document client intents, server-owned state, replicated state, and rejection reasons for each M03 interaction.
- [ ] Decide a movement synchronization method after a local latency test; document any prediction or interpolation added.
- [ ] Set a development-only connection flow using Godot high-level multiplayer, ENet, and a headless server.
- [ ] Set clear match capacity and connection/disconnection behavior for the initial 4–8 player target.

### Implement the session

- [ ] Add a dedicated server launch command that works without a graphical window.
- [ ] Add minimal host, join, and connection status UI for local development.
- [ ] Assign and validate one peer identity per connected player on the server.
- [ ] Spawn one player scene per connected peer and remove it after disconnect.
- [ ] Move authoritative player positions and essential motion state to the server.
- [ ] Replicate other players' positions with smoothing suitable for observation gameplay.
- [ ] Prevent a client from directly changing another player's character or camera.
- [ ] Keep each client's camera and local presentation separate from server gameplay state.
- [ ] Move mission assignment, completion, and offense event ownership to server-side match state.
- [ ] Accept interaction requests as intents and validate player, target, distance, context, and repeat use on the server.
- [ ] Send only confirmed outcomes back to clients and keep UI consistent after a rejected request.
- [ ] Handle a disconnected player without leaving interactables locked or breaking the match.

### Validate and document

- [ ] Run one headless server and at least two local clients from repeatable CLI commands.
- [ ] Verify both clients see each other move and can complete their own assigned missions.
- [ ] Verify a forged completion request or invalid target cannot award progress or create an offense event.
- [ ] Test join, leave, reconnect, and server shutdown behavior.
- [ ] Test playability under a documented simulated latency and packet-loss setting.
- [ ] Record the chosen synchronization method, known latency limits, and message ownership rules.
- [ ] Run automated rule tests and scene startup checks after networking changes.

**Acceptance:** The headless server owns and validates player, mission, and offense state. Multiple clients share one match without trusting a client's claim of completion. Accounts, matchmaking, and external services remain out of scope.

## M05 — Observation, evidence, and reporting

**State:** Planned; requires M04 approval and completion. **Estimate:** 7–11 development days.

### Specify outcomes before implementation

- [ ] Agree on what counts as an observable offense, including distance, line of sight, timing, and identity visibility.
- [ ] Define evidence opportunities for private-location infractions without placing implausible surveillance inside bathrooms.
- [ ] Agree on an initial evidence model for identification, clarity, and reliability without treating proximity as automatic proof.
- [ ] Decide the first evidence sources; direct observation and one camera-like source are candidate minimums.
- [ ] Decide the initial outcomes for unsupported, weak, strong, and false reports.
- [ ] Define fictional report outcomes for petty crimes and for noncriminal incivilities without implying that they have the same real-world legal status.
- [ ] Decide report cooldowns, one-offense limits, and other anti-farming rules.
- [ ] Decide whether reporting credibility changes in this milestone and what it affects.

### Implement server-known evidence

- [ ] Store server-owned offense records with bounded retention for report resolution.
- [ ] Sample or reconstruct relevant observer position, facing, visibility, and timing on the server.
- [ ] Create structured evidence records tied to a real offense and an eligible observer.
- [ ] Apply occlusion and identity uncertainty before assigning evidence quality.
- [ ] Allow a player to inspect collected evidence without exposing hidden server-only facts.
- [ ] Reject fabricated offense IDs, timestamps, observer identities, and client media as authoritative evidence.
- [ ] Keep camera-derived evidence under the same server validation rules as direct observation.

### Implement reporting

- [ ] Provide a report flow that allows a named target and optional evidence selection.
- [ ] Resolve reports on the server and return a clear result to reporter and accused.
- [ ] Ensure a report without evidence cannot by itself establish guilt or impose a significant penalty.
- [ ] Reward useful evidence according to a configurable policy rather than a UI-side formula.
- [ ] Apply one reversible, limited penalty type first and ensure it does not undo completed missions.
- [ ] Enforce cooldowns or per-offense limits and reject self-reports and duplicate farming attempts.
- [ ] Record enough match events to diagnose disputed report outcomes during playtests.

### Validate and document

- [ ] Test witnessed, blocked, distant, expired, mistaken-identity, and fabricated evidence scenarios.
- [ ] Test reports with no evidence, weak evidence, strong evidence, and repeated submissions.
- [ ] Run a multi-client scenario where one player offends and another submits a valid report.
- [ ] Confirm penalties are temporary and do not remove player control for excessive periods.
- [ ] Record evidence rules, report outcomes, tuning values, and unresolved balance questions.

**Acceptance:** Players can gather meaningful server-validated evidence and report offenses. Report rewards and penalties vary with support, while unsupported and repeated accusations cannot be farmed for major gains.

## M06 — Game modes, match economy, and equipment

**State:** Planned; requires M05 approval and completion. **Estimate:** 7–11 development days.

### Finalize match rules

- [ ] Decide Task Race mission count, optional time limit, timeout ranking, and deterministic tie breakers.
- [ ] Decide Score Attack duration options, scoring events, and tie breakers.
- [ ] Decide whether a completed offense mission also grants separate offense points; prevent accidental double counting.
- [ ] Define match start, active play, end, and results states owned by the server.
- [ ] Put mode settings and scoring or victory rules in validated configuration instead of scene-specific branches.
- [ ] Ensure both modes reuse M03–M05 mission, offense, evidence, and report rules.

### Implement both modes

- [ ] Assign missions when a Task Race match starts and end the match on the first validated full completion.
- [ ] Resolve Task Race timeout and ties using the agreed deterministic rules.
- [ ] Track Score Attack points separately from task progress and match currency.
- [ ] End Score Attack at the configured server timer and display a stable ranking.
- [ ] Show active mode, remaining time where applicable, progress or score, and final results in UI.
- [ ] Prevent late or duplicate actions from changing results after a match ends.

### Add match economy and equipment

- [ ] Define currency earning events, starting balance, item prices, and spending rules.
- [ ] Add a small shop flow and server-validated purchase requests.
- [ ] Maintain server-owned balances and inventory; reject insufficient funds and duplicate purchases where restricted.
- [ ] Implement at least one surveillance item with a measurable observation benefit and a real limitation.
- [ ] Implement at least one stealth item with a measurable detection reduction and a real counterplay path.
- [ ] Add item use limits, placement rules, duration, or cooldown as appropriate to each chosen item.
- [ ] Ensure purchases do not lower Task Race progress or Score Attack score.
- [ ] Reset currency, inventory, and temporary effects when a new match begins.

### Validate and document

- [ ] Test winner selection, timer expiry, simultaneous finishes, ties, and post-match action rejection.
- [ ] Test score, progress, currency, and credibility remain logically independent.
- [ ] Test invalid purchases, item limits, effect expiration, and no-perfect-surveillance or invisibility cases.
- [ ] Run one complete local multiplayer match in each mode with reports and equipment.
- [ ] Update player-facing rules, mode configuration, balance assumptions, and known limitations.

**Acceptance:** Task Race and Score Attack produce correct server-owned winners from shared gameplay systems. Match currency buys limited equipment without changing victory progress or score. No persistent accounts, unlocks, or external infrastructure are required.

## Approval gates

- [ ] Complete M01's visual playtest and record any controller or camera fixes.
- [ ] Receive explicit approval before implementing M02.
- [ ] At the end of each later milestone, review its acceptance evidence and unresolved decisions before approving the next one.
