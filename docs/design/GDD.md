# Game Design Document

## Status

This document separates **confirmed direction**, **proposed mechanics**, **open decisions**, and **implemented features**. The initial product specification is the source for future design work; numerical tuning awaits playtesting.

## Confirmed direction

- Third person, stylized 3D social stealth in a colorful suburban neighborhood.
- Competitive multiplayer, initially targeting 4 to 8 players on Windows PC.
- Every player can complete tasks, commit offenses, observe neighbors, collect evidence, and report. There are no fixed innocent or impostor roles.
- Risky methods should save time while exposing players to detection and penalties.
- Task Race is the primary mode; Score Attack is the secondary mode.
- Progress, match currency, and reputation are separate values. Buying equipment does not directly reduce victory score.
- The server will authoritatively determine completion, offenses, evidence, reports, economy, penalties, and victory.

## Proposed gameplay loop

1. Receive ordinary neighborhood tasks and travel to relevant locations.
2. Choose a slower legitimate method or a faster illegitimate method where available.
3. Observe others, collect evidence, and decide whether to report.
4. Earn match currency, purchase surveillance or stealth equipment, and pursue the mode's victory condition.

Task definitions should describe available completion methods, conditions, time or effort costs, and rewards as data. Example: take garbage to the designated bin, or dump it on a neighbor's property and create an offense event. Some tasks may eventually have more than two methods; the system should not encode a fixed binary choice.

## Proposed systems

### Evidence and reporting

Evidence is structured server-validated gameplay data. Candidate factors are identification, clarity, reliability, observation conditions, and time. Client screenshots or video files are never authoritative proof. Reports may be submitted without evidence, but repeated submissions must not become a currency farm. Weak and strong evidence may change rewards and penalties; false reports may damage reputation or temporarily restrict reporting. Exact thresholds and rewards remain open.

### Modes

Task Race ends when someone completes all required tasks. A time limit and progress-based tie breakers are configurable. Score Attack ends on a configurable timer and ranks task, offense, and report points, less penalties. Both modes use the same task, evidence, reporting, and equipment systems.

### Equipment and economy

Surveillance equipment can improve observation, while stealth equipment can make detection harder. Items have match currency prices, limits, and trade-offs. No item guarantees perfect detection or invisibility. The initial economy resets every match; persistent accounts and unlocks are outside scope.

## Open decisions

- Exact task counts, time limits, and Task Race tie breakers.
- Evidence scoring and visibility model, including occlusion and identity uncertainty.
- Reward and penalty amounts, report rate limits, and anti-farming rules.
- Equipment catalog, costs, counterplay, and whether offense points belong in Score Attack.
- Final player count and map size after playtesting.

## Implemented in M01

A capsule player can move and jump on a flat test ground with a mouse-controlled follow camera. No gameplay loop, multiplayer, or scoring is implemented.
