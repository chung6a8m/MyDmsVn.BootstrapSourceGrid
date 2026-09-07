# Cell, Header, and Selection Theming Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Apply Bootstrap semantic visuals to SourceGrid default cells, alternating rows, row/column headers, selection, and focus while preserving custom consumer Views and all SourceGrid behavior.

**Architecture:** SourceGrid cell assignment cannot be intercepted through the public indexer because its setter calls a private `InsertCell`. Instead, `BootstrapSourceGrid` overrides virtual `GetCell(int,int)` and lazily substitutes only known SourceGrid default View singletons with integration-owned shared Views. Custom consumer Views are never replaced. Shared Views read/update integration theme state; no full-grid rewrite is required on theme changes.

**Tech Stack:** SourceGrid `Cells.Views.*`, DevAge.Drawing visual elements, SourceGrid selection decorator properties, Bootstrap theme snapshot/DPI metrics from Stage 1, NUnit.

**Spec:** PRD FR-04/FR-05/FR-06/FR-07; architecture sections 5/6/8/12; decisions D-003/D-004/D-007/D-015.

## Verified SourceGrid integration points

At `f4e457b...`:

- `SourceGrid.Grid.GetCell(int,int)` is virtual through the grid hierarchy.
- `grid[row,col] = cell` uses private `InsertCell`, so subclass assignment interception is unavailable without a vendor patch.
- `Cells.Virtual.CellVirtual.View` is public and intentionally shareable.
- ordinary cells default to `Cells.Views.Cell.Default`.
- column headers default to `Cells.Views.ColumnHeader.Default`.
- row headers default to `Cells.Views.RowHeader.Default`.
- `Cells.Views.ViewBase` exposes `BackColor`, `ForeColor`, `Border`, `Padding`, `Font` and is designed to be shared.
- `CellContext.Position` is available during View preparation.
- non-themed DevAge visual elements `DevAge.Drawing.VisualElements.ColumnHeader` and `RowHeader` expose `BackColor` and `Border`, allowing Bootstrap colors without losing SourceGrid header interfaces/sort indicator behavior.
- selection rendering consumes `Selection.BackColor`, `Selection.FocusBackColor`, and `Selection.Border`.

---

### Task 1: Implement integration-owned ordinary cell View with dynamic striping

**Files:**
- Create: `src/MyDmsVn.BootstrapSourceGrid/Views/BootstrapSourceGridCellView.cs`
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridCellViewTests.cs`

**Interfaces:**
- Consumes current theme snapshot + DPI metrics.
- Produces one shared SourceGrid View whose background depends on `CellContext.Position.Row`.

- [ ] **Step 1: Write failing View tests**

Construct a `BootstrapSourceGrid`, `Redim(3,1)`, assign ordinary `SourceGrid.Cells.Cell` instances, then call `grid.GetCell(...)` so lazy integration is exercised.

Assert:

```text
row 0 default cell -> integration-owned View
row 1 default cell -> same integration-owned View instance
consumer custom View -> exact custom View instance remains
```

Do not assert static `View.BackColor` for alternate rows because one shared View dynamically selects background during `PrepareView`.

- [ ] **Step 2: Implement `BootstrapSourceGridCellView`**

Subclass `SourceGrid.Cells.Views.Cell`.

Store current integration snapshot and current `DevAge.Drawing.RectangleBorder`/`Padding` values in fields owned by the View.

Override `PrepareView(CellContext context)`:

```csharp
protected override void PrepareView(SourceGrid.CellContext context)
{
    var snapshot = _snapshot;

    BackColor = (context.Position.Row & 1) == 0
        ? snapshot.CellBackColor
        : snapshot.AlternateCellBackColor;
    ForeColor = snapshot.CellForeColor;
    Border = _border;
    Padding = _padding;

    base.PrepareView(context);
}
```

Keep `Font = null` so SourceGrid uses `grid.Font`; this makes the Stage 1 theme-font/consumer-font contract apply automatically.

Provide internal `ApplyTheme(snapshot, dpiMetrics)` that replaces fields once per theme/DPI update, not once per cell.

- [ ] **Step 3: Build border/padding with DevAge value types**

Use:

```csharp
new DevAge.Drawing.RectangleBorder(
    new DevAge.Drawing.BorderLine(snapshot.BorderColor, 1))
