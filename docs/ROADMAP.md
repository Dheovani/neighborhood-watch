# Roadmap

Work moves to the next milestone only after approval. Acceptance criteria describe intended outcomes; later milestones are not implemented yet.

| Milestone | Acceptance criteria | Validation |
|---|---|---|
| **M01 Foundation** | Godot .NET project builds; main scene launches; capsule moves and jumps on collidable ground; mouse controls follow camera; VS Code and CLI workflow documented. | `scripts/dev/validate.ps1` including movement smoke; visual playtest of movement, camera, and ground collision. |
| **M02 Neighborhood Prototype** | One readable suburban block with houses, street, sidewalks, obstacles, collision, and interaction points. | Headless scene load; visual walk-through; verify collisions and reachability. |
| **M03 Tasks and Offenses** | Data-defined sample tasks support legitimate and illegitimate methods; progress and offense events are explicit. | Unit tests for task rules; scene interaction playtest; verify an illegal method creates an offense event. |
| **M04 Multiplayer Foundation** | Dedicated headless server supports multiple players, synchronized essential state, and server validation of actions. | Two or more local clients; reject forged completion; disconnection and latency checks. |
| **M05 Surveillance and Reporting** | Server-derived evidence and reports resolve rewards, penalties, and reputation with abuse limits. | Deterministic rule tests; multi-client witness and false report scenarios. |
| **M06 Modes and Economy** | Task Race and Score Attack share systems; configurable victory and tie rules; match currency buys equipment without reducing progress. | Mode and economy tests; full local match playtests. |

M01 is the active milestone. M02 is the recommended next milestone after M01's visual playtest and explicit approval.
