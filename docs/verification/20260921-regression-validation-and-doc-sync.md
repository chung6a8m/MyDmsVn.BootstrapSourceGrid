# Stage 2 regression validation and documentation sync — 2026-09-21

## Disposition

**Complete on 2026-09-21.** The linked worktree is `regression-doc-sync`. The accepted Bootstrap gitlink is `aba102e33c48937fd92468791c287afda0a59e77`, upgraded from `cceba3c969e28726935793a1c6ca3772bed60a35`. SourceGrid remains `f4e457b43582bf01892f50bdc74aa480531e5944`. The agent-run and owner-reported manual evidence are recorded separately below.

## Automated gate

Run from the linked worktree after `git submodule update --init --recursive` checked out both accepted pins:

| Command | Result |
| --- | --- |
| `dotnet restore MyDmsVn.BootstrapSourceGrid.sln` | Exit 0; all five solution projects restored |
| `dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release` | Exit 0; both TFMs; 0 warnings, 0 errors |
| `dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m` | Initial checkout: exit 0, 239/239 on each TFM. After both demo fixes: exit 0, 240/240 on each TFM; 0 skipped. |
| `dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Debug` | Exit 0; both TFMs; 0 warnings, 0 errors; required before Visual Studio Designer could load the custom control |

The final restore, Release build, and full dual-TFM test command exited 0 after both demo fixes. The final build reported 0 warnings and 0 errors; each TFM passed 240 tests without skips.

## Regression matrix

| Contract | Evidence |
| --- | --- |
| Construction, Dispose, handle and theme subscription | `BootstrapSourceGridConstructionTests`, `BootstrapSourceGridHandleLifecycleTests`, `BootstrapSourceGridThemeLifecycleTests`, `BootstrapSourceGridThemeStressTests` |
| Theme-owned and consumer-owned fonts; consumer Views | `BootstrapSourceGridFontTests`, `BootstrapSourceGridRuntimeViewThemeTests`; demo typography ownership tests |
| Runtime mode/profile/motion composition and external theme sync | `BootstrapSourceGridDemoTypographyTests`, theme adapter/lifecycle tests |
| Header/cell/selection rendering contracts | Header, cell View, selection style and theme adapter tests; manual 96 DPI Light/Dark inspection |
| Shared editor allocation, never-started and used disposal | `BootstrapEditorRegistryTests`, `BootstrapEditorOwnershipTests`, demo identity tests |
| Text, formatted RawValue and lookup SelectedValue commit/cancel | Text, formatted and lookup editor test classes; edit lifecycle tests |
| Lookup focus/popup keyboard, mouse, outside click, deactivation and theme switch | `BootstrapLookupBoxInteractionTests`, `BootstrapLookupBoxEditorTests` |
| Keyboard navigation, selection, spans, scrolling and sort | Keyboard, compatibility, edit lifecycle and demo tests; manual lookup-header sort and vertical scroll |
| Reset/repopulate without replacing grid or editor adapters | `BootstrapSourceGridDemoTests`, including the sorted-span reset regression, and `BootstrapSourceGridDemoTypographyTests` |
| Non-modal unattended failure paths | `WinFormsTestGuardTests`; full dual-TFM test runs used bounded hang diagnostics and completed without UI input |

## Demo header clipping found and corrected

At 96 DPI, the Base 16px profile visibly truncated the `Bootstrap formatted` column header in Light and Dark at the default 960x640 demo size. An observable-behavior assertion using the pinned SourceGrid View's `Measure` method failed before the fix: required width 164px, actual column width 160px, on the net8.0-windows test target at both 960x640 and 760x520. `DemoGridContent.ApplyTypographyLayout` now expands demo-owned columns only when a header requires more space. Product-level row/column sizing is unchanged. The focused six-case profile/window-size test then passed. A new net48 demo run at 96 DPI visibly showed the full header in Base 16px Light and Dark.

## Sorted-span Reset failure found and corrected

At 96 DPI, sorting the lookup column and clicking Reset raised a modal WinForms exception: SourceGrid reported the new `(3, 8)` span intersecting an old `(3, 9)` cell. The root cause was demo repopulation calling `Redim(40, 10)` on a grid that already had those dimensions. Pinned SourceGrid keeps existing rows and span references for a same-size `Redim`; sorting had moved those rows. A new `ResetAfterSortingSpannedRowsRepopulatesWithoutAnOverlap` test reproduced the exact `OverlappingCellException` on net8.0-windows before the fix. Demo repopulation now first calls `Redim(0, 0)`, removing old rows and span references, then restores its 40x10 shape on the same grid with the same shared editors. The focused test passed afterward on net8.0-windows and net48. The demo exception dialog was closed without modifying vendor code.