```

only if that creates the intended four-side border in the pinned DevAge contract. If the constructor applies a different edge set, use the explicit `RectangleBorder` constructor already used by SourceGrid to set the required right/bottom or all-side edges. Verify visually in the Stage 2 demo smoke.

Construct `DevAge.Drawing.Padding` from `BootstrapSourceGridDpiMetrics.CellPadding` only after Stage 1 proved it is an integration-owned metric and does not double-scale SourceGrid layout.

- [ ] **Step 4: Run focused tests**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapSourceGridCellViewTests
```

Expected: PASS after wiring in Task 2.

---

### Task 2: Lazily substitute only known SourceGrid default Views

**Files:**
- Create: `src/MyDmsVn.BootstrapSourceGrid/Internal/BootstrapSourceGridStyleApplicator.cs`
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Controls/BootstrapSourceGrid.cs`
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridCellViewTests.cs`

**Interfaces:**
- Consumes `ICellVirtual` returned by SourceGrid.
- Produces deterministic default-View substitution without touching custom Views.

- [ ] **Step 1: Add failing lazy-substitution tests**

Minimum cases:

```text
GetCell_ReplacesViewsCellDefault
GetCell_DoesNotReplaceCustomCellView
IndexerGetBeforeGetCell_DoesNotRequireBootstrapWrapperType
ThemeChange_DoesNotRequireReassigningCells
```

- [ ] **Step 2: Implement style applicator identity checks**

The applicator must use reference identity against SourceGrid's static default instances, not type-only checks:

```csharp
if (ReferenceEquals(cell.View, SourceGrid.Cells.Views.Cell.Default))
{
    cell.View = _cellView;
}
```

Header identities are handled in later tasks before the generic cell check so a header does not lose its specialized View.

If `cell.View` is already one of this integration's owned Views, leave it unchanged.

Any other View is consumer-owned and must remain unchanged.

- [ ] **Step 3: Override `GetCell` narrowly**

In `BootstrapSourceGrid`:

```csharp
public override SourceGrid.Cells.ICellVirtual GetCell(int row, int column)
{
    var cell = base.GetCell(row, column);
    _styleApplicator.ApplyDefaultView(cell);
    return cell;
}
```

The applicator must accept null because sparse grid positions can be empty.

Do not hide/redeclare the non-virtual indexer. Do not patch SourceGrid `InsertCell`.

- [ ] **Step 4: Prove common SourceGrid assignment remains unchanged**

```csharp
grid.Redim(2, 2);
var cell = new SourceGrid.Cells.Cell("A");
grid[0, 0] = cell;
Assert.That(grid[0, 0], Is.SameAs(cell));
Assert.That(grid.GetCell(0, 0), Is.SameAs(cell));
```

The integration changes only the default View reference when SourceGrid asks through `GetCell`.

- [ ] **Step 5: Commit lazy View application**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid/Controls src/MyDmsVn.BootstrapSourceGrid/Internal src/MyDmsVn.BootstrapSourceGrid/Views tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridCellViewTests.cs
git commit -m "feat: apply Bootstrap default cell views lazily"
```

---

### Task 3: Implement Bootstrap column-header View without losing sort indicators

**Files:**
- Create: `src/MyDmsVn.BootstrapSourceGrid/Views/BootstrapSourceGridColumnHeaderView.cs`
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridHeaderViewTests.cs`
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Internal/BootstrapSourceGridStyleApplicator.cs`

**Interfaces:**
- Preserves `SourceGrid.Cells.Views.ColumnHeader` sort-indicator behavior while replacing OS-themed header background with DevAge's non-themed programmable header visual element.

- [ ] **Step 1: Write failing column-header substitution test**

```csharp
grid.Redim(1, 1);
var header = new SourceGrid.Cells.ColumnHeader("Name");
grid[0, 0] = header;

