# Stage 1 verification: SourceGrid Demo typography profiles

Date: 2026-09-21

Scope: `docs/plans/20260921-003-sourcegrid-demo-typography-profiles.md`. Stage 2 remains open.

## Implementation

- Added demo-only Default, Base 14px, and Base 16px profile definitions. Default uses the exact `BootstrapThemeTypography.Default` object; no framework default changes.
- Added a complete demo theme factory. Mode, typography, and reduced motion settings can be changed independently. Unknown custom typography remains the same object until a known profile is explicitly selected.
- Added a Base font selector and Reduced motion checkbox to the existing demo form. External theme changes update these controls without publishing another theme.
- Applied Body typography to the native demo shell with owned font lifetime management. Profile changes retain the existing grid, cells, selection, and shared editor adapters. Demo row heights follow the selected typography and editor preferred heights; product row and column sizing remains unchanged.
- Corrected integration-owned grid font comparison to use the requested token. A missing font family may resolve to an installed font while a native scrollbar retains the original `Font` reference; comparing the resolved name could dispose that still-referenced font on theme change.

## Automated evidence

- Started with a failing selector test, then implemented the UI. A row-height test failed at the previous 20px rows before the demo-only layout adjustment. The unavailable-font regression exposed a disposed-font failure before the requested-token correction.
- `dotnet restore MyDmsVn.BootstrapSourceGrid.sln`: passed.
- `dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release`: passed, both TFMs, zero warnings/errors.
- `dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m`: passed, 237/237 on `net48` and 237/237 on `net8.0-windows`.
- New tests cover token values and references, custom typography, startup behavior, one notification per setting action, external synchronization, native shell font, consumer grid font, grid/editor identity, active text editor and lookup popup, diagnostics, row heights, and toolbar bounds at 960x640 and 760x520.

## Manual demo smoke

- Opened the `net8.0-windows` Release demo at 960x640 and visually checked Base 16px with Dark mode and Reduced motion. The toolbar, diagnostics, help text, grid header, and visible rows remained usable.
- Started a text editor and selected Base 14px; the form stayed responsive and diagnostics reflected the new profile.
- Enabled consumer font, opened the lookup popup in Dark mode, and selected Base 16px. The form remained responsive and diagnostics continued to report consumer font ownership.

Final gate: `git submodule update --init --recursive`, restore, build, and full tests passed after all implementation edits. `git diff --check` passed. Both `git -C vendor/Bootstrap5WinFormUI status --short` and `git -C vendor/sourcegrid status --short` produced no output. No vendor source or tests were modified.

## PR #15 review follow-up

- Reproduced font reuse failure when two different unavailable family names resolve to the same installed font. The grid now keeps that existing owned `Font`, updates the requested token, and verifies actual assignment before disposing the prior font.
- Reproduced stale demo row heights after a grid DPI event. The demo schedules row layout after the grid's DPI metrics refresh, and also recalculates when the form first appears. The queued callback checks disposal state.
- Added focused dual-TFM regressions for both cases. The review fixes remain demo/integration-owned; vendor source and tests are unchanged.
- Final review-fix gate: restore and Release build passed with zero warnings/errors; full test project passed 239/239 on `net48` and 239/239 on `net8.0-windows` with bounded hang diagnostics.
