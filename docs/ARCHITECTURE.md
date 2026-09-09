# Architecture

## 1. System boundary

`MyDmsVn.BootstrapSourceGrid` is an integration layer between two independent vendors:

```text
Application
    |
    v
BootstrapSourceGrid : SourceGrid.Grid
    +--> Bootstrap5WinFormUI  (theme/control semantics)
    `--> SourceGrid           (grid/edit behavior)
```

It is not a new grid engine, a SourceGrid fork, or a wrapper that re-exposes SourceGrid APIs.

Pinned-vendor implementation facts belong in `UPSTREAM_API_SEAMS.md`; active editor-specific design belongs in `EDITOR_REPLACEMENT.md`.

## 2. Stable ownership

### SourceGrid owns

- concrete/virtual grid model;
- cells, rows, columns, ranges, positions, spans;
- selection and active position;
- controllers and keyboard/mouse navigation;
- editor lifecycle, placement, final validation/conversion;
- scrolling;
- painting orchestration and View contracts.

### Bootstrap5WinFormUI owns

- current theme and runtime theme notifications;
- semantic colors, metrics, typography;
- DPI helpers;
- visual and interactive behavior of Bootstrap controls.

### BootstrapSourceGrid owns

- translating Bootstrap theme tokens into SourceGrid visual state;
- integration-owned Views/styles;
- BootstrapSourceGrid font/resource lifecycle;
- narrow editor adapters between SourceGrid and Bootstrap controls;
- grid-owned Bootstrap editor lifetime;
- tests, demo, docs, and package metadata.

Neither vendor may gain a dependency on this integration or on the other vendor because of it.

## 3. Inheritance and public model

```text
System.Windows.Forms.Panel
    -> SourceGrid.CustomScrollControl
    -> SourceGrid.GridVirtual
    -> SourceGrid.Grid
    -> MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid
```

Direct inheritance preserves SourceGrid's concrete API. Do not add `BootstrapRow`, `BootstrapColumn`, `BootstrapCell`, or similar wrappers merely for naming consistency.

## 4. Theme translation

The integration captures a coherent Bootstrap theme snapshot and maps semantic tokens into integration-owned SourceGrid visual objects.

Conceptual flow:

```text
BootstrapThemeManager.CurrentTheme
        -> BootstrapSourceGridThemeAdapter
        -> integration-owned cell/header Views
        -> selection visual state
        -> BootstrapSourceGrid-owned font/metrics
```

Rules:

- use semantic tokens rather than hard-coded Bootstrap colors;
- preserve consumer-assigned Views and Fonts;
- scale only integration-owned metrics through `DpiScaler`;
- do not rebuild grid data on theme changes;
- unsubscribe theme events and dispose integration-owned resources deterministically.

## 5. SourceGrid View integration

SourceGrid's Model/View/Editor/Controller separation makes Views the primary visual integration seam.

The implementation may replace only known SourceGrid default View singleton identities with shared BootstrapSourceGrid Views. Unknown/custom consumer View instances remain authoritative.

Shared Views must not store mutable cell-specific state. Alternating-row decisions may use the current `CellContext.Position` at draw/measure time rather than allocating one View per row.

Do not replace the complete SourceGrid paint engine when a View/VisualModel seam is sufficient.

Exact default identities, header visual-element seams, and the `GetCell(...)` interception constraint are documented in `UPSTREAM_API_SEAMS.md`.

## 6. Selection

SourceGrid continues to own selected ranges and active position. BootstrapSourceGrid changes only integration-owned selection visual properties.

Theme refresh must never clear/recreate selection merely to recolor it. If application code changes a selection visual property after the integration applied it, that property becomes consumer-owned and later theme changes must preserve it.

## 7. Font and DPI ownership

BootstrapSourceGrid starts in theme-font mode using Bootstrap body typography. If application code assigns `Font`, that grid instance enters consumer-font mode; the integration no longer replaces or disposes that font.

Only integration-owned metrics are DPI-scaled. SourceGrid/application row heights, column widths, scrollbar dimensions, and other externally owned sizes are not silently rescaled.

## 8. Editor architecture

The completed MVP used SourceGrid's native editors plus `BootstrapSourceGridEditorStyler` for safe appearance alignment.

Post-MVP Bootstrap-native editors extend, rather than replace, that architecture:

```text
SourceGrid EditorControlBase lifecycle
        |
        v
Bootstrap adapter
        |
        v
