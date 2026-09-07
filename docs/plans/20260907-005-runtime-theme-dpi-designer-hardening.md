# Runtime Theme, DPI, and Designer Hardening Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Harden the themed grid for repeated runtime theme changes, DPI transitions, handle recreation, WinForms Designer construction, and resource/event lifecycle on both target frameworks.

**Architecture:** Theme/DPI refresh updates shared integration-owned Views and metrics, never grid data. The control owns exactly one global theme subscription while alive and only its own GDI resources. Designer construction uses the framework's safe default `CurrentTheme` and does not depend on runtime services.

**Tech Stack:** Windows Forms lifecycle/DPI APIs, Bootstrap `DpiScaler`, SourceGrid, NUnit STA tests, Visual Studio Designer manual smoke.

**Spec:** PRD FR-03/FR-09/FR-10/FR-12; COMPATIBILITY sections 8/9/12; TESTING sections 4/10/11; decisions D-006/D-012/D-013/D-014.

## Fixed DPI mappings

Use only integration-owned metrics:

```text
CellPadding      <- CurrentTheme.Metrics.SpacingXS (4 at 96 DPI)
CellBorderWidth  <- CurrentTheme.Metrics.BorderWidth (1 at 96 DPI)
FocusBorderWidth <- CurrentTheme.Metrics.FocusBorderWidth (2 at 96 DPI)
```

Scale through `DpiScaler.Scale(logicalPixels, dpi)`. Do not scale SourceGrid row heights, column widths, scrollbar dimensions, or application-assigned sizes.

---

### Task 1: Make theme refresh idempotent and state-preserving

**Files:**
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Controls/BootstrapSourceGrid.cs`
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Internal/BootstrapSourceGridStyleApplicator.cs`
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridThemeStressTests.cs`

- [ ] **Step 1: Write a repeated-theme-switch regression test**

Create data, a span, custom View, active position, selection, row/column sizes, and a consumer font where applicable. Switch Light/Dark at least 20 times in a loop, always restoring the original global theme in `finally`.

After every switch assert logical state is unchanged:

```text
cell values
RowsCount/ColumnsCount
row height/column width
span owner/range
custom View identity
selected ranges
active position
```

- [ ] **Step 2: Make `ApplyBootstrapTheme()` idempotent**

One call must only:

```text
capture current snapshot
recompute integration DPI metrics
update shared integration Views
update still-integration-owned selection visuals
update theme-owned Font when enabled
set safe grid BackColor/ForeColor
Invalidate/PerformLayout only when needed
```

It must not enumerate and rewrite application cells.

- [ ] **Step 3: Avoid event recursion**

Changing `Font`, `BackColor`, selection visuals, or Views during theme apply must not trigger another theme apply. If a guard is necessary use a private `_applyingTheme` boolean scoped with `try/finally`; do not suppress normal consumer events globally.

- [ ] **Step 4: Run test on both TFMs**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --filter BootstrapSourceGridThemeStressTests
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapSourceGridThemeStressTests
```

- [ ] **Step 5: Commit idempotent theme refresh**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridThemeStressTests.cs
git commit -m "fix: harden repeated SourceGrid theme changes"
```

---

### Task 2: Implement DPI refresh for integration-owned metrics

**Files:**
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Internal/BootstrapSourceGridDpiMetrics.cs`
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Controls/BootstrapSourceGrid.cs`
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridDpiLifecycleTests.cs`

- [ ] **Step 1: Lock pure scale values**

For a default theme assert expected framework-derived values:

```text
96 DPI:  padding 4, border 1, focus 2
120 DPI: padding 5, border 1, focus 3
144 DPI: padding 6, border 2, focus 3
192 DPI: padding 8, border 2, focus 4
```

These values follow `DpiScaler.Scale` with AwayFromZero midpoint rounding.

- [ ] **Step 2: Add a narrow `CurrentDpi` helper**

Use `DeviceDpi` when valid; before handle creation fall back to `DpiScaler.DefaultDpi` (96). Keep this helper private/internal and testable.

Do not create a graphics object solely to discover DPI during constructor execution.

- [ ] **Step 3: Refresh metrics on DPI lifecycle notification**

Override the supported WinForms DPI callback used by the pinned Bootstrap framework (`OnDpiChangedAfterParent` where available in the dual-target surface), call base first or according to the framework's proven convention, recompute integration metrics, update shared Views/selection border, then invalidate/layout.

If the compiler proves the callback differs by target framework, isolate only the callback shim with conditional compilation; share `RefreshDpiMetrics()` across both targets.

- [ ] **Step 4: Prove no application dimension double-scaling**

Set explicit:

```csharp
grid.Rows[0].Height = 37;
grid.Columns[0].Width = 123;
```

Call the internal DPI refresh seam with multiple DPI values and assert those SourceGrid dimensions remain 37/123.

- [ ] **Step 5: Run DPI lifecycle tests on both TFMs**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --filter BootstrapSourceGridDpiLifecycleTests
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapSourceGridDpiLifecycleTests
```

- [ ] **Step 6: Commit DPI lifecycle**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridDpiLifecycleTests.cs
git commit -m "feat: refresh Bootstrap SourceGrid metrics on DPI changes"
```

---

### Task 3: Harden handle creation/destruction and disposal

**Files:**
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridHandleLifecycleTests.cs`
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Controls/BootstrapSourceGrid.cs` only if test reveals a lifecycle defect.

- [ ] **Step 1: Add repeated handle recreation test**

On STA, host the grid in a temporary `Form` without showing modal UI. Force handle creation with `CreateControl()` / handle access, then recreate parent/control lifecycle in a bounded loop using framework-supported methods.

Assert:

