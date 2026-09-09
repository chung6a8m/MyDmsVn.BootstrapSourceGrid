# Editor Replacement Architecture and Ownership Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Prove the SourceGrid/Bootstrap editor lifecycle, sharing, disposal, sizing, and ownership rules before shipping any public Bootstrap editor adapter.

**Architecture:** Use a test-only BootstrapTextBox-backed probe editor to exercise the real `EditorControlBase` lifecycle. Add only the minimum internal grid-owned disposal/ownership seam required by evidence; defer the public registry surface until all three editor adapters are proven. The pinned SourceGrid attach sequence is treated as a hard boundary: integration code must not claim deterministic fail-before-attach cross-grid enforcement unless a separately approved SourceGrid seam makes that possible.

**Tech Stack:** C#, WinForms, SourceGrid 5.0, MyDmsVn.Bootstrap5WinFormUI, NUnit, `net48`, `net8.0-windows`.

**Spec:** `docs/EDITOR_REPLACEMENT.md` sections 4–8, 12, and 14.

## Global Constraints

- SourceGrid owns edit lifecycle, validation/conversion, placement, and navigation.
- Bootstrap controls own their theme and visual state.
- Probe/real Bootstrap adapters use `UseCellViewProperties = false`.
- No per-cell editor allocation policy may be introduced.
- No cross-grid sharing may be normalized as supported behavior.
- Do not patch either vendor as part of this stage. If deterministic fail-before-attach ownership enforcement is considered mandatory, stop and obtain separate approval for the smallest SourceGrid seam before changing vendor code.
- GUI tests are STA, bounded, deterministic, and non-modal.

---

### Task 1: Re-verify exact editor seams and promote them to canonical docs

**Files:**
- Inspect: `vendor/sourcegrid/SourceGrid/SourceGrid/Cells/Editors/EditorControlBase.cs`
- Inspect: `vendor/sourcegrid/SourceGrid/SourceGrid/Cells/Editors/EditorBase.cs`
- Inspect: `vendor/sourcegrid/SourceGrid/SourceGrid/Cells/Editors/TextBox.cs`
- Inspect: `vendor/sourcegrid/SourceGrid/SourceGrid/Cells/Editors/Factory.cs`
- Inspect: pinned Bootstrap input sources for `BootstrapTextBox`, `BootstrapFormattedTextBox`, `BootstrapLookupBox`
- Modify: `docs/UPSTREAM_API_SEAMS.md`

**Interfaces:**
- Consumes: exact pinned vendor source.
- Produces: the verified lifecycle/value/lifetime contract all later adapter tasks must use.

- [ ] **Step 1: Verify SourceGrid control creation and attach sequence**

Confirm directly in the pinned source that `EditorControlBase` creates `Control` from `CreateControl()` during construction, attaches it through the grid linked-control mechanism on first edit, and uses `ShowControl`/hide logic without requiring a new control per cell.

Record the exact first-edit order, including that `InternalStartEdit(...)` calls the private `AttachControl(cellContext.Grid)` before the first protected callback that receives `CellContext` (`OnStartingEdit(...)`). Explicitly record that an adapter in the integration assembly therefore has no current SourceGrid seam that can reject a wrong-grid first edit before SourceGrid mutates its linked-control attachment state.

- [ ] **Step 2: Verify commit/cancel and first-character seams**

Record the exact call paths for `SetEditValue`, `SafeSetEditValue`, `GetEditedValue`, `SetCellValue`, `InternalEndEdit`, `OnStartingEdit`, and `OnSendCharToEditor`. Record that every concrete `EditorControlBase` subclass must implement `OnSendCharToEditor(char)` even when later tasks add richer first-character behavior.

- [ ] **Step 3: Verify focus/validation behavior**

Confirm the `Control.Validated` subscription and the exact condition that calls `EditCellContext.EndEdit(false)`. This fact is a hard dependency for Stage 3 lookup integration.

- [ ] **Step 4: Lock the cross-grid ownership boundary**

Choose and record one of these outcomes based on the pinned source:

```text
A. No vendor change (default)
   - cross-grid adapter reuse remains explicitly unsupported;
   - registry/grid ownership and documentation prevent supported creation flows from encouraging reuse;
   - no runtime guarantee claims failure before SourceGrid attachment;
   - do not add a post-attach guard that throws after LinkedControls/mGrid have already been mutated.

B. Separately approved SourceGrid seam
   - add the smallest protected/pre-attach validation hook upstream;
   - record the exact pinned/upstream change in UPSTREAM_API_SEAMS.md;
   - only then may Stage 4 require deterministic InvalidOperationException before attachment.
```

