# Bootstrap Editor Replacement Master Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add Bootstrap-native SourceGrid editor adapters for `BootstrapTextBox`, `BootstrapFormattedTextBox`, and `BootstrapLookupBox` while preserving SourceGrid edit lifecycle, validation, conversion, navigation, and compatibility.

**Architecture:** SourceGrid remains the editor engine. Each Bootstrap control is hosted by a thin `EditorControlBase` adapter with `UseCellViewProperties = false`; Bootstrap controls own visual/theme behavior, and a grid-owned registry owns adapter/control lifetime so composite controls are shared rather than created per cell.

**Tech Stack:** C#, Windows Forms, SourceGrid 5.0, MyDmsVn.Bootstrap5WinFormUI, NUnit, `net48`, `net8.0-windows`.

**Spec:** `docs/EDITOR_REPLACEMENT.md`; exact pinned-vendor facts in `docs/UPSTREAM_API_SEAMS.md`.

## Global Constraints

- Public grid: `MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid`.
- Base type: `SourceGrid.Grid`.
- TFMs: `net48;net8.0-windows`.
- Bootstrap baseline: `cceba3c969e28726935793a1c6ca3772bed60a35`.
- SourceGrid baseline: `f4e457b43582bf01892f50bdc74aa480531e5944`.
- SourceGrid owns start/commit/cancel, final validation/conversion, editor placement, and grid navigation.
- Bootstrap controls own their own theme/font/background/border/focus visuals.
- Bootstrap adapters default `UseCellViewProperties = false`.
- No default editor-per-cell allocation.
- No adapter instance shared across grid instances.
- Do not patch SourceGrid global editor `Factory`.
- Do not modify vendor source unless a separately proven blocker is approved.
- GUI tests are STA, bounded, deterministic, and non-modal.
- Vendor worktrees must remain clean.

---

## Target structure

```text
src/MyDmsVn.BootstrapSourceGrid/
  Controls/
    BootstrapSourceGrid.cs
  Editors/
    BootstrapSourceGridEditorStyler.cs
    BootstrapSourceGridEditorRegistry.cs
    BootstrapTextBoxEditor.cs
    BootstrapFormattedTextBoxEditor.cs
    BootstrapLookupBoxEditor.cs
    Internal/
      BootstrapSourceGridTextBoxControl.cs
      BootstrapEditorOwnershipGuard.cs

tests/MyDmsVn.BootstrapSourceGrid.Tests/
  BootstrapEditorOwnershipTests.cs
  BootstrapTextBoxEditorTests.cs
  BootstrapFormattedTextBoxEditorTests.cs
  BootstrapLookupBoxEditorTests.cs
  BootstrapEditorRegistryTests.cs
  BootstrapEditorInteractionTests.cs

samples/MyDmsVn.BootstrapSourceGrid.Demo/
  MainForm.cs
```

Create only helpers proven necessary by the stage spike; do not preserve an empty abstraction merely because it appears in this target tree.

## Stage sequence

### Stage 0 — Editor architecture and ownership spike

Plan: `docs/archive/20260909-bootstrap-editor-replacement/20260909-002-editor-replacement-architecture-and-ownership.md`

Acceptance:

- [ ] exact `EditorControlBase` lifecycle seams used by adapters are re-verified against the pinned SourceGrid source;
- [ ] eager control creation is covered by a regression test;
- [ ] one editor can edit multiple cells sequentially in one grid;
- [ ] cross-grid reuse behavior is understood and a deterministic prevention rule is implemented or locked for Stage 4;
- [ ] disposal covers editors never attached to `LinkedControls`;
- [ ] Bootstrap input preferred-height behavior is measured without silently resizing SourceGrid rows;
- [ ] `BootstrapSourceGridEditorStyler` ignores Bootstrap adapters with `UseCellViewProperties == false`.

### Stage 1 — BootstrapTextBox reference adapter

Plan: `docs/archive/20260909-bootstrap-editor-replacement/20260909-003-bootstrap-text-box-editor.md`

Acceptance:

