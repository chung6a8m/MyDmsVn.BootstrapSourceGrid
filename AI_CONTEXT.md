# AI_CONTEXT.md

Compact, stable project context for AI assistants working on `MyDmsVn.BootstrapSourceGrid`.

For mandatory operating rules read `AGENTS.md`. For detailed requirements start at `docs/README.md`.

## Identity

- Repository: `chung6a8m/MyDmsVn.BootstrapSourceGrid`
- Product: Bootstrap-themed SourceGrid control for native WinForms
- Product assembly/package: `MyDmsVn.BootstrapSourceGrid`
- Primary public type: `MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid`
- Direct base type: `SourceGrid.Grid`
- Required TFMs: `net48;net8.0-windows`
- Repository/package license: MIT

## Product intent

Create an integration control that retains SourceGrid's mature grid model, cells, views, editors, selection, keyboard behavior, spans, and scrolling while adopting the visual language and runtime theme infrastructure of `MyDmsVn.Bootstrap5WinFormUI`.

This is not:

- a new grid engine;
- a wrapper that re-exposes all SourceGrid APIs;
- a fork that injects Bootstrap dependencies into SourceGrid;
- a CSS/HTML/WebView control;
- a replacement for the SourceGrid editor system in the initial release.

## Approved architectural baseline

```text
MyDmsVn.BootstrapSourceGrid
        |
        +----> MyDmsVn.Bootstrap5WinFormUI
        |        Theme / Metrics / Typography / DpiScaler
        |
        `----> SourceGrid
                 Grid / Cells / Views / Editors / Selection
```

Primary inheritance:

```text
System.Windows.Forms.Panel
    -> SourceGrid.CustomScrollControl
    -> SourceGrid.GridVirtual
    -> SourceGrid.Grid
    -> BootstrapSourceGrid
```

## Vendor baselines

Initial implementation is designed and validated against:

- Bootstrap5WinFormUI commit `95077df0c8bad8593143c2190606d2f444bfc653`
- SourceGrid commit `f4e457b43582bf01892f50bdc74aa480531e5944`

Bootstrap5WinFormUI provides `BootstrapThemeManager`, `BootstrapTheme`, colors, metrics, typography, `DpiScaler`, and existing themed native controls such as `BootstrapDataGridView` that demonstrate lifecycle/font/theme patterns.

SourceGrid 5.0 supports `net48` and `net8.0-windows` and treats its large historical public API as a compatibility constraint. Its cell model separates Model, View, Editor, and Controller concerns; Views are the preferred styling integration point.

Exact upstream seams verified against these commits are recorded in `docs/UPSTREAM_API_SEAMS.md`. Do not substitute remembered SourceGrid/Bootstrap APIs for that verified record.

## Approved two-phase dependency strategy

During development/pre-release, vendor source is consumed as pinned Git submodules and referenced with `ProjectReference` (temporary strategy 2B):

```text
vendor/
  Bootstrap5WinFormUI/  @ 95077df...
  sourcegrid/           @ f4e457b...
```

This provides commit-level reproducibility without copying or rewriting vendor source.

For **public NuGet release**, strategy 2A is mandatory: `MyDmsVn.BootstrapSourceGrid` must depend on resolvable exact-version vendor NuGet packages that have been verified as equivalent to the tested source baselines, or to explicitly approved upgraded baselines.

Public publication is blocked until both vendor packages have verified PackageId/version/feed, both required TFMs, source/commit correspondence, API/behavior equivalence, acceptable transitive dependencies, and license/notice obligations. Do not silently embed vendor DLLs/source into the integration package to bypass this requirement.

## Licensing

The integration repository/package uses MIT (`docs/DECISIONS.md` D-016; root `LICENSE`). Future package metadata must use `PackageLicenseExpression=MIT`.

This decision applies to the integration's own source only. Vendor license/notice obligations must still be verified before public package release.

## Ownership boundaries

### SourceGrid owns

- concrete grid storage and virtual-grid foundation;
- rows, columns, positions, ranges, spans;
- selection and active position;
- controllers and input dispatch;
- editors and edit lifecycle;
- scrolling;
- core grid painting and cell View contracts.

### BootstrapSourceGrid owns

- mapping Bootstrap theme tokens into SourceGrid-compatible visual state;
- default Bootstrap cell/header/selection Views or styles;
- runtime theme subscription and repaint/update lifecycle;
- Bootstrap typography defaults while respecting consumer font overrides;
- Bootstrap-owned DPI-scaled metrics;
- integration-specific accessibility/designer behavior;
- integration tests and demo.

## Verified SourceGrid styling seam

A critical implementation fact at the pinned SourceGrid commit:

```csharp
grid[row, column] = cell;
```

reaches a private SourceGrid `InsertCell(...)`. A subclass cannot reliably intercept all normal assignment through `SetCell(...)`.

The approved MVP no-patch strategy is therefore:

```text
override virtual GetCell(row, column)
    -> call base.GetCell
    -> if View is exactly a known SourceGrid default singleton, replace it with an integration-owned shared View
    -> if View is already integration-owned, keep it
    -> otherwise treat it as consumer-owned and leave it untouched
    -> return the same SourceGrid cell
