# AI_CONTEXT.md

Compact current context for AI assistants working on `MyDmsVn.BootstrapSourceGrid`.

For mandatory operating rules read `AGENTS.md`. For historical context, see [Archive](./docs/archive/).

## Identity

```text
Repository:     chung6a8m/MyDmsVn.BootstrapSourceGrid
Product:        MyDmsVn.BootstrapSourceGrid
Public control: MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid
Base type:      SourceGrid.Grid
TFMs:           net48;net8.0-windows
License:        MIT
```

Pinned vendors:

- Bootstrap5WinFormUI `95077df0c8bad8593143c2190606d2f444bfc653`
- SourceGrid `f4e457b43582bf01892f50bdc74aa480531e5944`

Development uses pinned submodules + `ProjectReference`. Public NuGet publication remains gated until exact vendor packages are verified against approved source baselines and license/dependency requirements.

## Stable architecture

```text
Application
    |
    v
BootstrapSourceGrid
    +--> Bootstrap5WinFormUI  (theme/control semantics)
    `--> SourceGrid           (grid/edit behavior)
```

SourceGrid owns grid data, cells, rows/columns, ranges/spans, selection, controllers, keyboard/mouse navigation, editor lifecycle/placement/validation/conversion, scrolling, and painting orchestration.

Bootstrap5WinFormUI owns theme selection, semantic colors/metrics/typography, DPI helpers, and Bootstrap control behavior.

This repository owns only the integration/translation/adapters between them.

## Completed MVP

The first roadmap is complete. It delivered:

- `BootstrapSourceGrid : SourceGrid.Grid`;
- Bootstrap-themed default cells/headers/selection;
- runtime theme and DPI lifecycle;
- consumer View/font ownership rules;
- conservative styling of existing SourceGrid editors;
- dual-TFM tests/demo/package preparation.

The completed roadmap is archived and should not be loaded during normal active work.

## Active initiative — Bootstrap-native editors

Canonical design: `docs/EDITOR_REPLACEMENT.md`.

Active master roadmap: `docs/plans/20260909-001-bootstrap-editor-replacement-master-roadmap.md`.

Initial targets:

1. `BootstrapTextBox`
2. `BootstrapFormattedTextBox`
3. `BootstrapLookupBox`

Core formula:

```text
SourceGrid editor lifecycle + thin adapter + Bootstrap control + grid-owned shared lifetime
```

### Ownership

| Concern | Owner |
| --- | --- |
| Cell rendering | BootstrapSourceGrid Views |
| Editor rendering/theme | Bootstrap control |
| Editor placement/show/hide | SourceGrid |
| Start/commit/cancel | SourceGrid |
| Final validation/type conversion | SourceGrid |
| Formatting/popup/search | Bootstrap control |
| Grid navigation | SourceGrid |
| Adapter/control lifetime | BootstrapSourceGrid editor registry |

### Mandatory editor rules

- Bootstrap adapters derive from SourceGrid `EditorControlBase`.
- They default `UseCellViewProperties = false` so SourceGrid cell View font/colors are not pushed onto composite Bootstrap controls.
- `BootstrapSourceGridEditorStyler` remains for legacy SourceGrid editors only when `UseCellViewProperties == true`.
- Do not patch SourceGrid's static `Cells.Editors.Factory` for global replacement.
- Do not create one Bootstrap composite editor per cell.
- Share one adapter within one grid, normally per column/configuration.
- Never share one adapter across grid instances.
- Grid/registry owns disposal, including adapters never attached to a grid.
- Consumer custom editors remain untouched.

Logical value bridges:

```text
BootstrapTextBox          -> Text
BootstrapFormattedTextBox -> RawValue
BootstrapLookupBox        -> SelectedValue
```

SourceGrid performs final typed conversion/validation after the adapter returns the logical value.

## Critical verified SourceGrid editor seams

At the pinned baseline, `EditorControlBase`:

- creates its WinForms control eagerly through `CreateControl()` during editor construction;
- attaches the control to a grid when editing starts;
- manages show/position/focus/hide;
- initializes/restores through `SetEditValue` / `SafeSetEditValue`;
- commits through `GetEditedValue` -> `SetCellValue`;
- performs SourceGrid validation/type conversion before the cell gets its final value;
- handles first-character editing through `OnSendCharToEditor`;
- listens to `Control.Validated` and can end/commit the edit;
- uses control preferred size for minimum editor sizing.

Because control creation is eager, per-cell Bootstrap adapters are a scalability/lifetime anti-pattern.

Exact seams belong in `docs/UPSTREAM_API_SEAMS.md`; re-verify them after vendor baseline changes.

## Bootstrap control specifics

### BootstrapTextBox

Composite themed input with protected inner editor. The adapter may use a narrow internal subclass for select-all/caret/first-character behavior, but must not expose the native child publicly.

### BootstrapFormattedTextBox

Separates formatted `Text` from canonical `RawValue`. Commit `RawValue`; formatting is not a replacement for SourceGrid's declared cell type.

### BootstrapLookupBox

Uses `SelectedValue` as logical value and owns datasource/display/value/search/result/popup/highlight/pending-text behavior. The difficult seam is popup/focus event ordering versus SourceGrid's `Control.Validated` auto-end-edit path.

Lookup tests must lock Enter, Escape, Tab/Shift+Tab, arrows/Page navigation, mouse result selection, outside click, focus transfer, deactivation/Alt+Tab-equivalent behavior, theme switch while popup is open, unmatched text, validation failure, and disposal.

## Current planned public API direction

After the three adapter slices are proven:

```csharp
public BootstrapSourceGridEditorRegistry BootstrapEditors { get; }

BootstrapTextBoxEditor CreateTextBox(Type valueType);
BootstrapFormattedTextBoxEditor CreateFormattedTextBox(Type valueType);
BootstrapLookupBoxEditor CreateLookupBox(Type valueType);
```

Each adapter exposes a read-only strongly typed `BootstrapControl` property for normal Bootstrap-specific configuration. Constructors remain non-public so grid ownership is explicit.

## Testing rules

- Test both `net48` and `net8.0-windows`.
- GUI tests are STA and bounded with hang diagnostics.
- No modal/default exception dialogs may wait for input.
- Use object/lifetime/count assertions for editor sharing; avoid brittle performance timing thresholds.
- Theme tests restore global `BootstrapThemeManager.CurrentTheme` after mutation.
- Vendor worktrees must remain clean.

GitHub Actions is disabled as of 2026-09-09; local build/test commands are the required automated gate.

## Source-of-truth order

When current documents conflict:

1. explicit current user instruction;
2. `docs/DECISIONS.md`;
3. `docs/ARCHITECTURE.md`;
4. active scoped spec (`docs/EDITOR_REPLACEMENT.md` for editor work);
5. `docs/UPSTREAM_API_SEAMS.md` for exact pinned-vendor facts;
6. relevant compatibility/testing/upstream docs;
7. `docs/DEVELOPMENT_PLAN.md`;
8. active implementation plan under `docs/plans/`;
9. pinned vendor source/tests.

Archived plans are historical evidence only and do not override current canonical docs.