- [ ] `BootstrapTextBoxEditor` hosts a Bootstrap text control through SourceGrid `EditorControlBase`;
- [ ] edit initialization, select-all, first typed character, caret, commit, cancel, Tab, Shift+Tab, Enter, and Escape are covered;
- [ ] SourceGrid final conversion remains authoritative;
- [ ] theme switching during edit does not force View colors/fonts into the Bootstrap control;
- [ ] enabled/read-only, disposal, DPI, and compact-row behavior are covered on both TFMs.

### Stage 2 — BootstrapFormattedTextBox adapter

Plan: `docs/archive/20260909-bootstrap-editor-replacement/20260909-004-bootstrap-formatted-text-box-editor.md`

Acceptance:

- [ ] cell value initializes `RawValue` rather than formatted `Text`;
- [ ] committed value comes from `RawValue` and passes through SourceGrid conversion/validation;
- [ ] cancel restores the original logical value;
- [ ] formatting, caret, undo/redo, and conversion-failure paths are deterministic;
- [ ] representative `None`, `General`, `Numeral`, `Date`, and `Time` scenarios pass on both TFMs.

### Stage 3 — BootstrapLookupBox adapter

Plan: `docs/archive/20260909-bootstrap-editor-replacement/20260909-005-bootstrap-lookup-box-editor.md`

Acceptance:

- [ ] cell value maps to/from `SelectedValue`;
- [ ] lookup configuration is established before value initialization;
- [ ] popup opening/closing does not accidentally commit due to SourceGrid `Validated` handling;
- [ ] Enter/Escape/Tab/Shift+Tab/arrow/Page navigation and mouse selection are locked by tests;
- [ ] outside click, focus transfer, Alt+Tab, theme switch, unmatched text, validation failure, and disposal cannot hang tests;
- [ ] selected display text is never mistaken for the logical cell value.

### Stage 4 — Registry, hardening, demo, and expansion pattern

Plan: `docs/archive/20260909-bootstrap-editor-replacement/20260909-006-editor-registry-hardening-and-expansion.md`

Acceptance:

- [ ] `BootstrapSourceGrid.BootstrapEditors` owns every editor it creates;
- [ ] public `CreateTextBox`, `CreateFormattedTextBox`, and `CreateLookupBox` APIs are documented and XML-commented;
- [ ] consumers can configure each adapter through its strongly typed `BootstrapControl` property;
- [ ] large-grid tests prove no default editor-per-cell allocation;
- [ ] consumer custom SourceGrid editors are untouched;
- [ ] demo shows shared per-column/configuration editor usage;
- [ ] docs identify the pattern for future `BootstrapComboBox`/date/time/numeric editors without implementing them opportunistically.

## Cross-stage regression checklist

Run at every stage boundary:

- [ ] SourceGrid cell/range/selection/span APIs remain unchanged.
- [ ] Commit/cancel behavior remains SourceGrid-owned.
- [ ] `BootstrapSourceGridEditorStyler` still handles legacy editors only when SourceGrid allows it.
- [ ] Bootstrap adapters remain in `UseCellViewProperties = false` mode unless a test proves a specific opt-in safe.
- [ ] No editor is automatically created for each cell.
- [ ] No adapter is silently shared across grid instances.
- [ ] No Bootstrap control's global theme subscription survives grid/editor disposal.
- [ ] No modal WinForms failure UI can block an unattended test.
- [ ] Both TFMs are tested.
- [ ] Vendor worktrees remain clean.
- [ ] Durable seam/API decisions are promoted to canonical docs.

## Full validation

```powershell
git submodule update --init --recursive
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Expected at final gate: build/tests exit `0` for both TFMs, no unattended UI waits for input, and both vendor worktrees are clean.

## Execution discipline

Implement one stage at a time. A later stage may depend on the previous stage's adapter contract, but it must not silently redesign an earlier ownership/value rule. If exact pinned-vendor source contradicts a plan assumption, stop that task, update `docs/UPSTREAM_API_SEAMS.md` and this roadmap, then continue from the corrected contract.
