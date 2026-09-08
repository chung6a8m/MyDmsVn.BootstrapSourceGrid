# Bootstrap editor replacement architecture

## 1. Status and goal

This is the active post-MVP editor initiative for `MyDmsVn.BootstrapSourceGrid`.

The completed MVP deliberately kept SourceGrid's built-in editors and only aligned their appearance. The next objective is to add Bootstrap-native editing controls without replacing SourceGrid's edit lifecycle.

Initial target controls:

1. `BootstrapTextBox`
2. `BootstrapFormattedTextBox`
3. `BootstrapLookupBox`

The target architecture is:

```text
SourceGrid edit lifecycle
        |
        v
thin Bootstrap editor adapter
        |
        v
Bootstrap5WinFormUI control
```

SourceGrid remains the grid/editor engine; Bootstrap5WinFormUI remains the control/theme owner; this repository owns only the adapter and lifetime integration.

For historical context, see [Archive](./archive/).

## 2. Non-goals

This initiative does not:

- replace SourceGrid's `StartEdit` / `EndEdit` lifecycle;
- replace SourceGrid validation or final typed conversion;
- patch SourceGrid's global `Cells.Editors.Factory`;
- modify either vendor merely to simplify the adapter;
- create one Bootstrap composite editor control per cell by default;
- share one editor instance across multiple grid instances;
- make Bootstrap controls imitate SourceGrid cell Views by copying View colors/fonts into them;
- replace SourceGrid scrolling, selection, navigation, or painting;
- introduce `BootstrapCell`, `BootstrapColumn`, or other wrappers around SourceGrid abstractions.

## 3. Fixed constraints

- Repository: `chung6a8m/MyDmsVn.BootstrapSourceGrid`
- Public control: `MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid`
- Base type: `SourceGrid.Grid`
- TFMs: `net48;net8.0-windows`
- Bootstrap baseline: `95077df0c8bad8593143c2190606d2f444bfc653`
- SourceGrid baseline: `f4e457b43582bf01892f50bdc74aa480531e5944`
- Vendor dependency direction remains one-way from this integration.
- GUI tests are STA, bounded, deterministic, and non-modal.

## 4. Ownership model

| Concern | Owner |
| --- | --- |
| Cell rendering | BootstrapSourceGrid Views |
| Editor rendering/theme | Bootstrap5WinFormUI editor control |
| Editor placement/visibility | SourceGrid |
| Start/commit/cancel lifecycle | SourceGrid |
| Final typed conversion | SourceGrid `EditorBase` / validator path |
| Bootstrap-specific interactive formatting | Bootstrap editor control |
| Lookup popup/search/highlight | `BootstrapLookupBox` |
| Grid navigation | SourceGrid |
| Editor instance lifetime | `BootstrapSourceGrid` editor registry |

The adapter must not become a second editor framework.

## 5. Verified SourceGrid lifecycle that adapters must preserve

At the pinned SourceGrid baseline, `EditorControlBase`:

- creates its WinForms `Control` eagerly in the editor constructor through `CreateControl()`;
- attaches the control to a grid only when editing starts;
- shows, positions, focuses, and hides that control as part of the normal edit lifecycle;
- commits through `GetEditedValue()` -> `SetCellValue(...)`;
- uses SourceGrid validation/type conversion before the cell receives its final value;
- uses `SafeSetEditValue(...)` when edit state is initialized/restored;
- handles first-character editing through `OnSendCharToEditor(...)`;
- listens to `Control.Validated` and may end the active edit when the control validates;
- calculates minimum editor size from `Control.GetPreferredSize(...)`.

These seams are canonicalized in `UPSTREAM_API_SEAMS.md` and must be re-verified after any vendor baseline change.

## 6. Critical lifetime rule

A Bootstrap input is a composite WinForms control and may subscribe to global theme events at construction. Because `EditorControlBase` eagerly creates its control, a naive editor-per-cell model can create thousands of controls and event subscriptions even when most cells are never edited.

Therefore:

