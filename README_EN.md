# Dalamud ACT Compat — Taiwan build

This independent checkout targets Taiwan Dalamud API 13 / .NET 9 and game version `2026.09.14.0000.0000`. It is an offline-tested candidate, pending Taiwan in-game acceptance.

The Taiwan IINACT branch `PlusoneChiang/IINACT:tc/api13_net9` is the parser base. Existing DACT features are retained; Matcha is excluded at the user's request because no verified Taiwan adaptation was found. The default UI is Traditional Chinese. Parser region is fixed to Taiwan even after importing a profile.

Build with .NET SDK 10, .NET 9 Windows Desktop runtime and 7-Zip:

```powershell
./tools/build-taiwan.ps1
```

The script verifies pinned reference hashes, builds and runs offline checks, then writes `artifacts/taiwan/DalamudActCompat-TW-candidate.zip`. See `tools/taiwan-dependencies.lock.json` for sources and checksums, and the Chinese README for the full acceptance scope.

The subscription feed is intentionally empty until a Taiwan release is validated. The old CN repair utility is not published from this branch. Native hooks, VFX, alliance layouts, SilverDasher FateInfo discovery and real FFLogs TW data still require target-client/service validation. No CN/global ranking population is substituted for Taiwan.