```

Known default identities include:

```text
SourceGrid.Cells.Views.Cell.Default
SourceGrid.Cells.Views.Header.Default
SourceGrid.Cells.Views.ColumnHeader.Default
SourceGrid.Cells.Views.RowHeader.Default
```

Do not hide/redeclare the SourceGrid indexer merely to intercept assignment, and do not patch SourceGrid unless this verified seam stops satisfying a concrete requirement.

## Styling strategy

Prefer this flow:

```text
BootstrapThemeManager.CurrentTheme
       |
       v
BootstrapSourceGridThemeAdapter
       |
       +--> shared cell View/style
       +--> shared generic header View/style
       +--> shared column header View/style
       +--> shared row header View/style
       +--> selection/focus visual state
       `--> active-editor appearance refresh where safe
```

Ordinary alternating-row styling can use `CellContext.Position.Row` inside a shared View rather than allocating one View per row.

Column/row header integration should preserve the SourceGrid header View type behavior while substituting programmable non-OS-themed DevAge background visual elements where required for Bootstrap colors.

Do not perform broad `OnPaint` replacement when SourceGrid Views/VisualModels can express the requirement.

## Selection ownership

SourceGrid selection rendering already consumes:

```text
Selection.BackColor
Selection.FocusBackColor
Selection.Border
```

Theme updates may change these visual properties but must never reset selection, active position, or ranges simply to repaint.

If application code changes a selection visual property after BootstrapSourceGrid applied it, treat that property as consumer-owned and stop overwriting it on later theme changes.

## Editor integration

SourceGrid `EditorBase.UseCellViewProperties` defaults to `true`, and `EditorControlBase.OnStartingEdit` already copies the cell View's foreground/background/font into the editor control.

Therefore MVP does not replace SourceGrid editors. The integration only needs a narrow active-editor refresh when a runtime theme change occurs during editing. If `UseCellViewProperties == false`, respect that SourceGrid-native opt-out and do not restyle the editor.

## Runtime theme lifecycle

Follow Bootstrap5WinFormUI conventions:

1. Construct safely with default/current theme even before handle creation.
2. Subscribe to `BootstrapThemeManager.ThemeChanged` while alive.
3. Re-apply Bootstrap-owned shared visual objects on theme change.
4. Preserve grid data, spans, selection, SourceGrid behavioral state, and consumer Views.
5. Refresh an active editor only when SourceGrid's View-property ownership says it is safe.
6. Repaint/invalidate without rebuilding grid data.
7. Unsubscribe and dispose owned GDI resources in `Dispose(bool)`.

## Font ownership

Default behavior should use the Bootstrap body typography token. If the integration creates a `Font`, it owns and disposes that `Font`. If application code assigns `Font`, switch to consumer-font mode and never dispose the consumer's instance.

Keep integration cell/header View `Font` unset/null where SourceGrid can inherit `grid.Font`, so this single control-level ownership contract remains authoritative.

## DPI ownership

Initial integration-owned metric mapping:

```text
Cell padding      <- CurrentTheme.Metrics.SpacingXS
Cell border width <- CurrentTheme.Metrics.BorderWidth
Focus border      <- CurrentTheme.Metrics.FocusBorderWidth
```

Scale those through `DpiScaler`. Do not rescale SourceGrid/application row heights, column widths, scrollbar dimensions, or other dimensions that the integration does not own.

## Initial scope

Initial release includes:

- `BootstrapSourceGrid : SourceGrid.Grid`;
- Bootstrap surface/text/border styling;
- normal/alternate cells;
- column and row headers;
- selection/focus/read-only/disabled visual treatment where SourceGrid exposes safe hooks;
- runtime light/dark/custom theme updates;
- DPI-aware Bootstrap-owned metrics;
- designer-safe construction;
- default editor appearance hardening without replacing editor architecture;
- demo, tests, packaging/documentation.

Initial release excludes:

- custom replacement scrollbars;
- a Bootstrap clone of SourceGrid rows/columns/cells APIs;
- a new data-binding/virtualization layer;
- wholesale replacement of SourceGrid editors;
- modifications that make SourceGrid depend on Bootstrap5WinFormUI.

## Testing model

Tests must cover both `net48` and `net8.0-windows`.

Pure mapping/style logic should be tested without handles. Control and editor behavior that requires WinForms should use STA tests with bounded hang detection and deterministic exception handling. No test may leave modal UI waiting for input.

Manual/demo gates cover designer loading, runtime light/dark switching, DPI changes, selection/focus, keyboard editing, scrolling, spans, and representative SourceGrid samples.

## Owner-decision status

The initial owner-only release choices are resolved:

- 1A: MIT license -> D-016.
- 2A with 2B temporary -> D-017.

`docs/PENDING_DECISIONS.md` is now an audit trail/future decision register. No unresolved owner decision currently blocks ordinary MVP implementation. Public release can still be blocked by failed verification of the already-approved license/dependency requirements.

## Source-of-truth order

When information conflicts, use this precedence:

1. Explicit current user instruction
2. `docs/DECISIONS.md`
3. `docs/PRD.md`
4. `docs/ARCHITECTURE.md`
5. `docs/UPSTREAM_API_SEAMS.md` for exact pinned-vendor API facts
6. `docs/UPSTREAM.md`
7. `docs/COMPATIBILITY.md`
8. `docs/TESTING.md`
9. `docs/DEVELOPMENT_PLAN.md`
10. Active file under `docs/plans/`
11. Vendor source/tests at the pinned commits
12. Historical discussion/feasibility notes

Read `AGENTS.md` before implementation work.