```text
control remains usable
theme switch still applies once
no duplicate child/editor controls
no exception after parent handle recreation
```

Never call unbounded `Application.Run` or `ShowDialog`.

- [ ] **Step 2: Add dispose-after-handle test**

Construct, create handle, resolve Views, switch theme, dispose, then switch global theme again. No callback may access disposed handles/resources.

- [ ] **Step 3: Add owned-resource probes**

Use internal state/test seams rather than finalizers. Assert integration-owned font reference becomes null after disposal and theme subscription flag is false. Shared Views must not retain disposable per-control GDI handles that require separate unmanaged cleanup.

- [ ] **Step 4: Run with hang diagnostics**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --filter BootstrapSourceGridHandleLifecycleTests --blame-hang --blame-hang-timeout 5m
```

Expected: PASS without visible modal UI.

- [ ] **Step 5: Commit lifecycle hardening**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridHandleLifecycleTests.cs
git commit -m "test: harden SourceGrid handle and disposal lifecycle"
```

---

### Task 4: Make Designer metadata and constructor behavior explicit

**Files:**
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Controls/BootstrapSourceGrid.cs`
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridDesignerContractTests.cs`
- Modify: `docs/COMPATIBILITY.md` only if the verified Designer contract needs clarification.

- [ ] **Step 1: Add reflection-level Designer contract tests**

Assert:

```text
ToolboxItem(true)
public parameterless constructor
no required runtime service constructor arguments
Bootstrap-specific internal helpers are not browsable public properties
```

If later public Bootstrap-specific properties are introduced, ensure they have `Category`, `Description`, and stable `DefaultValue`/serialization semantics.

- [ ] **Step 2: Ensure constructor does no runtime-only work**

Constructor must not:

```text
show UI
start timers/background workers
open files/network
access a Form/Parent that may not exist
force a handle
require ThemeManager initialization beyond its safe default CurrentTheme
```

- [ ] **Step 3: Add `DesignMode` branches only for proven runtime-only operations**

Do not wrap normal theme initialization in broad `if (DesignMode) return;`, because Designer needs representative visual state. Use design-mode checks only if a concrete operation is unsafe in Designer.

- [ ] **Step 4: Build both TFMs**

```powershell
dotnet build src/MyDmsVn.BootstrapSourceGrid/MyDmsVn.BootstrapSourceGrid.csproj -c Release -f net48
dotnet build src/MyDmsVn.BootstrapSourceGrid/MyDmsVn.BootstrapSourceGrid.csproj -c Release -f net8.0-windows
```

- [ ] **Step 5: Commit Designer contract changes**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridDesignerContractTests.cs docs/COMPATIBILITY.md
git commit -m "fix: harden BootstrapSourceGrid designer contract"
```

---

### Task 5: Perform manual DPI and Designer matrix

**Files:**
- Create: `docs/verification/20260907-stage-3-dpi-designer.md`
- Use: `samples/MyDmsVn.BootstrapSourceGrid.Demo`

- [ ] **Step 1: Add a non-modal diagnostic page to demo if Stage 2 does not already expose one**

Show current:

```text
TFM/runtime
DeviceDpi
Current theme mode/name
computed padding/border/focus metrics
consumer font mode vs theme font mode
```

This is demo diagnostics only, not public product API.

- [ ] **Step 2: Run DPI matrix**

Verify at 100%, 125%, 150%, 200%:

```text
cell text not clipped
header text/sort indicator aligned
borders visible and proportional
alternate rows stable
selection/focus visible
editor bounds aligned (smoke only; Stage 4 hardens editors)
native scrollbars/layout remain functional
```

- [ ] **Step 3: Run multi-monitor DPI transition when environment permits**

Move the running demo between monitors/scales and record whether `DeviceDpi` and integration metrics update without data/selection loss.

If the available environment lacks mixed-DPI monitors, record `Not exercised: environment limitation`; do not mark the requirement removed.

- [ ] **Step 4: Run WinForms Designer matrix**

For both target workflows in supported Visual Studio:

```text
open form designer
place/control instantiate BootstrapSourceGrid
resize/dock/anchor
save/close/reopen
build/run
repeat open/close at least 3 times
```

Record Visual Studio version and result.

- [ ] **Step 5: Write verification evidence**

`docs/verification/20260907-stage-3-dpi-designer.md` must contain:

```text
machine/OS/Visual Studio context
TFMs tested
DPI rows with Pass/Fail/Not exercised
Designer rows with Pass/Fail
findings/links to fixes
final stage verdict
```

Do not place screenshots in Git unless they communicate a durable regression that text cannot describe.

- [ ] **Step 6: Commit verification record**

```powershell
git add samples docs/verification/20260907-stage-3-dpi-designer.md
git commit -m "docs: record DPI and designer verification"
```

---

### Task 6: Full Stage 3 gate

- [ ] **Step 1: Full restore/build/test**

```powershell
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
```

- [ ] **Step 2: Verify no vendor dirt**

```powershell
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Expected: no output.

- [ ] **Step 3: Review state-preservation invariant**

No theme/DPI/handle lifecycle code may call SourceGrid data resize/reset APIs or selection reset APIs simply to repaint.

## Stage completion criteria

- [ ] Theme refresh is repeatable/idempotent.
- [ ] Data, spans, sizes, selection and active position survive repeated theme changes.
- [ ] Bootstrap-owned DPI metrics scale via `DpiScaler`; SourceGrid-owned dimensions are untouched.
- [ ] Handle recreation/disposal is safe.
- [ ] Theme event/font resources do not outlive the control.
- [ ] Designer construction is safe and manually verified.
- [ ] 100/125/150/200% DPI matrix is recorded.
- [ ] Both TFMs pass automated tests.
- [ ] Vendor submodules remain clean.