Bootstrap5WinFormUI control
```

### SourceGrid remains authoritative for

- edit start/end;
- commit/cancel;
- editor placement/show/hide;
- final validation/type conversion;
- grid navigation.

### Bootstrap editor control remains authoritative for

- theme/font/background/border/focus visuals;
- control-specific interaction such as formatting or lookup popup/search.

### Integration adapter owns

- value transfer into/out of the Bootstrap control;
- first-character/caret adaptation where required;
- event-ordering glue when SourceGrid lifecycle and Bootstrap child/popup focus interact;
- explicit association with the grid-owned registry and deterministic owned-resource disposal.

Bootstrap adapters default `UseCellViewProperties = false`; the legacy editor styler must not push SourceGrid cell View properties into them.

Canonical editor design: `EDITOR_REPLACEMENT.md`.

## 9. Bootstrap editor lifetime

SourceGrid `EditorControlBase` eagerly creates its WinForms control. Composite Bootstrap inputs can also own child controls and global theme subscriptions.

Therefore the default lifetime model is:

```text
BootstrapSourceGrid
    -> BootstrapSourceGridEditorRegistry
        -> small set of shared editor adapters
            -> one Bootstrap control per adapter
```

Normally create one adapter per grid/column/configuration and assign that same editor to many cells.

The public creation surface is:

```csharp
BootstrapSourceGridEditorRegistry BootstrapSourceGrid.BootstrapEditors { get; }

BootstrapTextBoxEditor CreateTextBox(Type valueType);
BootstrapFormattedTextBoxEditor CreateFormattedTextBox(Type valueType);
BootstrapLookupBoxEditor CreateLookupBox(Type valueType);
```

Never:

- create a Bootstrap editor automatically for every cell;
- share one adapter across multiple grid instances;
- patch SourceGrid's static editor factory to make replacement global.

The registry/grid disposes every adapter/control it creates, including editors never used to start an edit.

Cross-grid reuse is unsupported. At the pinned SourceGrid baseline, no protected integration
callback runs before attachment mutates `mGrid` and `LinkedControls`; therefore the integration
does not add an unsafe post-attach guard or promise a fail-before-attach exception.

## 10. Initial Bootstrap editor value bridges

```text
BootstrapTextBox          -> Text
BootstrapFormattedTextBox -> RawValue
BootstrapLookupBox        -> SelectedValue
```

These are edit-layer logical values only. SourceGrid remains responsible for converting/validating them into the declared cell type.

Formatted display text and lookup display text are presentation, not substitutes for the logical value.

Future Bootstrap editor integrations such as combo-box, date/time, or numeric inputs follow this
sequence: prove the logical value contract; prove SourceGrid lifecycle and first-key behavior;
use grid-owned shared lifetime; lock popup/focus semantics when applicable; then add the registry
creator. Future controls are not implied by the initial three adapters.

## 11. Lookup focus/popup boundary

`BootstrapLookupBox` is the highest-risk initial adapter because its popup/result focus behavior intersects SourceGrid's `Control.Validated` end-edit path.

The integration must solve that at the adapter boundary and explicitly test event order. It must not globally disable SourceGrid validation/focus semantics or fork the lookup popup controller without a separately proven vendor bug.

Required interaction coverage is listed in `EDITOR_REPLACEMENT.md` and the active lookup stage plan.

## 12. Sizing

SourceGrid places editor controls inside cell bounds; Bootstrap inputs have theme-driven preferred/minimum heights.

The integration does not silently change SourceGrid row heights to fit Bootstrap editors. Preferred-size behavior is tested, while consumer/application row sizes remain authoritative.

## 13. Scrollbars

SourceGrid's `CustomScrollControl` continues to own native scrollbar layout and behavior. Scrollbar replacement is outside the current editor initiative and requires separate architecture approval.

## 14. Designer and lifecycle

Control/editor construction must be safe when no handle exists and no application theme initialization has run.

Every global event subscription and owned WinForms/GDI resource must have a deterministic disposal path. Automated GUI tests must be STA, bounded, and non-modal.

## 15. Performance principles

- no grid-data rebuild on theme changes;
- no default per-cell composite editor controls;
- prefer shared Views/adapters where SourceGrid safely supports sharing;
- avoid hot-paint allocations;
- test lifetime/object-count invariants rather than brittle wall-clock thresholds;
- measure before introducing complex caching.

## 16. Packaging boundary

The integration remains its own assembly/package and does not merge vendor source/binaries into one assembly. Development uses pinned vendor project references; public package release remains gated by the exact verified vendor-package strategy in `DECISIONS.md` and `RELEASE.md`.
