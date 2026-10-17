# Game Design Document

## Status

This document separates **confirmed direction**, **proposed mechanics**, **open decisions**, and **implemented features**. Direct player clarification on 2026-10-17 supersedes conflicting wording in the initial specification and the later concept attachment. Numerical tuning awaits playtesting.

## Confirmed direction

- Third person, stylized 3D social stealth in an ordinary U.S. neighborhood with houses and gardens, some shops, a gas station, and a park. The setting is not intended to signal low income or associate any socioeconomic group with wrongdoing.
- Competitive multiplayer, initially targeting 4 to 8 players on Windows PC.
- Every player can complete tasks, commit offenses, observe neighbors, collect evidence, and report. There are no fixed innocent or impostor roles.
- Missions involve minor reportable infractions: petty crimes or merely rude and inconsiderate acts. The fictional police can receive reports about either category. No responsible alternate action completes the same mission.
- A player can decline to commit the offense, but then does not complete that mission or earn its completion points.
- The tone is silly, friendly, and lightly satirical. Severe violence, killing, violent robbery, corruption, and similarly grave wrongdoing are outside the intended mission set.
- Task Race is the primary mode; Score Attack is the secondary mode.
- Progress, match currency, and reputation are separate values. Buying equipment does not directly reduce victory score.
- The server will authoritatively determine completion, offenses, evidence, reports, economy, penalties, and victory.

## Proposed gameplay loop

1. Receive missions involving minor neighborhood offenses and travel to relevant locations.
2. Decide whether, when, and how discreetly to commit each offense; declining leaves the mission incomplete.
3. Observe others, collect evidence, and decide whether to report.
4. Earn match currency, purchase surveillance or stealth equipment, and pursue the mode's victory condition.

Task definitions should describe required preparation, the rule-breaking action, completion conditions, and rewards as data. Player-provided examples are: buy several listed products at a market while leaving one unpaid; walk a dog in the park and leave its waste; use the gas station bathroom and leave without flushing; read a neighbor's mail; throw a stone at a window; or park in an accessible parking space. These illustrate context-sensitive missions rather than finalized content. Completing a mission registers a reportable infraction event. There may be multiple ways to perform or conceal the act, but no responsible alternative that completes the same mission.

The game's use of "infraction" describes its fictional reporting rules, not a claim about real-world criminal law. The exact response and penalty for each kind of act remain open. A mission that depends on the dog's behavior must remain completable without unpredictable waiting.

## Proposed systems

### Evidence and reporting

Evidence is structured server-validated gameplay data. Candidate factors are identification, clarity, reliability, observation conditions, and time. Client screenshots or video files are never authoritative proof. Reports may be submitted without evidence, but repeated submissions must not become a currency farm. Weak and strong evidence may change rewards and penalties; false reports may damage reputation or temporarily restrict reporting. Exact thresholds and rewards remain open.

### Modes

Task Race ends when someone completes all required missions. Declining a mission's offense leaves that mission incomplete. A time limit and progress-based tie breakers are configurable. Score Attack ends on a configurable timer and ranks mission and report points, less penalties. Whether offense actions also award separate points is unresolved; scoring the same action twice should be a deliberate rule. Both modes use the same mission, evidence, reporting, and equipment systems.

### Equipment and economy

Surveillance equipment can improve observation, while stealth equipment can make detection harder. Items have match currency prices, limits, and trade-offs. No item guarantees perfect detection or invisibility. The initial economy resets every match; persistent accounts and unlocks are outside scope.

## Open decisions

- Exact task counts, time limits, and Task Race tie breakers.
- The specific U.S. city or region, street density, and architectural references for the playable block.
- How the game's fictional police respond to different infractions, including noncriminal incivilities.
- Evidence scoring and visibility model, including occlusion and identity uncertainty.
- How a player could gather meaningful evidence of an act in a private place, such as a bathroom, without implausible surveillance.
- Reward and penalty amounts, report rate limits, and anti-farming rules.
- Equipment catalog, costs, counterplay, and whether offense points belong in Score Attack.
- Final player count and map size after playtesting.

## Implemented in M01

A capsule player can move and jump on a flat test ground with a mouse-controlled follow camera. No gameplay loop, multiplayer, or scoring is implemented.
