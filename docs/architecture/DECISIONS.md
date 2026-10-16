# Architecture Decisions

## Accepted for M01

1. **Godot 4.7.2 .NET, C#, and `net8.0`.** The available Godot ZIP reports `4.7.2.stable.mono`; the installed .NET 10 SDK and .NET 8 runtime support this target. Recheck compatibility when upgrading Godot.
2. **Text scenes and a small C# controller.** This supports the VS Code-first workflow and keeps the foundation inspectable without editor-only work.
3. **GL Compatibility renderer.** The original project shell forced Windows D3D12. Removing that override avoids a platform-specific setting and is sufficient for primitive M01 visuals. Revisit renderer choice for art and performance testing.
4. **Local movement only in M01.** It allows a playable controller without pretending it is network authoritative. M04 replaces this ownership with server-validated movement.
5. **No extra infrastructure.** No database, backend, dependency injection framework, asset pipeline, or networking code exists before a concrete need.

## Proposed for later milestones

- Data-driven task methods and configurable match mode policies.
- Server-owned offense events, evidence evaluation, reporting outcomes, and balances.
- Godot high-level multiplayer API, ENet, and a dedicated headless server.

## Unresolved

- Movement replication and prediction model for acceptable latency.
- Evidence scoring and observation rules.
- Exact mode tie breakers and scoring numbers.