var resolved = grid.GetCell(0, 0);
Assert.That(resolved.View, Is.TypeOf<BootstrapSourceGridColumnHeaderView>());
```

Also assert a custom `SourceGrid.Cells.Views.ColumnHeader` instance assigned before insertion remains unchanged.

- [ ] **Step 2: Implement the specialized View**

Subclass `SourceGrid.Cells.Views.ColumnHeader` so `ElementSort` and `PrepareVisualElementSortIndicator` remain SourceGrid-owned.

In the constructor replace themed background:

```csharp
Background = new DevAge.Drawing.VisualElements.ColumnHeader();
```

In `ApplyTheme(...)`:

```csharp
ForeColor = snapshot.HeaderForeColor;
var background = (DevAge.Drawing.VisualElements.ColumnHeader)Background;
background.BackColor = snapshot.HeaderBackColor;
background.Border = CreateHeaderBorder(snapshot.BorderColor);
Padding = ...;
```

Use a flat/solid DevAge header background mode if `BackgroundColorStyle` otherwise defaults to a gradient. Set the exact enum value representing flat/solid after reading `DevAge.Drawing.BackgroundColorStyle` at the pinned commit; do not rely on OS visual styles.

- [ ] **Step 3: Extend default-view detection in correct order**

Before ordinary-cell default detection:

```text
Views.ColumnHeader.Default -> integration column-header View
Views.RowHeader.Default    -> integration row-header View (Task 4)
Views.Cell.Default         -> integration ordinary-cell View
```

- [ ] **Step 4: Add sort-model regression assertion**

Verify assigning the integration View does not remove the header's `Models.SortableHeader` model or `Controllers.SortableHeader.Default`. The test need not perform a full sort yet; assert the model/controller remain discoverable.

- [ ] **Step 5: Run header tests**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapSourceGridHeaderViewTests
```

---

### Task 4: Implement Bootstrap row-header View

**Files:**
- Create: `src/MyDmsVn.BootstrapSourceGrid/Views/BootstrapSourceGridRowHeaderView.cs`
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridHeaderViewTests.cs`
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Internal/BootstrapSourceGridStyleApplicator.cs`

- [ ] **Step 1: Add failing row-header tests**

Verify default `SourceGrid.Cells.RowHeader` receives the integration View while a custom View remains untouched.

- [ ] **Step 2: Implement specialized row-header View**

Subclass `SourceGrid.Cells.Views.RowHeader` and replace the default OS-themed background with:

```csharp
Background = new DevAge.Drawing.VisualElements.RowHeader();
```

Apply `HeaderBackColor`, `HeaderForeColor`, Bootstrap border, and integration padding through the non-themed visual element's `BackColor`/`Border` APIs.

- [ ] **Step 3: Run tests on both TFMs**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --filter BootstrapSourceGridHeaderViewTests
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapSourceGridHeaderViewTests
```

- [ ] **Step 4: Commit header Views**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid/Views src/MyDmsVn.BootstrapSourceGrid/Internal tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridHeaderViewTests.cs
git commit -m "feat: theme SourceGrid row and column headers"
```

---

### Task 5: Apply selection/focus theme while respecting later consumer overrides

**Files:**
- Create: `src/MyDmsVn.BootstrapSourceGrid/Internal/BootstrapSourceGridSelectionStyle.cs`
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridSelectionStyleTests.cs`
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Controls/BootstrapSourceGrid.cs`

**Interfaces:**
- Consumes `Grid.Selection.BackColor`, `FocusBackColor`, and `Border`.
- Produces Bootstrap selection overlay/focus border without changing ranges or active position.

- [ ] **Step 1: Write failing semantic and ownership tests**

Minimum cases:

```text
ConstructionAppliesBootstrapSelectionColors
ThemeChangeUpdatesIntegrationOwnedSelectionStyle
ConsumerChangedSelectionBackColorIsNotOverwrittenLater
ThemeChangePreservesActivePosition
ThemeChangePreservesSelectedRanges
```

