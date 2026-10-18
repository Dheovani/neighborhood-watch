# Game Design Document

## Status

This document separates **confirmed direction**, **proposed mechanics**, **open decisions**, and **implemented features**. Direct player clarification and [Missions, Infractions, and Penalties v0.2](MISSIONS_AND_INFRACTIONS.md) from 2026-10-17 supersede conflicting earlier drafts. Numerical tuning awaits playtesting.

## Confirmed direction

- Third person, stylized 3D social stealth in an ordinary U.S. neighborhood with houses and gardens, some shops, a gas station, and a park. The setting is not intended to signal low income or associate any socioeconomic group with wrongdoing.
- Competitive multiplayer, initially targeting 4 to 8 players on Windows PC.
- Every player can complete tasks, commit offenses, observe neighbors, collect evidence, and report. There are no fixed innocent or impostor roles.
- Missions involve minor reportable infractions: petty crimes or merely rude and inconsiderate acts. The fictional police can receive reports about either category. No responsible alternate action completes the same mission.
- A player can decline to commit the offense, but then does not complete that mission or earn its completion points.
- Missions prescribe the required infraction, not a fixed method. Shared interactions and infractions can occur without a mission; timing, entry permission, distraction, stealth, and environmental manipulation remain player choices.
- Committing an infraction never causes an automatic penalty. Immediate world effects occur regardless of witnesses; a penalty requires a voluntary report with sufficient server-validated evidence. Traces alone do not prove authorship, and cameras do not report automatically.
- The tone is silly, friendly, and lightly satirical. Severe violence, killing, violent robbery, corruption, and similarly grave wrongdoing are outside the intended mission set.
- Task Race is the primary mode; Score Attack is the secondary mode.
- Progress, match currency, and reputation are separate values. Buying equipment does not directly reduce victory score.
- The server will authoritatively determine completion, offenses, evidence, reports, economy, penalties, and victory.

## Proposed gameplay loop

1. Receive missions involving minor neighborhood offenses and travel to relevant locations.
2. Decide whether, when, and how discreetly to commit each offense; declining leaves the mission incomplete.
3. Observe others, collect evidence, and decide whether to report.
4. Earn match currency, purchase surveillance or stealth equipment, and pursue the mode's victory condition.

Task definitions should describe required preparation, the rule-breaking action, completion conditions, and rewards as data. The initial catalog of twelve concepts and five detailed examples are in [MISSIONS_AND_INFRACTIONS.md](MISSIONS_AND_INFRACTIONS.md). Completing a mission requires its specific infraction and any other conditions; a player can also commit an infraction spontaneously. The M03 unpaid-snack sample remains a prototype rather than part of that initial catalog. There may be multiple ways to perform or conceal an act, but no responsible alternative that completes the same mission.

The game's use of "infraction" describes its fictional reporting rules, not a claim about real-world criminal law. The exact response and penalty for each kind of act remain open. A mission that depends on the dog's behavior must remain completable without unpredictable waiting.

## Proposed systems

### Evidence and reporting

Evidence is structured server-validated gameplay data. Candidate factors are identification, clarity, reliability, observation conditions, and time. Initial sources are direct witness observations, functioning cameras, and environmental traces. Traces alone cannot identify an offender; client screenshots or video files are never authoritative proof. A player must voluntarily submit a report naming an accused player and providing sufficient evidence before a penalty can be applied. Unsupported reports are rejected without punishment. The earlier suggestion of rewarding reports without evidence is superseded by v0.2. Exact thresholds, false-report outcomes, and anti-farming limits remain open.

### Modes

Task Race ends when someone completes all required missions. Declining a mission's offense leaves that mission incomplete. A time limit and progress-based tie breakers are configurable. Score Attack ends on a configurable timer and ranks mission and report points, less penalties. Whether offense actions also award separate points is unresolved; scoring the same action twice should be a deliberate rule. Both modes use the same mission, evidence, reporting, and equipment systems.

### Equipment and economy

Surveillance equipment can improve observation, while stealth equipment can make detection harder. Items have match currency prices, limits, and trade-offs. No item guarantees perfect detection or invisibility. The initial economy resets every match; persistent accounts and unlocks are outside scope.

## Open decisions

- Exact task counts, time limits, and Task Race tie breakers.
- The specific U.S. city or region, street density, and architectural references for the playable block.
- The exact outcomes within the three provisional consequence levels, including noncriminal incivilities.
- Evidence scoring and visibility model, including occlusion and identity uncertainty.
- How a player could gather meaningful evidence of an act in a private place, such as a bathroom, without implausible surveillance.
- Reward and penalty amounts, evidence retention, report rate limits, and anti-farming rules.
- How player-home targets remain viable after disconnects, and how dog behavior avoids unbounded waiting.
- Equipment catalog, costs, counterplay, and whether offense points belong in Score Attack.
- Final player count and map size after playtesting.

## Implemented through the M03 prototype

A capsule player can move and jump through a primitive neighborhood with a mouse-controlled follow camera. The prototype has five houses, a market, a gas station, a park, road and sidewalks, simple props, and exterior mission spots. Buildings are closed shells. Three sample missions can be completed by pressing E at the corresponding exterior spot: litter in the park, read a neighbor's mail, and take an unpaid snack from a market display. Each action completes one mission, grants configured prototype points, and records one local infraction event. The player confirmed all three interactions work visually. Their content and points are not finalized mode rules. There is no spontaneous action system, multi-step mission, multiplayer, evidence, reporting, or match loop. The block layout is recorded in `docs/design/M02_BLOCK_PLAN.md`.