1. Bootstrap editor adapters are shared at grid scope, normally one instance per column/configuration.
2. The same adapter may be assigned to many cells in the same `BootstrapSourceGrid` because SourceGrid permits only one active edit for that editor at a time.
3. An adapter created for one grid must not be reused by another grid.
4. `BootstrapSourceGrid` owns and disposes every adapter/control created through its editor registry, including controls that were created but never attached to `LinkedControls`.
5. No automatic per-cell Bootstrap editor creation is allowed in the default cell factory path.

Cross-grid reuse is unsupported, but the pinned SourceGrid attach sequence provides no integration-level callback before it mutates the editor's grid/linked-control attachment. The current no-vendor-change contract therefore does not promise a deterministic fail-before-attach runtime exception and does not add a post-attach guard that would leave SourceGrid in partially mutated state. Such enforcement requires a separately approved SourceGrid pre-attach seam.

## 7. Registry and public API direction

The integration will expose a grid-owned registry after the three vertical slices prove the common adapter contract:

```csharp
public BootstrapSourceGridEditorRegistry BootstrapEditors { get; }
```

The registry creates owned/shared adapter instances explicitly:

```csharp
BootstrapTextBoxEditor CreateTextBox(Type valueType);
BootstrapFormattedTextBoxEditor CreateFormattedTextBox(Type valueType);
BootstrapLookupBoxEditor CreateLookupBox(Type valueType);
```

Each returned editor is intended to be configured once and then assigned to many cells in that grid. The adapter exposes its strongly typed Bootstrap control through a read-only property named `BootstrapControl` so existing Bootstrap control APIs remain available without wrapper duplication.

Example target usage:

```csharp
var nameEditor = grid.BootstrapEditors.CreateTextBox(typeof(string));
nameEditor.BootstrapControl.PlaceholderText = "Customer name";

for (var row = 1; row < grid.RowsCount; row++)
{
    grid[row, 1].Editor = nameEditor;
}
```

The registry does not mutate SourceGrid's global static editor `Factory` and does not silently replace a consumer-assigned editor.

## 8. Common adapter contract

All Bootstrap adapters derive from `SourceGrid.Cells.Editors.EditorControlBase` and must:

- set `UseCellViewProperties = false` by default;
- let the Bootstrap control own its theme, font, foreground, background, border, focus visuals, and internal child controls;
- preserve SourceGrid commit/cancel/navigation semantics;
- implement `SetEditValue`, `GetEditedValue`, and first-character behavior explicitly;
- avoid reflection into Bootstrap controls;
- use a narrow internal subclass only when protected Bootstrap members are required for caret/selection behavior;
- be safe before handle creation;
- work on both target frameworks.

`BootstrapSourceGridEditorStyler` continues to style only legacy SourceGrid editors where `UseCellViewProperties == true`; it must not restyle a Bootstrap adapter whose flag is false.

## 9. BootstrapTextBox adapter

### Value contract

```text
SourceGrid cell value -> BootstrapTextBox.Text
BootstrapTextBox.Text -> SourceGrid conversion/validation -> cell value
```

The adapter must preserve SourceGrid text-editor behaviors that users rely on:

- beginning edit with a typed character replaces the selected/current text with that character;
- caret is positioned after the inserted character;
- normal edit start selects all where the SourceGrid text editor does so;
- Enter/Escape/Tab behavior remains SourceGrid-owned unless the Bootstrap control consumes a key for an established input behavior;
- read-only and enabled states remain coherent.

Because the inner native editor is protected rather than public, a small internal subclass may expose only the caret/selection operations required by the adapter. Do not expose the native `TextBox` publicly.

## 10. BootstrapFormattedTextBox adapter

`BootstrapFormattedTextBox` separates formatted display text from canonical raw input.

The adapter contract is:

```text
SetEditValue(cellValue) -> BootstrapControl.RawValue
GetEditedValue()        -> BootstrapControl.RawValue
SourceGrid              -> converts RawValue to the declared cell value type
```

Rules:

- `Text` is display text, not the committed logical value.
- `FormatMode` and formatter options control interactive formatting only; they do not replace SourceGrid's declared cell type or validator.
- cancel/reset must restore the original logical value through the normal SourceGrid `SafeSetEditValue` path.
- tests must cover `None`, `General`, `Numeral`, `Date`, and `Time` behavior where supported by the pinned control, plus undo/redo and conversion failures.