- [ ] **Step 2: Use SourceGrid's overlay model rather than opaque cell recoloring**

Build selection overlay from `snapshot.SelectionBackColor` with alpha comparable to SourceGrid's existing default (`Color.FromArgb(75, ...)`). Use a named constant such as `SelectionOverlayAlpha = 75` so the visual policy is explicit.

Use `snapshot.FocusColor` for selection/focus border. Keep `FocusBackColor` transparent or lightly alpha-blended unless a focused-cell fill is proven visually necessary; do not hide cell text.

- [ ] **Step 3: Track last integration-owned selection values**

On first application, record applied `BackColor`, `FocusBackColor`, and `Border`.

On later theme changes update each property only if its current value still equals the value last applied by the integration. If application code changed a SourceGrid selection property, treat that property as consumer-owned from then on.

This avoids adding a duplicate public `SelectionStyle` abstraction while preserving consumer customization.

- [ ] **Step 4: Never change selection model state during style apply**

`ApplyTheme` must not call:

```text
ResetSelection
Focus
SelectRange
Clear
```

Capture selected ranges/active position in tests before theme switch and assert exact logical state afterward.

- [ ] **Step 5: Run selection tests on both TFMs**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --filter BootstrapSourceGridSelectionStyleTests
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapSourceGridSelectionStyleTests
```

- [ ] **Step 6: Commit selection integration**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid/Internal src/MyDmsVn.BootstrapSourceGrid/Controls tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridSelectionStyleTests.cs
git commit -m "feat: theme SourceGrid selection and focus"
```

---

### Task 6: Wire theme changes into shared Views without full-grid rewrites

**Files:**
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Controls/BootstrapSourceGrid.cs`
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Internal/BootstrapSourceGridStyleApplicator.cs`
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridRuntimeViewThemeTests.cs`

- [ ] **Step 1: Add failing runtime View tests**

Arrange a grid containing:

- ordinary default cell;
- column header;
- row header;
- consumer custom cell View;
- a span;
- active selection.

Resolve default cells once via `GetCell`, capture object identities/state, switch Light -> Dark, and assert:

```text
integration-owned View instances remain shared or are replaced only by integration
consumer View identity unchanged
data unchanged
span unchanged
active position/selection unchanged
newly inserted default cell resolves to current themed View
```

- [ ] **Step 2: Update all integration-owned Views once per theme event**

`ApplyBootstrapTheme()` should:

```text
update _cellView
update _columnHeaderView
update _rowHeaderView
update integration-owned selection style
set safe control-level BackColor/ForeColor
Invalidate()
```

Do not walk every row/column/cell merely to update an already-shared View.

- [ ] **Step 3: Add sparse/large-grid allocation smoke**

Create a representative grid with thousands of positions but only a limited number of materialized cells. A theme change must not allocate a new View per cell. Assert all default ordinary cells resolved after the change share the same integration View instance.

- [ ] **Step 4: Run all Stage 2 tests**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --blame-hang --blame-hang-timeout 5m
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --blame-hang --blame-hang-timeout 5m
```

Expected: PASS.

- [ ] **Step 5: Build full solution and verify vendor cleanliness**

```powershell
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Expected: build exits `0`; vendor status produces no output.

- [ ] **Step 6: Commit runtime View integration tests**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridRuntimeViewThemeTests.cs
git commit -m "test: harden Bootstrap SourceGrid visual integration"
```

## Stage completion criteria

- [ ] Ordinary SourceGrid default cells receive Bootstrap visuals lazily through `GetCell`.
- [ ] Alternating rows are based on current row position and remain correct after row movement/reindexing.
- [ ] Column headers retain sort-indicator/model/controller behavior.
- [ ] Row headers use programmable Bootstrap colors rather than OS themed background.
- [ ] Consumer custom Views are never replaced by theme refresh.
- [ ] Selection/focus use Bootstrap semantics without changing selection state.
- [ ] Theme changes update shared integration Views without full-grid data rewrites.
- [ ] Spans remain unchanged.
- [ ] Both TFMs pass.
- [ ] No SourceGrid vendor patch was required.
