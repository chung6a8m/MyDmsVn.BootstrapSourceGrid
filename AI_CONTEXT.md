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

## Initial dependency strategy

During repository bootstrap, vendor source is consumed as pinned Git submodules and referenced with `ProjectReference`:

```text
vendor/
  Bootstrap5WinFormUI/  @ 95077df...
  sourcegrid/           @ f4e457b...
```

This provides commit-level reproducibility without copying or rewriting vendor source. A later migration to published NuGet versions is allowed only after equivalent API/behavior is verified and documented.

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

## Styling strategy

Prefer this flow:

```text
BootstrapThemeManager.CurrentTheme
       |
       v
BootstrapSourceGridThemeAdapter
       |
       +--> cell View/style
       +--> column header View/style
       +--> row header View/style
       +--> selection/focus visual state
       `--> editor appearance bridge where safe
```

Do not perform broad `OnPaint` replacement when SourceGrid Views/VisualModels can express the requirement.

## Runtime theme lifecycle

Follow Bootstrap5WinFormUI conventions:

1. Construct safely with default/current theme even before handle creation.
2. Subscribe to `BootstrapThemeManager.ThemeChanged` while alive.
3. Re-apply Bootstrap-owned visual objects on theme change.
4. Preserve grid data, selection, and SourceGrid behavioral state.
5. Repaint/invalidate.
6. Unsubscribe and dispose owned GDI resources in `Dispose(bool)`.

## Font ownership

Default behavior should use the Bootstrap body typography token. If the integration creates a `Font`, it owns and disposes that `Font`. If application code assigns `Font`, switch to consumer-font mode and never dispose the consumer's instance.

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

## Source-of-truth order

When information conflicts, use this precedence:

1. Explicit current user instruction
2. `docs/DECISIONS.md`
3. `docs/PRD.md`
4. `docs/ARCHITECTURE.md`
5. `docs/UPSTREAM.md`
6. `docs/COMPATIBILITY.md`
7. `docs/TESTING.md`
8. `docs/DEVELOPMENT_PLAN.md`
9. Active file under `docs/plans/`
10. Vendor source/tests at the pinned commits
11. Historical discussion/feasibility notes

Read `AGENTS.md` before implementation work.