## 11. BootstrapLookupBox adapter

`BootstrapLookupBox` has a richer state model: `DataSource`, `DisplayMember`, `ValueMember`, search/result configuration, `SelectedItem`, `SelectedValue`, popup/highlight state, pending text, and unmatched-text policy.

The adapter contract is:

```text
SetEditValue(cellValue) -> BootstrapControl.SelectedValue
GetEditedValue()        -> BootstrapControl.SelectedValue
```

Configuration is applied before edit value initialization.

`Text` and committed display text are never used as the SourceGrid logical value when a `ValueMember` is configured.

### Interaction matrix

The lookup stage must explicitly test and lock behavior for:

- Enter with popup closed/open;
- Escape with popup closed/open;
- Tab and Shift+Tab;
- Up/Down/PageUp/PageDown result navigation;
- mouse result selection;
- outside click;
- focus leaving and returning;
- Alt+Tab/application deactivation;
- runtime theme switch while popup is open;
- unmatched text;
- SourceGrid validation failure;
- disposal while popup/control has been created.

The key risk is SourceGrid's `Control.Validated` auto-end-edit behavior interacting with popup/focus transitions. The adapter must solve event ordering at the integration boundary rather than modifying lookup popup internals unless a proven vendor bug exists.

## 12. Sizing and layout

Bootstrap inputs have theme-driven preferred/minimum heights, while SourceGrid places the editor inside cell bounds.

Each vertical slice must verify:

- `GetPreferredSize`/SourceGrid minimum-size interaction;
- clipping in compact rows;
- behavior at 100%, 150%, and 200% DPI;
- no row-height mutation performed implicitly by the editor adapter;
- consumer-controlled row heights remain authoritative.

If a cell is smaller than the Bootstrap control's preferred height, the documented default is clipping/available-bounds behavior rather than silently resizing the grid.

## 13. Error and validation policy

- SourceGrid remains responsible for final cell validation/conversion.
- Bootstrap controls may present their own input/lookup validation state, but must not commit a value behind SourceGrid's lifecycle.
- Conversion errors must fail through the existing SourceGrid validation path and must not display an unbounded modal dialog during automated tests.
- The adapter must not swallow SourceGrid exceptions.

## 14. Performance and disposal gates

Before the registry API is considered stable, tests must prove:

- a large grid does not create one Bootstrap editor control per cell;
- one shared editor can edit many cells sequentially in one grid;
- unused registry-created editors are disposed with the grid;
- theme subscriptions owned by Bootstrap controls are released on disposal;
- no editor is accidentally kept alive by the registry after grid disposal;
- a cross-grid reuse attempt is rejected or otherwise prevented by the final implementation contract.

## 15. Delivery stages

1. **Stage 0 — Architecture and ownership spike**: prove lifecycle, sharing, sizing, disposal, and exact cross-grid enforcement seam.
2. **Stage 1 — BootstrapTextBox vertical slice**: establish the reference adapter contract.
3. **Stage 2 — BootstrapFormattedTextBox**: prove raw/display value adaptation while retaining SourceGrid conversion.
4. **Stage 3 — BootstrapLookupBox**: prove complex popup/focus/keyboard integration.
5. **Stage 4 — Registry, hardening, docs, expansion pattern**: expose the public grid-owned registry and prepare the repeatable pattern for `BootstrapComboBox` and later editors.

The active master roadmap is `plans/20260909-001-bootstrap-editor-replacement-master-roadmap.md`.

## 16. Definition of done

This initiative is complete when:

- all three Bootstrap editor adapters work on `net48` and `net8.0-windows`;
- SourceGrid still owns edit lifecycle, validation, conversion, and navigation;
- Bootstrap controls own their visual/theme behavior;
- no default per-cell composite editor allocation exists;
- grid-owned editor disposal is deterministic;
- lookup interaction tests are bounded and non-hanging;
- consumer custom editors remain untouched;
- demo/docs show the explicit shared-editor usage model;
- both vendor worktrees remain clean.
