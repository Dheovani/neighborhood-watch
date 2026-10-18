# Missions, Infractions, and Penalties — Design Specification v0.2

**Status:** Player-approved design direction for M04/M05, supplied on 2026-10-17. This document distinguishes confirmed rules from provisional content and unresolved values. It supersedes conflicting mission, evidence, and reporting proposals in earlier documents. It does not claim these systems are implemented. In this document, **authoritative host** means the Godot server instance that owns match state; the current architecture calls for a dedicated server.

## Confirmed rules

- Every mission requires at least one specific infraction. A lawful substitute does not complete that mission. Declining the act leaves the mission incomplete and grants no completion points.
- A mission defines **what** must happen, while the player chooses **how** to make it happen. Stealth, distractions, access permission, timing, environmental changes, and equipment affect risk but do not replace the required infraction.
- Interactions and infractions exist independently of missions. A player may perform them spontaneously, including to help with another objective. Missions evaluate validated world state and events; they do not exclusively own interactions.
- Committing an infraction can change the world immediately, but **never causes an automatic penalty**. A penalty requires a voluntary player report, sufficient evidence, and server validation.
- A trace alone does not automatically identify its creator. Cameras record without filing reports. A power outage does not erase evidence already recorded.
- The server owns the true match state. Clients receive only facts their players are legitimately able to know.

## Initial mission catalog

These twelve concepts define initial design content, not a promise that all are implemented or balanced. Target player, location, quantity, and conditions may vary.

| Mission | Required infraction and context |
|---|---|
| **An Unforgettable Walk** | Take a dog to the park, wait for it to defecate, and leave the waste behind. |
| **Lunch Outdoors** | Eat a snack in the public square and drop its wrapper on the ground. |
| **Number Two Emergency** | Use another player's home bathroom and leave without flushing. |
| **The Mysterious Letter** | Open and read mail addressed to another player. |
| **Convenient Blackout** | Switch off the electrical breaker for another player's home. |
| **The Perfect Shortcut** | Cross a neighbor's yard without permission. |
| **Doorbell Dash** | Ring the doorbells of three different homes and leave before anyone answers. |
| **Rebel Posters** | Put posters on three properties without permission. |
| **The Model Gardener** | Sweep leaves from one's own yard into another player's yard. |
| **Free Water** | Use another resident's outdoor tap without permission to fill a watering can. |
| **Express Recycling** | Dispose of waste improperly in another resident's bin. |
| **The Wandering Bin** | Move a bin so that it blocks another player's garage. |

### Five detailed mission examples

| Mission | Required sequence | Strategy and evidence |
|---|---|---|
| **An Unforgettable Walk** | Lead the dog to a valid area; the dog defecates; leave without collecting the waste. | Choose a location and time with less surveillance. A witness, camera, or residue may reveal the act, but residue alone does not identify the player. Dog behavior must remain predictable enough to avoid unbounded waiting. |
| **Number Two Emergency** | Enter another player's home; use its bathroom; leave without flushing. | The player may ask for entry or sneak in. Unauthorized entry can create a separate infraction; the required one is failing to flush. Entrance and exit may be observed, and the bathroom's state may be discovered later. That state alone does not identify who used it. |
| **The Mysterious Letter** | Locate the assigned player's mailbox; access the mail; read it. | A witness or camera may record the improper interaction. Moved or opened mail raises suspicion without automatically identifying the reader. |
| **Convenient Blackout** | Locate and access another player's breaker; switch off the circuit. | Connected lights, cameras, and other equipment are affected immediately. An unpowered camera stops recording new events; earlier recordings remain. Witnesses or recordings made before the outage can identify the actor. |
| **Lunch Outdoors** | Obtain a snack; eat it in the public square; leave the wrapper on the ground. | Witnesses, cameras, or the discarded wrapper may reveal the event. The wrapper remains in the world until collected, but it alone does not prove authorship. |

These sequences specify outcomes, not mandatory routes. Permission to enter a home can be part of the bathroom mission's strategy, because not flushing remains mandatory. Cutting power may also create an opportunity for another player to enter a yard without a related mission.

## Infraction, observation, evidence, and report lifecycle

1. **Infraction committed:** the server validates the interaction, applies immediate world effects, and records an event with a unique ID, even if there is no mission or witness.
2. **Infraction observable:** the server evaluates player and camera position, orientation, distance, obstruction, timing, device power, and relevant environmental conditions.
3. **Evidence acquired:** a structured witness observation, recording, or trace is linked to the real event. Proof that an act occurred and proof of authorship may come from different sources. A trace alone cannot establish authorship.
4. **Report submitted:** a player voluntarily names an accused player and provides evidence. The reporter need not be the victim. Observation or recording never submits a report automatically.
5. **Report resolved:** the server rejects insufficient proof or applies a proportional consequence when the accusation is established. The same infraction cannot be punished twice.