Do not defer this decision as an unspecified Stage 4 implementation detail.

- [ ] **Step 5: Verify SourceGrid factory limitation**

Confirm `Cells.Editors.Factory` remains static/hardwired and has no supported registration hook appropriate for globally replacing built-in editors. Record that the initiative uses explicit adapters instead.

- [ ] **Step 6: Verify pinned Bootstrap control contracts**

Record at least these facts in `UPSTREAM_API_SEAMS.md`:

```text
BootstrapTextBox
- composite UserControl
- protected inner Editor seam
- theme subscription/disposal behavior
- Text/SelectAll/read-only behavior used by adapter

BootstrapFormattedTextBox
- derives from BootstrapTextBox
- RawValue distinct from formatted Text
- FormatMode/formatter and undo-redo behavior used by adapter

BootstrapLookupBox
- derives from BootstrapTextBox
- SelectedValue/SelectedItem and lookup configuration
- popup/result/highlight/pending-text behavior
- CancelPendingEdit and selection APIs
```

- [ ] **Step 7: Commit only documentation corrections if pinned code differs from the current canonical record**

```powershell
git add docs/UPSTREAM_API_SEAMS.md
git commit -m "docs: verify Bootstrap editor integration seams"
```

Skip the commit only when the canonical record already matches every verified fact.

---

### Task 2: Prove eager construction and same-grid sharing with a test-only probe editor

**Files:**
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/EditorProbes/BootstrapTextBoxProbeEditor.cs`
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapEditorOwnershipTests.cs`

**Interfaces:**
- Produces: executable evidence for eager control creation and one-editor-many-cells behavior without committing the production adapter API.

- [ ] **Step 1: Add a minimal test-only probe editor**

Implement:

```csharp
internal sealed class BootstrapTextBoxProbeEditor : SourceGrid.Cells.Editors.EditorControlBase
{
    public BootstrapTextBoxProbeEditor()
        : base(typeof(string))
    {
        UseCellViewProperties = false;
    }

    public BootstrapTextBox BootstrapControl => (BootstrapTextBox)Control;

    protected override Control CreateControl() => new BootstrapTextBox();

    public override void SetEditValue(object editValue)
        => BootstrapControl.Text = editValue?.ToString() ?? string.Empty;

    public override object GetEditedValue()
        => BootstrapControl.Text;

    protected override void OnSendCharToEditor(char key)
        => BootstrapControl.Text = key.ToString();
}
```

Use the exact method access modifiers required by the pinned SourceGrid base after Task 1 verification; do not change vendor source. The probe only needs a compile-safe first-character seam; the production text adapter defines the full caret/selection contract in Stage 1.

- [ ] **Step 2: Write an eager-construction test**

Construct the probe editor without a grid and assert `BootstrapControl` already exists before any `StartEdit` call. This test documents why per-cell allocation is forbidden.

- [ ] **Step 3: Write a same-grid sharing test**

Create one `BootstrapSourceGrid`, two editable cells, assign the same probe editor to both, edit/commit the first cell, then edit/commit the second. Assert both values are updated independently and no second Bootstrap control is created.

- [ ] **Step 4: Run focused tests on both TFMs**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --filter BootstrapEditorOwnershipTests --blame-hang --blame-hang-timeout 5m
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapEditorOwnershipTests --blame-hang --blame-hang-timeout 5m
```

Expected: PASS with no modal UI.

- [ ] **Step 5: Commit the durable probe tests**

```powershell
git add tests/MyDmsVn.BootstrapSourceGrid.Tests/EditorProbes tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapEditorOwnershipTests.cs
git commit -m "test: prove Bootstrap editor lifetime assumptions"
```

---

### Task 3: Add internal grid-owned editor disposal registration

**Files:**
- Create: `src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapSourceGridEditorRegistry.cs`
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Controls/BootstrapSourceGrid.cs`
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapEditorOwnershipTests.cs`

**Interfaces:**
- Produces: an internal registry that owns `EditorControlBase` instances created for one grid and disposes their controls even if never attached.
- Consumed later by: Stages 1–4.

- [ ] **Step 1: Add failing disposal tests**

Cover both cases:

```text
RegisteredEditorUsedByCell_IsDisposedWithGrid
RegisteredEditorNeverStarted_IsDisposedWithGrid
```

Use a test editor/control that exposes a boolean set in `Dispose(bool)` so the assertion does not rely on indirect GC behavior.

- [ ] **Step 2: Implement an internal ownership container**

Create an internal registry with one responsibility:

```csharp
internal sealed class BootstrapSourceGridEditorRegistry : IDisposable
{
    private readonly List<EditorControlBase> _ownedEditors = new();

