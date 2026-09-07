# MyDmsVn.BootstrapSourceGrid

Bootstrap-inspired SourceGrid control for native Windows Forms applications.

`BootstrapSourceGrid` combines the SourceGrid 5.0 grid engine with the theme, typography, rendering, DPI, accessibility, and design conventions of `MyDmsVn.Bootstrap5WinFormUI` while preserving SourceGrid's public programming model.

## Status

This repository is in the architecture and implementation-planning phase. The initial implementation must follow the decisions in `docs/DECISIONS.md` and the staged plans under `docs/plans/`.

## Core design

```text
                  MyDmsVn.BootstrapSourceGrid
                            |
                    BootstrapSourceGrid
                            |
                      SourceGrid.Grid

          uses                                  uses
           |                                     |
           v                                     v
MyDmsVn.Bootstrap5WinFormUI              SourceGrid 5.0
Theme / Rendering / DPI                  Grid engine / Cells
Typography / semantic tokens             Views / Editors / Selection
```

The key public type is intended to be:

```csharp
namespace MyDmsVn.Bootstrap5WinFormUI.Controls;

public class BootstrapSourceGrid : SourceGrid.Grid
{
}
```

The integration assembly/package remains separate from the Bootstrap framework:

- Assembly: `MyDmsVn.BootstrapSourceGrid.dll`
- Package: `MyDmsVn.BootstrapSourceGrid`
- Base namespace for the public control: `MyDmsVn.Bootstrap5WinFormUI.Controls`
- Target frameworks: `net48;net8.0-windows`

## Pinned architectural baselines

The first implementation is designed against these exact upstream snapshots:

- Bootstrap framework: `chung6a8m/MyDmsVn.Bootstrap5WinFormUI@95077df0c8bad8593143c2190606d2f444bfc653`
- SourceGrid: `chung6a8m/sourcegrid@f4e457b43582bf01892f50bdc74aa480531e5944`

See `docs/UPSTREAM.md` for upgrade rules and `docs/UPSTREAM_API_SEAMS.md` for the exact vendor APIs/extension seams already verified against those commits.

## Non-negotiable boundaries

1. `BootstrapSourceGrid` inherits `SourceGrid.Grid`; do not introduce a wrapper unless a documented blocker proves inheritance cannot meet a requirement.
2. Dependency flow is one-way: this project depends on Bootstrap5WinFormUI and SourceGrid. Neither vendor may depend on this project or on each other because of this integration.
3. Preserve SourceGrid's public grid API. Do not create Bootstrap-prefixed wrappers for rows, columns, cells, selection, controllers, editors, or ranges merely for naming consistency.
4. Bootstrap styling is applied through theme adapters and SourceGrid Views/VisualModels where practical. Do not fork the SourceGrid painting engine just to recolor it.
5. Keep SourceGrid patches at zero or near zero. Any required upstream patch must be isolated, justified, regression-tested, and proposed in the SourceGrid fork first.
6. Both `net48` and `net8.0-windows` are first-class targets.
7. Initial scope covers grid/cell/header/selection/theme integration. Native SourceGrid scrollbars remain unchanged initially; replacing the full editor subsystem is out of initial scope.

## Documentation

Start here:

- `AGENTS.md` — mandatory operating rules for coding agents.
- `AI_CONTEXT.md` — compact project model for AI assistants.
- `CONTRIBUTING.md` — contributor workflow and validation discipline.
- `docs/README.md` — documentation map and source-of-truth order.
- `docs/PRD.md` — product requirements and definition of MVP.
- `docs/ARCHITECTURE.md` — component boundaries and integration design.
- `docs/DECISIONS.md` — approved architectural decisions.
- `docs/UPSTREAM_API_SEAMS.md` — vendor integration seams verified against pinned commits.
- `docs/UPSTREAM.md` — pinned vendor baselines and upgrade policy.
- `docs/COMPATIBILITY.md` — target-framework and API compatibility rules.
- `docs/TESTING.md` — automated/manual WinForms verification strategy.
- `docs/DEVELOPMENT_PLAN.md` — stage roadmap and release gates.
- `docs/PENDING_DECISIONS.md` — numbered owner decisions that agents may not silently make.
- `docs/plans/` — task-level implementation plans.

## Implementation plans

The initial plan set is intentionally staged:

```text
20260907-001  Master roadmap
20260907-002  Foundation and vendor pinning
20260907-003  Control shell and theme adapter
20260907-004  Cell/header/selection theming
20260907-005  Runtime theme/DPI/Designer hardening
20260907-006  Editor and interaction hardening
20260907-007  Demo, packaging, and release
```

Each stage has its own acceptance criteria, dual-target test gate, and vendor-cleanliness check.

## Development principles

- TDD for logic and observable behavior.
- Preserve SourceGrid behavior unless the requirement explicitly changes it.
- Reuse Bootstrap5WinFormUI theme tokens, typography, `DpiScaler`, and runtime theme notifications.
- Keep WinForms Designer construction safe without application bootstrap/global initialization.
- Dispose owned GDI resources and unsubscribe events.
- Never let unattended GUI tests wait on modal dialogs or default WinForms exception UI.
- Validate both TFMs before completing a stage.

## Initial success criterion

Existing SourceGrid usage should require only a type substitution for the common path:

```csharp
var grid = new BootstrapSourceGrid();
grid.Redim(10, 4);
grid[0, 0] = new SourceGrid.Cells.Cell("Northwind");
grid.Selection.EnableMultiSelection = true;
```

The surrounding SourceGrid programming model should remain intact while the grid adopts Bootstrap5WinFormUI visual semantics and runtime theme behavior.