An infraction can remain unpunished indefinitely when nobody submits a valid report. Reports without enough evidence cannot punish. The earlier proposal to reward a bare accusation is superseded by this specification. Report validity and rate limits must prevent farming.

## Provisional consequence levels

| Level | Examples | Intended result after a valid report |
|---|---|---|
| **1 — Incivility** | Litter, abandoned dog waste, not flushing, minor disturbances. | Light delay, repair, or small penalty. |
| **2 — Intrusion** | Reading mail, trespassing, unauthorized use of facilities. | Intermediate penalty or temporary restriction. |
| **3 — Sabotage** | Cutting power or interfering with another resident's equipment. | Stronger temporary restriction, repair, or penalty. |

Task Race may use delays, added tasks, and temporary restrictions. Score Attack may subtract points. No consequence may permanently remove a player's ability to participate. Amounts, durations, stacking rules, and evidence thresholds require playtesting. Immediate world effects do not wait for a report: a validated breaker action affects its connected equipment at once.

## M04/M05 architecture contract

**M04 — interactions and infractions:** clients submit action intents. The server validates actor, target, access, distance, and world state; applies world changes; records infractions with unique IDs; and checks mission requirements. `ActionExecuted`, `WorldStateChanged`, `InfractionCommitted`, and `MissionCompleted` are **conceptual events**, not a requirement for four classes or an event bus. A mission completes only after its required infraction and other conditions are met. The same action can occur without a mission.

Candidate reusable interactions are `DropTrash`, `CollectWaste`, `ReadMail`, `KnockDoor`, `GrantAccess`, `EnterProperty`, `UseToilet`, `FlushToilet`, and `ToggleBreaker`. The first M04 slice should select only the interactions its playable map supports. Doors, permissions, waste, mail, circuits, cameras, and social interactions become shared world systems as they are added.

**M05 — surveillance and reports:** the server evaluates who could observe an event, creates and retains structured evidence, permits authorized recording access, receives voluntary reports, and applies consequences after validation. `InfractionObserved`, `EvidenceCreated`, `ReportSubmitted`, `ReportResolved`, `PenaltyApplied`, and `PenaltyCompleted` are also conceptual event names. No client can assert evidence, authorship, punishment, or completion as authoritative fact.

### Implementation planning checks

**M04 — first playable slice, not the whole catalog:**

- [ ] Choose a small set of reusable v0.2 interactions that fits the available map; record which content requires later interiors or dog behavior.
- [ ] Validate each action intent on the server and generate unique IDs, world changes, and infractions even when there is no matching mission.
- [ ] Let missions consume validated events and complete only after their required infraction and any other conditions; reject lawful acts and unrelated infractions as mission completion.
- [ ] Replicate only visible world state and legitimately knowable outcomes to each client, including rejection of invalid intents.
- [ ] Test spontaneous actions, valid missions, duplicates, wrong targets, and disconnects with two local clients and one headless server.

**M05 — first surveillance and reporting slice:**

- [ ] Record witness observations, functioning-camera recordings, and traces while keeping occurrence and authorship information distinct.
- [ ] Test distance, orientation, obstruction, power, and event timing; a power cut must prevent new recordings without deleting old ones.
- [ ] Require a voluntary report with an accused player and sufficient evidence; reject accusations without established authorship and limit repeated attempts.
- [ ] Apply at most one temporary penalty per infraction after a validated report; do not punish upon committing or observing the act.
- [ ] Test with two clients that the actor sees immediate world effects, a witness gets only legitimate evidence, and other players receive no hidden facts.

These checks refine `docs/ROADMAP.md`. They do not authorize work on M05 before the player approves that milestone.

## Open design questions

- How can missions target player-owned homes while remaining possible after a target disconnects?
- How can dog behavior be timely and predictable without making that mission trivial?
- Which observations establish authorship, especially for bathrooms, mail, and traces discovered later?
- How long do evidence and reports remain valid, how are repeats limited, and what happens after a false report?
- What are the exact point awards, fines, restriction durations, and mode-specific consequences?

## Implementation status

M03 offers three immediate local actions: dropping paper in the park, reading mail, and taking an unpaid snack from an outdoor market display. The first M04 network slice moves validation and movement to a dedicated server for those same one-time sample spots. A second client can drop paper without that mission; the server records the infraction without granting mission progress. The park paper and spot use count persist and replicate as placeholder world state. General free-form actions and the multi-step sequences above are still absent. The unpaid snack is a prototype sample, not an entry in the v0.2 initial catalog. M04 remains in progress; M05 has not begun.