## Agent-run manual demo matrix at 96 DPI

Visual Studio's desktop reported a single 1920x1080 display; the running demo reported `DeviceDpi: 96`.

| Cell | Observation |
| --- | --- |
| Default Light/Dark | net48 demo inspected; toolbar, diagnostics, grid headers, visible rows and selection remained visible in both modes |
| Base 14px Light/Dark | net48 demo inspected, including Reduced motion and consumer grid font; diagnostics reflected the profile and ownership |
| Base 16px Light/Dark | net48 demo inspected after the header fix; full `Bootstrap formatted` header visible, toolbar and diagnostics contained |
| F2 and profile change | F2 opened the Bootstrap text editor; selecting the profile combo transferred focus and ended the edit through SourceGrid's normal validation path. Programmatic in-edit profile preservation is covered by the automated demo test. |
| Formatted editor | F2 opened the currency-formatted editor in Base 14px Light; Escape canceled and retained the original decimal value. |
| Lookup popup and theme switch | F2 opened the lookup; typing `Con` showed two matches. Clicking Dark while it was open ended the edit and closed the popup through normal focus transfer; the form stayed responsive. |
| Sort, scrolling and Reset | Sorting the lookup column changed row order; vertical scrolling moved through sorted rows. Reset initially exposed the overlap defect described above. After the fix, a new net48 demo run sorted the lookup column, clicked Reset, restored Item 01/02/03 order and remained responsive without a dialog. |
| Other combinations and interactions | Default with Reduced motion, a full Enter/Escape/Tab/Shift+Tab and mouse/outside-click lookup sequence for every profile, and span editing for every profile were not all repeated by the agent. The owner later reported the remaining manual cells as checked and OK; see the attestation below. |

## Agent-run DPI and Designer

- **100% (96 DPI):** net48 demo visual smoke completed. Tests also exercise logical DPI metrics and lifecycle refresh at 144 and 192 DPI; these are not substitutes for actual 150%/200% visual inspection.
- **150% and 200%:** not executed as actual Windows display scale settings. The available desktop exposed one display at 96 DPI. A cross-monitor move was therefore unavailable.
- **Visual Studio 2022 Community 17.14.41:** the solution opened. The first `MainForm` Designer load showed a missing `BootstrapSourceGrid` type because the current Debug/Any CPU output had not been built. After `dotnet build -c Debug` succeeded, the Designer opened and reopened, showing the form toolbar, diagnostics placeholder, help text and grid surface without a runtime bootstrap. This is a default Visual Studio design-target smoke; a separately confirmed net8 Designer target and toolbox placement/property serialization remain untested.

The limitations above describe only the agent-run session. The owner subsequently confirmed the remaining manual cells as checked and OK. The specific display configuration, Visual Studio target selection steps, and individual interaction observations from the owner's run were not supplied, so this record does not attribute them to the agent-run session.

## Owner manual attestation

On 2026-09-21, the owner responded to the outstanding-cell list and stated: “Các mục này đã được tôi manual kiểm chứng. Kết quả OK. Plan hoàn tất.” This confirms the previously open 150%/200% DPI visual checks, cross-monitor DPI move, net8 Designer workflow, and remaining per-profile interactions as passed manual validation. The owner requested documentation completion, push, and a PR. This attestation is owner-reported evidence; the agent did not independently repeat those checks.

## Public package gate recheck

On 2026-09-21, the official NuGet flat-container endpoint for `MyDmsVn.Bootstrap5WinFormUI` returned 404; `SourceGrid` listed only `4.4.0`. The accepted Bootstrap source tree has no root license file. No candidate package corresponding to the accepted Bootstrap SHA or SourceGrid SHA was available from that public feed, so package/source equivalence, exact package dependency pins and package-level license obligations cannot be verified. The 2026-09-09 internal-feed observation was not rerun. Public publication remains blocked; source builds continue with pinned submodules and `ProjectReference`.

## Repository hygiene and completion

The final dual-TFM build/test run passed as recorded above. The final `git diff --check` found no whitespace errors; both vendor worktrees had empty `git status --short` output at the accepted pins. A tracked-file audit found no `bin/`, `obj/`, `TestResults/`, `artifacts/`, `.vs/`, dump or `.nupkg` paths. With the owner's manual attestation, the initiative is marked complete and its dated plan set is archived.
