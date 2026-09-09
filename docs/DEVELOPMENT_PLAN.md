# Development plan

`docs/plans/` contains active implementation plans only.

The initial BootstrapSourceGrid MVP roadmap is complete and archived. For historical context, see [Archive](./archive/).

## Completed initiative — Bootstrap editor replacement

Goal: add Bootstrap-native SourceGrid editor adapters without replacing SourceGrid's editor lifecycle.

Canonical design: [`EDITOR_REPLACEMENT.md`](./EDITOR_REPLACEMENT.md)

Archived roadmap: [`archive/20260909-bootstrap-editor-replacement/`](./archive/20260909-bootstrap-editor-replacement/)

Stages:

1. Architecture and ownership — lifecycle, sharing, sizing, disposal, and grid ownership verified.
2. `BootstrapTextBox` — reference adapter implemented.
3. `BootstrapFormattedTextBox` — `RawValue` adaptation and SourceGrid conversion contract implemented.
4. `BootstrapLookupBox` — `SelectedValue`, popup/focus, and keyboard interaction contract implemented.
5. Registry and hardening — public grid-owned registry, allocation/disposal proofs, demo, docs, and future editor pattern implemented.

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
