# Development plan

`docs/plans/` contains active implementation plans only.

The initial BootstrapSourceGrid MVP roadmap is complete and archived. For historical context, see [Archive](./archive/).

## Active initiative — Bootstrap editor replacement

Goal: add Bootstrap-native SourceGrid editor adapters without replacing SourceGrid's editor lifecycle.

Canonical design: [`EDITOR_REPLACEMENT.md`](./EDITOR_REPLACEMENT.md)

Master roadmap: [`plans/20260909-001-bootstrap-editor-replacement-master-roadmap.md`](./plans/20260909-001-bootstrap-editor-replacement-master-roadmap.md)

Stages:

1. [`20260909-002-editor-replacement-architecture-and-ownership.md`](./plans/20260909-002-editor-replacement-architecture-and-ownership.md) — verify lifecycle, sharing, sizing, disposal, and ownership.
2. [`20260909-003-bootstrap-text-box-editor.md`](./plans/20260909-003-bootstrap-text-box-editor.md) — reference `BootstrapTextBox` adapter.
3. [`20260909-004-bootstrap-formatted-text-box-editor.md`](./plans/20260909-004-bootstrap-formatted-text-box-editor.md) — `RawValue` adapter and formatting/conversion contract.
4. [`20260909-005-bootstrap-lookup-box-editor.md`](./plans/20260909-005-bootstrap-lookup-box-editor.md) — `SelectedValue`, popup/focus, and keyboard interaction contract.
5. [`20260909-006-editor-registry-hardening-and-expansion.md`](./plans/20260909-006-editor-registry-hardening-and-expansion.md) — public grid-owned registry, hardening, demo, docs, and future editor pattern.

## Cross-stage invariants

Every stage must preserve:

- `BootstrapSourceGrid : SourceGrid.Grid` and the existing SourceGrid public programming model;
- SourceGrid ownership of start/commit/cancel, final validation/conversion, placement, and grid navigation;
- Bootstrap control ownership of editor theme/font/background/border/focus visuals;
- `UseCellViewProperties = false` for Bootstrap-native adapters;
- shared grid-scoped editor lifetime rather than per-cell composite controls;
- no cross-grid adapter sharing;
- consumer custom editors unchanged;
- no SourceGrid global editor-factory patch;
- `net48;net8.0-windows` support;
- bounded non-modal STA GUI tests;
- clean vendor worktrees.

## Stage gate

Before moving to the next stage:

```powershell
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Expected: all commands exit `0` and both vendor status commands produce no output.

Do not start a later stage while the current stage's acceptance gate is failing.
