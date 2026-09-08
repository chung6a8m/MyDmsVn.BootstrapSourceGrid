# Development plan

## 1. Strategy

Implementation is split into small stages so each review can reject or accept an independently testable integration boundary. Do not collapse all stages into one large PR unless explicitly requested.

Detailed task plans live under `docs/plans/`.

## 2. Stage 0 — Repository foundation and pinned vendors

**Outcome:** reproducible dual-target solution with pinned vendor source, empty control/test/demo projects, and safe WinForms test harness.

Deliverables:

- solution/project structure;
- Git submodules pinned to approved commits;
- product project targeting `net48;net8.0-windows`;
- test project targeting both TFMs;
- demo project;
- project references to both vendors;
- baseline build/test commands;
- unattended WinForms test safeguards.

Gate:

- clean clone + submodule init restores and builds both TFMs;
- baseline tests run without modal UI;
- no vendor source modification.

Plan: `plans/20260907-002-foundation-and-vendor-pinning.md`.

## 3. Stage 1 — BootstrapSourceGrid shell and theme adapter

**Outcome:** `BootstrapSourceGrid : SourceGrid.Grid` exists, is Designer-safe, subscribes to runtime themes correctly, and can map theme tokens without yet styling every cell category.

Deliverables:

- public control shell;
- theme snapshot/adapter;
- theme-owned font lifecycle;
- theme event subscribe/unsubscribe;
- DPI helper for integration-owned metrics;
- initial tests.

Gate:

- exact inheritance/namespace/API identity verified;
- construction works before handle creation;
- repeated theme switching/disposal is safe;
- consumer font override is preserved.

Plan: `plans/20260907-003-control-shell-and-theme-adapter.md`.

## 4. Stage 2 — Cell, header, alternate row, selection visual integration

**Outcome:** SourceGrid visual extension points render Bootstrap-aligned default cell/generic-header/row-header/column-header states while preserving consumer Views and SourceGrid behavior.

Deliverables:

- default cell View/style;
- alternating-row View/style;
- generic-header View/style;
- column-header View/style;
- row-header View/style;
- selection/focus visual integration;
- read-only/disabled visual treatment where safe;
- ownership tracking for integration Views;
- compatibility tests.

Gate:

- light/dark/custom theme tests pass;
- consumer custom View remains untouched on theme switch;
- selection/active position and spans remain unchanged;
- no broad SourceGrid paint-engine fork.

Plan: `plans/20260907-004-cell-header-selection-theming.md`.

## 5. Stage 3 — Runtime theme, DPI, Designer, lifecycle hardening

**Outcome:** visual integration remains correct across theme changes, DPI transitions, handle lifecycle, repeated disposal/recreation, and Designer use.

Deliverables:

- runtime theme refresh path;
- DPI recalculation/invalidation path;
- handle lifecycle tests;
- Designer-safe defaults/property metadata;
- resource/event leak hardening;
- manual Designer/DPI matrix.

Gate:

- no data/selection reset on theme switch;
- no duplicate theme subscriptions;
- no integration-owned GDI leaks in repeated lifecycle tests;
- 100/150/200% DPI manual smoke passes;
- Designer smoke passes.

Plan: `plans/20260907-005-runtime-theme-dpi-designer-hardening.md`.

## 6. Stage 4 — Editor and interaction compatibility hardening

**Outcome:** representative SourceGrid editors look coherent enough for MVP without replacing editor architecture, and input/focus/navigation remains SourceGrid-compatible.

Deliverables:

- editor appearance bridge for safely supported editor controls;
- editing commit/cancel tests;
- Tab/Shift+Tab/arrow/Page navigation regression tests where supported;
- active-editor theme-switch behavior decision backed by tests;
- focus/accessibility regression checks.

Gate:

- representative default editors can edit/commit/cancel on both TFMs;
- no custom editor engine introduced;
- keyboard navigation remains compatible;
- any editor that cannot safely be themed is explicitly documented rather than behaviorally rewritten.

Plan: `plans/20260907-006-editor-and-interaction-hardening.md`.

## 7. Stage 5 — Demo, packaging, documentation, release preparation

**Outcome:** consumer-ready MVP with demo, package metadata, reproducible validation, and complete docs.

Deliverables:

- demo scenarios from PRD;
- package metadata/readme;
- compatibility and upstream notices;
- full test script/workflow if appropriate;
- release checklist;
- final API review;
- no accidental vendor patches.

Gate:

- full build/test matrix passes;
- demo covers required scenarios;
- docs match shipped API;
- package includes correct target assets/dependency declarations;
- release notes state known editor/scrollbar limitations.

Plan: `plans/20260907-007-demo-packaging-release.md`.

## 8. Post-MVP candidates

These are intentionally not part of the initial release and require separate brainstorming/design approval:

- Bootstrap-specific custom editor suite;
- custom scrollbar rendering/replacement;
- `BootstrapSourceGridVirtual` or another explicit virtual-grid integration type;
- richer empty/loading states similar to `BootstrapDataGridView`;
- Bootstrap variants/sizes as public SourceGrid-specific API;
- advanced accessibility/automation enhancements;
- source-generated or configuration-driven View factories.

Do not implement these opportunistically while completing MVP stages.

## 9. Cross-stage invariants

Every stage must preserve:

- dependency direction;
- SourceGrid public programming model;
- dual-target support;
- zero/near-zero vendor patch posture;
- automatic runtime theme lifecycle;
- consumer font/View ownership;
- non-interactive automated tests;
- scrollbar/editor MVP boundaries.

## 10. Stage execution discipline

Each stage should normally be implemented on its own branch/PR. Before moving to the next stage:

1. complete focused tests;
2. build both TFMs;
3. complete relevant manual checks;
4. review API/diff for accidental SourceGrid behavior changes;
5. update canonical docs if a durable rule changed;
6. merge/accept the stage.
