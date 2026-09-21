# Development plan

`docs/plans/` contains active implementation plans only.

## Active initiative — Bootstrap baseline upgrade and demo typography profiles

Goal:

1. advance `vendor/Bootstrap5WinFormUI` from the current pinned baseline to the latest approved vendor `main` HEAD;
2. prove compatibility across both TFMs and the existing grid/theme/editor contracts;
3. add SourceGrid Demo typography profiles equivalent to Bootstrap5WinFormUI PR #63: `Default`, `Base 14px`, and `Base 16px`, without changing framework defaults.

Canonical scoped spec:

[`BOOTSTRAP_BASELINE_UPGRADE_AND_DEMO_TYPOGRAPHY.md`](./BOOTSTRAP_BASELINE_UPGRADE_AND_DEMO_TYPOGRAPHY.md)

Master roadmap:

[`plans/20260921-001-bootstrap-baseline-upgrade-and-demo-typography-master-roadmap.md`](./plans/20260921-001-bootstrap-baseline-upgrade-and-demo-typography-master-roadmap.md)

Planning snapshot:

- previous Bootstrap pin: `cceba3c969e28726935793a1c6ca3772bed60a35`;
- Bootstrap `main` at roadmap creation: `aba102e33c48937fd92468791c287afda0a59e77`;
- current SourceGrid pin remains `f4e457b43582bf01892f50bdc74aa480531e5944`.

Stage 0 re-resolved `origin/main` to `aba102e33c48937fd92468791c287afda0a59e77` and accepted that pin on 2026-09-21. The verification record is [`verification/20260921-bootstrap-baseline-upgrade.md`](./verification/20260921-bootstrap-baseline-upgrade.md).

Stages:

1. Bootstrap baseline upgrade and compatibility proof.
2. SourceGrid Demo typography profiles.
3. Full regression/manual validation and canonical documentation sync.

Cross-stage rules:

- keep `BootstrapSourceGrid : SourceGrid.Grid`;
- keep `net48;net8.0-windows`;
- do not modify vendor source from this repository;
- preserve SourceGrid public behavior and consumer customizations;
- preserve grid-owned shared Bootstrap editor adapters;
- keep profile implementation demo-only;
- keep framework `BootstrapThemeTypography.Default` unchanged;
- keep GUI tests bounded/non-modal;
- do not proceed through a failing stage gate.

## Completed initiative — Bootstrap editor replacement

Goal: add Bootstrap-native SourceGrid editor adapters without replacing SourceGrid's editor lifecycle.

Canonical design: [`EDITOR_REPLACEMENT.md`](./EDITOR_REPLACEMENT.md)

Archived roadmap: [`archive/20260909-bootstrap-editor-replacement/`](./archive/20260909-bootstrap-editor-replacement/)

Delivered:

1. architecture and ownership model;
2. `BootstrapTextBox` editor adapter;
3. `BootstrapFormattedTextBox` RawValue adapter;
4. `BootstrapLookupBox` SelectedValue/popup interaction adapter;
5. grid-owned editor registry, hardening, demo, docs, and expansion pattern.

## Stable cross-project invariants

Every active stage must preserve:

- SourceGrid ownership of grid behavior, edit lifecycle, final validation/conversion, selection/navigation, and scrolling;
- Bootstrap ownership of theme/visual semantics;
- consumer View/font precedence;
- `net48;net8.0-windows`;
- bounded non-modal STA GUI tests;
- clean vendor worktrees.

## Standard stage gate

```powershell
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Expected: build/tests exit 0 and both vendor status commands produce no worktree output.
