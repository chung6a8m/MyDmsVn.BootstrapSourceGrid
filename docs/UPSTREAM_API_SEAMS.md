# Verified Upstream API Seams

This document records integration seams verified directly against the two pinned vendor commits. It exists to prevent future implementation work from relying on remembered assumptions or accidentally inventing vendor APIs.

The canonical architectural intent remains in `DECISIONS.md` and `ARCHITECTURE.md`. When a vendor is upgraded, re-verify this document as part of the upgrade procedure in `UPSTREAM.md`.

## 1. Verified baselines

- Bootstrap framework: `chung6a8m/MyDmsVn.Bootstrap5WinFormUI@95077df0c8bad8593143c2190606d2f444bfc653`
- SourceGrid: `chung6a8m/sourcegrid@f4e457b43582bf01892f50bdc74aa480531e5944`

## 2. Bootstrap5WinFormUI seams

### 2.1 Theme manager

Verified public APIs:

```csharp
BootstrapThemeManager.CurrentTheme
BootstrapThemeManager.ThemeChanged
BootstrapTheme.CreateDefault(BootstrapThemeMode.Light)
BootstrapTheme.CreateDefault(BootstrapThemeMode.Dark)
```

`CurrentTheme` provides a usable default theme, so `BootstrapSourceGrid` construction does not need application-level theme initialization.

### 2.2 Semantic theme state

Verified theme groups:

```text
BootstrapTheme.Colors
BootstrapTheme.Metrics
BootstrapTheme.Typography
```

Verified color tokens used by the integration plans:

```text
Primary
Secondary
Light
Dark
Body
Surface
SurfaceSecondary
Border
Text
MutedText
Disabled
Focus
Hover
Active
```

Use semantic tokens instead of hard-coded Bootstrap hex colors.

### 2.3 Selection contrast helper

Verified helper:

```csharp
ColorUtil.GetContrastingTextColor(...)
```

Selection foreground calculations should delegate to the framework helper instead of introducing a second luminance/contrast implementation.

### 2.4 DPI helpers and tokens

Verified defaults at 96 logical DPI:

```text
Metrics.SpacingXS       = 4
Metrics.BorderWidth     = 1
Metrics.FocusBorderWidth = 2
```

Verified DPI helper:

```csharp
DpiScaler.DefaultDpi // 96
DpiScaler.Scale(int logicalPixels, int dpi)
```

Integer scaling uses midpoint rounding away from zero.

Initial integration mapping:

```text
BootstrapSourceGrid cell padding      <- SpacingXS
BootstrapSourceGrid cell border width <- BorderWidth
BootstrapSourceGrid focus border      <- FocusBorderWidth
```

These are integration-owned metrics only. SourceGrid-owned row heights, column widths, scrollbars, and application dimensions must not be scaled again by this integration.

### 2.5 Theme-owned Font pattern

The pinned framework's `BootstrapDataGridView` provides the reference lifecycle:

```text
construct in theme-font mode
create a Font from Typography.Body
own/dispose only that created Font
on external Font assignment -> consumer-font mode
runtime theme change replaces only the integration-owned Font
Dispose unsubscribes ThemeChanged and releases the owned Font
```

`BootstrapSourceGrid` should reproduce this ownership pattern, not inherit from `BootstrapDataGridView`.

## 3. SourceGrid control/inheritance seams

Verified SourceGrid concrete control:

```csharp
public partial class Grid : GridVirtual
```

Relevant inheritance conceptually remains:

```text
System.Windows.Forms.Panel
  -> SourceGrid.CustomScrollControl
  -> SourceGrid.GridVirtual
  -> SourceGrid.Grid
  -> BootstrapSourceGrid
```

`BootstrapSourceGrid` therefore receives SourceGrid's concrete rows/columns/cells/selection/editor/scrolling behavior directly.

## 4. Important insertion/interception constraint

A critical verified constraint is that common assignment:

```csharp
grid[row, column] = cell;
```

ultimately calls SourceGrid's private `InsertCell(...)` implementation. The subclass cannot intercept that indexer assignment without changing SourceGrid.

`Grid.SetCell(int, int, ICellVirtual)` is virtual, but relying only on it would not catch normal indexer assignment.

### Approved no-patch integration seam

Use the virtual read path:

```csharp
public override SourceGrid.Cells.ICellVirtual GetCell(int row, int column)
```

The integration can:

1. call `base.GetCell(row, column)`;
2. inspect the returned cell's current View;
3. replace only known SourceGrid default View singleton identities with Bootstrap integration-owned shared Views;
4. leave every unknown/custom consumer View unchanged;
5. return the same SourceGrid cell object.

This keeps normal SourceGrid assignment intact and avoids a vendor patch.

Do **not** hide/redeclare SourceGrid's indexer merely to intercept assignment.

## 5. SourceGrid View seams

### 5.1 Ordinary cells

Verified default:

```csharp
SourceGrid.Cells.Views.Cell.Default
```

`CellVirtual.View` is public and SourceGrid explicitly documents Views as shareable across many cells.

`SourceGrid.Cells.Views.ViewBase` exposes the required style properties:

```text
BackColor
ForeColor
Border
Padding
Font
```