    internal T Register<T>(T editor) where T : EditorControlBase
    {
        if (editor == null) throw new ArgumentNullException(nameof(editor));
        _ownedEditors.Add(editor);
        return editor;
    }

    public void Dispose()
    {
        foreach (var editor in _ownedEditors)
        {
            editor.Control.Dispose();
        }
        _ownedEditors.Clear();
    }
}
```

Before finalizing, inspect whether the pinned SourceGrid editor itself implements `IDisposable`; if it does, dispose through that contract instead of disposing only `Control`. Record the exact choice in `UPSTREAM_API_SEAMS.md`.

- [ ] **Step 3: Make `BootstrapSourceGrid` own one registry instance**

Construct the registry with the grid and dispose it from `BootstrapSourceGrid.Dispose(bool)` before/with the existing theme-resource cleanup. Do not expose it publicly in this stage.

- [ ] **Step 4: Run disposal and existing lifecycle tests**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --filter "BootstrapEditorOwnershipTests|BootstrapSourceGridThemeLifecycleTests" --blame-hang --blame-hang-timeout 5m
```

Expected: PASS on both TFMs.

- [ ] **Step 5: Commit**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapSourceGridEditorRegistry.cs src/MyDmsVn.BootstrapSourceGrid/Controls/BootstrapSourceGrid.cs tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapEditorOwnershipTests.cs docs/UPSTREAM_API_SEAMS.md
git commit -m "feat: add grid-owned editor lifetime registry"
```

---

### Task 4: Lock styler opt-out and sizing behavior

**Files:**
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridEditorTests.cs`
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapEditorSizingTests.cs`

**Interfaces:**
- Produces: regression gates for Bootstrap-owned visuals and compact-row sizing.

- [ ] **Step 1: Add a styler opt-out test**

Start editing with a BootstrapTextBox-backed probe whose `UseCellViewProperties` is false. Change the runtime Bootstrap theme and assert `BootstrapSourceGridEditorStyler` does not overwrite the probe control's Bootstrap-owned font/background/foreground from the cell View.

- [ ] **Step 2: Add preferred-height tests**

Record `BootstrapTextBox.GetPreferredSize(...)` behavior and verify SourceGrid can place the editor into a deliberately shorter row without the integration silently mutating `Rows[row].Height`.

- [ ] **Step 3: Add DPI-focused size checks**

Where deterministic automation is possible, verify preferred/minimum size calculations under the test DPI seam. Keep 100/150/200% real-monitor validation as a manual gate if the test environment cannot change process DPI reliably.

- [ ] **Step 4: Run focused tests on both TFMs**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --filter "BootstrapEditorSizingTests|BootstrapSourceGridEditorTests" --blame-hang --blame-hang-timeout 5m
```

Expected: PASS, no row-height side effects.

- [ ] **Step 5: Commit**

```powershell
git add tests/MyDmsVn.BootstrapSourceGrid.Tests
git commit -m "test: lock Bootstrap editor styling and sizing boundaries"
```

---

### Task 5: Stage gate and documentation sync

**Files:**
- Modify only if evidence changed: `docs/EDITOR_REPLACEMENT.md`, `docs/UPSTREAM_API_SEAMS.md`

- [ ] **Step 1: Run full validation**

```powershell
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Expected: all commands exit `0`; vendor status commands return no output.

- [ ] **Step 2: Confirm the Stage 1 contract**

Before moving on, the repository must explicitly support these statements:

```text
one adapter can serve many cells in one grid
Bootstrap control is created eagerly
Bootstrap adapter visuals are not copied from cell View
registry/grid owns disposal
row height is never silently resized by adapter
cross-grid reuse is unsupported
without an approved SourceGrid pre-attach seam, no deterministic fail-before-attach runtime guard is promised
```

- [ ] **Step 3: Commit any final canonical-doc correction**

```powershell
git add docs/EDITOR_REPLACEMENT.md docs/UPSTREAM_API_SEAMS.md
git commit -m "docs: finalize Bootstrap editor ownership contract"
```

Skip if no documentation changed.
