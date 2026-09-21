# Bootstrap baseline upgrade compatibility — 2026-09-21

## Disposition

**Accepted for Stage 0.** Bootstrap5WinFormUI is pinned to `aba102e33c48937fd92468791c287afda0a59e77`. SourceGrid remains pinned to `f4e457b43582bf01892f50bdc74aa480531e5944`. No integration or vendor source change was needed. The demo typography-profile work belongs to later stages.

## Resolution and compare

- Previous Bootstrap pin: `cceba3c969e28726935793a1c6ca3772bed60a35`.
- `git -C vendor/Bootstrap5WinFormUI fetch origin main` on 2026-09-21 resolved `origin/main` to `aba102e33c48937fd92468791c287afda0a59e77`, matching the planning snapshot.
- Target commit: 2026-09-21 05:00:12 UTC, `Merge pull request #71 from chung6a8m/codex/issue-70-toast-autohide`.
- The previous pin is a direct ancestor of the target (`merge-base --is-ancestor` exit 0).
- Compare: 106 commits, 197 changed files. Most changes are in unrelated controls, demos, tests, and documentation.
- `git diff --exit-code` between pins over `Theme/`, `Rendering/DpiScaler.cs`, the three integrated editor controls and all `BootstrapLookupBox*.cs` implementation files, and the framework `.csproj` exited 0. The consumed implementation and TFM definition are unchanged.

## API and behavior seams

The target source was checked against the integration's actual references and `docs/UPSTREAM_API_SEAMS.md`. `BootstrapThemeManager.CurrentTheme` and `ThemeChanged`, the `BootstrapTheme` constructor/immutable `Colors`, `Metrics`, and `Typography` properties, `BootstrapThemeColors`, `BootstrapThemeMetrics`, `BootstrapThemeTypography`, `BootstrapFontToken`, `DpiScaler`, the protected `BootstrapTextBox.Editor`/`OnEditorKeyDown` hooks, `BootstrapFormattedTextBox.RawValue`, and `BootstrapLookupBox.SelectedValue`/popup/keyboard/focus implementation remain unchanged. The old seam notes therefore continue to apply at the new pin. No semantic difference was found that called for a new regression test or integration-side change.

The SourceGrid pin and its editor lifecycle, selection, span, and navigation seams were not moved.

## Automated gates

| Baseline | Command | Result |
| --- | --- | --- |
| Previous pin | `git submodule update --init --recursive` | Exit 0; both pinned vendors checked out |
| Previous pin | `dotnet restore MyDmsVn.BootstrapSourceGrid.sln` | Exit 0 |
| Previous pin | `dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release --no-restore` | Exit 0; 0 warnings, 0 errors; both TFMs |
| Previous pin | `dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m` | Exit 0; net48 214/214; net8.0-windows 214/214 |
| New pin | `dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release --no-restore` | Exit 0; 0 warnings, 0 errors; both TFMs |
| New pin | focused test filter covering construction, lifecycle, theme, font, Views, selection, DPI, legacy/native editors, lookup interactions, registry, keyboard, compatibility, and demo, run separately with `-f net48` and `-f net8.0-windows` and bounded hang diagnostics | Exit 0; 186/186 on each TFM |
| New pin | `dotnet restore MyDmsVn.BootstrapSourceGrid.sln` | Exit 0 |
| New pin | `dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release` | Exit 0; 0 warnings, 0 errors; both TFMs |
| New pin | `dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m` | Exit 0; net48 214/214; net8.0-windows 214/214 |

Both `git -C vendor/Bootstrap5WinFormUI status --short` and `git -C vendor/sourcegrid status --short` produced no output after the final test run.

## Demo smoke

The release demo was launched as a desktop process on both `net8.0-windows` and `net48` at 96 DPI. On each runtime, its diagnostics reported the expected TFM and remained live through Light → Dark → Light, the consumer-font action followed by Dark, and Reset. The consumer-font diagnostic remained active after the theme change. On net8.0, screenshots were inspected for Light and Dark grid/header/selection rendering and for live Bootstrap text, formatted-text, and lookup editors opened with F2. The formatted editor displayed its currency mask; the lookup editor displayed the selected item's text. Typing `Con` into the lookup opened a two-result popup; clicking `Contoso` committed logical value `2` and updated the read-only display companion cell. A header click also exercised sort and changed row order. The demo stayed responsive and exited without a dialog or exception.

The automated integration suite additionally exercises selection/range/focus, scrolling/span navigation, text/formatted/lookup commit/cancel, popup keyboard and mouse selection, outside focus, application deactivation, theme switching with popup open, Reset editor sharing, and consumer font preservation. These are behavioral tests, not claims of separate pixel-level inspection of each scenario.

## Unresolved observations

No baseline regression was observed. Visual smoke was performed at 96 DPI; higher DPI and Designer inspection remain in the later initiative's full validation stage. No typography-profile feature was introduced in Stage 0.
