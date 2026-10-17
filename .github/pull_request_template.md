## Change

<!-- What changed and why? Link the issue or roadmap item when applicable. -->

## Scope

- Milestone: <!-- M01, documentation only, or other approved scope -->
- Issue: <!-- Closes #123, if applicable -->

## Validation

<!-- List exact commands and results. For visual changes, include a brief playtest result or screenshot. -->

- [ ] `dotnet format NeighborhoodWatch.csproj --verify-no-changes --no-restore`
- [ ] `dotnet build NeighborhoodWatch.csproj --no-restore`
- [ ] `./scripts/dev/validate.ps1` (Windows; includes Godot scene and movement smoke checks)

## Review checks

- [ ] Gameplay or architecture documentation reflects any confirmed decision or implemented feature.
- [ ] Proposed mechanics remain labeled as proposed or unresolved.
- [ ] If this affects future multiplayer rules, authority remains with the server.
- [ ] No commit or asset introduces secrets or unlicensed material.
