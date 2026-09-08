# BootstrapSourceGrid Master Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Deliver a consumer-ready `BootstrapSourceGrid : SourceGrid.Grid` integration that preserves SourceGrid behavior while adopting Bootstrap5WinFormUI theme, typography, DPI, and visual conventions on both supported TFMs.

**Architecture:** Keep SourceGrid as the grid engine and Bootstrap5WinFormUI as the design-system owner. The integration assembly depends on both vendors, maps Bootstrap theme tokens into SourceGrid Views/styles, subscribes to runtime theme changes, and does not introduce a second grid/editor/scrollbar engine.

**Tech Stack:** C#, Windows Forms, SDK-style .NET projects, `net48`, `net8.0-windows`, SourceGrid 5.0, MyDmsVn.Bootstrap5WinFormUI, NUnit, Git submodules.

**Spec:** `docs/PRD.md`; architecture in `docs/ARCHITECTURE.md`; fixed decisions in `docs/DECISIONS.md`.

## Global Constraints

- Public control: `MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid`.
- Base type: `SourceGrid.Grid`.
- TFMs: `net48;net8.0-windows`.
- Bootstrap baseline: `95077df0c8bad8593143c2190606d2f444bfc653`.
- SourceGrid baseline: `f4e457b43582bf01892f50bdc74aa480531e5944`.
- Vendor dependencies remain one-way from this integration.
- Preserve SourceGrid public APIs and behavioral semantics.
- Prefer SourceGrid Views/VisualModels to broad painting overrides.
- Initial release does not replace SourceGrid scrollbars or the full editor system.
- Consumer-assigned `Font` and SourceGrid Views override integration defaults.
- GUI tests must be STA, bounded, and non-interactive.

---

## File Structure Target

```text
MyDmsVn.BootstrapSourceGrid.sln
src/MyDmsVn.BootstrapSourceGrid/
  MyDmsVn.BootstrapSourceGrid.csproj
  Controls/BootstrapSourceGrid.cs
  Theming/BootstrapSourceGridThemeAdapter.cs
  Theming/BootstrapSourceGridThemeSnapshot.cs
  Theming/BootstrapSourceGridThemeFont.cs
  Views/BootstrapSourceGridCellView.cs
  Views/BootstrapSourceGridColumnHeaderView.cs
  Views/BootstrapSourceGridRowHeaderView.cs
  Editors/BootstrapSourceGridEditorStyler.cs
  Internal/BootstrapSourceGridStyleApplicator.cs
  Internal/BootstrapSourceGridDpiMetrics.cs

tests/MyDmsVn.BootstrapSourceGrid.Tests/
  MyDmsVn.BootstrapSourceGrid.Tests.csproj
  TestBootstrapThemes.cs
  WinFormsTestGuard.cs
  BootstrapSourceGridConstructionTests.cs
  BootstrapSourceGridThemeTests.cs
  BootstrapSourceGridViewTests.cs
  BootstrapSourceGridCompatibilityTests.cs
  BootstrapSourceGridEditorTests.cs

samples/MyDmsVn.BootstrapSourceGrid.Demo/
  MyDmsVn.BootstrapSourceGrid.Demo.csproj
  Program.cs
  MainForm.cs

vendor/
  Bootstrap5WinFormUI/  (submodule)
  sourcegrid/           (submodule)
```

Delete any proposed helper from this target structure if upstream API inspection proves it unnecessary; do not create empty abstractions only to match the diagram.

## Stage Sequence

### Stage 0 — Foundation and vendor pinning

Plan: `20260907-002-foundation-and-vendor-pinning.md`

Acceptance:

- [ ] clean clone initializes exact vendor commits;
- [ ] solution restores and builds both TFMs;
- [ ] product/test/demo projects compile with project references;
- [ ] baseline test harness cannot hang on default WinForms modal UI;
- [ ] vendor worktrees remain clean.

### Stage 1 — Control shell and theme adapter

Plan: `20260907-003-control-shell-and-theme-adapter.md`

Acceptance:

- [ ] `BootstrapSourceGrid` derives directly from `SourceGrid.Grid`;
- [ ] constructor is handle-independent and Designer-safe;
- [ ] theme adapter maps Bootstrap tokens deterministically;
- [ ] theme subscription/disposal is correct;
- [ ] consumer font override wins;
- [ ] both TFMs pass focused tests.

### Stage 2 — Cell/header/selection theming

Plan: `20260907-004-cell-header-selection-theming.md`

Acceptance:

- [ ] default cells, alternate rows, row headers, and column headers use Bootstrap semantic colors;
- [ ] selected/focused states remain legible;
- [ ] integration-owned Views update on theme change;
- [ ] consumer custom Views are preserved;
- [ ] spans and selection behavior remain SourceGrid-compatible.

### Stage 3 — Runtime theme/DPI/Designer hardening

Plan: `20260907-005-runtime-theme-dpi-designer-hardening.md`

Acceptance:

- [ ] repeated runtime theme switches preserve data/selection;
- [ ] Bootstrap-owned metrics handle DPI transitions;
- [ ] repeated handle/dispose cycles do not duplicate event subscriptions or leak owned fonts;
- [ ] Designer smoke passes;
- [ ] manual 100/150/200% DPI smoke passes.

### Stage 4 — Editor and interaction hardening

Plan: `20260907-006-editor-and-interaction-hardening.md`

Acceptance:

- [ ] safely supported default editors receive coherent theme styling;
- [ ] commit/cancel semantics remain SourceGrid-owned;
- [ ] Tab/Shift+Tab/arrow/navigation behavior remains compatible;
- [ ] active editor scenarios do not introduce modal/hanging failures;
- [ ] unsupported editor styling remains native and documented rather than behaviorally rewritten.

### Stage 5 — Demo, packaging, release

Plan: `20260907-007-demo-packaging-release.md`

Acceptance:

- [ ] demo covers PRD scenarios;
- [ ] full validation passes both TFMs;
- [ ] package metadata and upstream baselines are documented;
- [ ] vendor license/dependency obligations are verified;
- [ ] public API review finds no unnecessary SourceGrid wrappers;
- [ ] known limitations explicitly mention native scrollbars and conservative editor theming.

## Cross-Stage Regression Checklist

Run this review at every stage boundary:

- [ ] SourceGrid data/cell APIs still work through `BootstrapSourceGrid`.
- [ ] No vendor has gained a forbidden dependency.
- [ ] No uncommitted vendor submodule changes exist.
- [ ] No consumer-assigned Font/View is overwritten by theme refresh.
- [ ] Selection and active position are preserved through visual updates.
- [ ] Span behavior is unchanged.
- [ ] No per-cell theme event subscriptions were introduced.
- [ ] No custom scrollbar/editor engine has slipped into MVP scope.
- [ ] Both target frameworks are considered in code and tests.
- [ ] Canonical docs are updated if a durable rule changed.

## Full Validation Command

```powershell
git submodule update --init --recursive
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
```

Expected result at release: all commands exit `0`, vendor submodules point at documented commits, and no GUI test requires human interaction.

## Execution Model

Implement one stage at a time. Recommended branch/PR granularity is one stage per PR so architecture, compatibility, and UI behavior can be reviewed before later stages build on them.

After every stage:

```powershell
git status --short
git submodule status
```