The integration should keep the View's `Font` null where appropriate so SourceGrid falls back to `grid.Font`. That lets the BootstrapSourceGrid control-level theme/consumer Font ownership contract work consistently.

### 5.2 Dynamic alternating rows

`CellContext.Position` is available while the View prepares for drawing. Therefore one shared Bootstrap ordinary-cell View can select:

```text
even row -> Surface
odd row  -> SurfaceSecondary
```

at draw/measure preparation time instead of creating one View per row or walking all cells after each theme change.

### 5.3 Default-View ownership rule

Use **reference identity**, not only runtime type, to decide whether the integration owns a default View:

```text
Views.Cell.Default
Views.ColumnHeader.Default
Views.RowHeader.Default
```

If a consumer has assigned any different View instance, treat it as consumer-owned and preserve it across theme changes.

## 6. Header seams

### 6.1 Column headers

Verified SourceGrid column-header View:

```csharp
SourceGrid.Cells.Views.ColumnHeader
```

It owns sort-indicator behavior and defaults its background to an OS-themed DevAge visual element.

Verified programmable non-themed DevAge visual element:

```csharp
DevAge.Drawing.VisualElements.ColumnHeader
```

It exposes:

```text
BackColor
Border
BackgroundColorStyle
```

and implements the header background contract required by the SourceGrid View.

Recommended integration:

```text
subclass SourceGrid.Cells.Views.ColumnHeader
replace only Background with DevAge.Drawing.VisualElements.ColumnHeader
apply Bootstrap colors/border/padding
retain SourceGrid ElementSort/model/controller behavior
```

### 6.2 Row headers

Verified SourceGrid row-header View:

```csharp
SourceGrid.Cells.Views.RowHeader
```

Verified programmable DevAge background:

```csharp
DevAge.Drawing.VisualElements.RowHeader
```

It likewise exposes programmable background/border state.

## 7. Selection/focus seams

Verified selection decorator consumes:

```text
Grid.Selection.BackColor
Grid.Selection.FocusBackColor
Grid.Selection.Border
```

`GridVirtual.Selection` is publicly typed as `IGridSelection`, while these three
visual properties are declared by `SourceGrid.Selection.SelectionBase`. The
protected SourceGrid selection factory returns `SelectionBase` implementations,
so integration code must access the visual properties through that concrete base
contract rather than assuming they are members of `IGridSelection`.

Changing `Grid.SelectionMode` recreates the SourceGrid selection object. Visual
ownership tracking must therefore reset for the new selection instance so its
fresh defaults are not mistaken for consumer overrides.

SourceGrid's default selection background uses a translucent highlight with alpha `75`.

The integration can theme these properties without replacing the selection engine.

### Ownership rule

For each selection visual property:

1. store the value last applied by BootstrapSourceGrid;
2. on a later theme change, update it only if the current value still equals the last integration-applied value;
3. if application code changed the value, treat that property as consumer-owned from then on.

Theme application must never call selection-state-changing methods simply to recolor selection.

## 8. Editor seams

Verified SourceGrid editor contract:

```csharp
EditorBase.UseCellViewProperties // public, default true
EditorControlBase.Control       // actual WinForms editor Control
CellContext.StartEdit()
CellContext.EndEdit(bool cancel)
```

### Built-in appearance propagation

`EditorControlBase.OnStartingEdit(...)` already copies from the cell View when `UseCellViewProperties == true`:

```text
BackColor
ForeColor
Font
```

The default SourceGrid text editor uses a `DevAgeTextBox` with `BorderStyle.None` and remains SourceGrid-owned.

### Integration consequence

MVP does not need Bootstrap-specific replacement editor classes.

The only extra bridge planned is:

```text
runtime theme changes while an editor is already active
```

For that case, update the active `EditorControlBase.Control` from the current cell View only when `UseCellViewProperties == true`.

If a consumer sets `UseCellViewProperties = false`, the integration must not restyle that editor control.

## 9. Scrollbar seam

SourceGrid's `CustomScrollControl` owns its horizontal/vertical native scrollbar instances and scrolling layout.

MVP leaves this subsystem unchanged. Replacing scrollbars would expand the integration into wheel input, thumb tracking, PageUp/PageDown, focus, Win32 message handling, accessibility, layout, and DPI behavior.

## 10. Upgrade re-verification checklist

Whenever either vendor baseline changes, verify all of the following before accepting the update:

```text
[ ] BootstrapThemeManager CurrentTheme/ThemeChanged contracts
[ ] Bootstrap theme token names/semantics
[ ] DpiScaler behavior
[ ] framework theme-font ownership reference pattern
[ ] SourceGrid Grid/GridVirtual inheritance
[ ] Grid.GetCell remains virtual
[ ] indexer/InsertCell behavior and whether interception assumptions changed
[ ] default Cell/ColumnHeader/RowHeader View identities
[ ] ViewBase style properties/shareability
[ ] header visual-element contracts
[ ] Selection visual properties/decorator behavior
[ ] Grid.Selection interface type and SelectionBase visual-property contract
[ ] EditorBase.UseCellViewProperties behavior
[ ] EditorControlBase appearance propagation
[ ] CustomScrollControl ownership
```

If a seam changes, update this document, affected plans/tests, and `UPSTREAM.md` in the same vendor-upgrade change.
