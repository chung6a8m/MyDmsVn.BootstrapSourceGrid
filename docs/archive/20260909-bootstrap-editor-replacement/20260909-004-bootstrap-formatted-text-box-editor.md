# BootstrapFormattedTextBox Editor Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a BootstrapFormattedTextBox-backed SourceGrid editor that commits canonical `RawValue` while keeping SourceGrid responsible for final typed conversion and validation.

**Architecture:** Reuse the proven BootstrapTextBox adapter pattern but change the logical value bridge from formatted display `Text` to `RawValue`. Formatting, caret mapping, first-character insertion, and undo/redo remain BootstrapFormattedTextBox responsibilities; SourceGrid still owns edit lifecycle and the final cell value type.

**Tech Stack:** C#, WinForms, SourceGrid 5.0, `BootstrapFormattedTextBox`, NUnit, `net48`, `net8.0-windows`.

**Spec:** `docs/EDITOR_REPLACEMENT.md` sections 8, 10, and 12–14.

## Global Constraints

- `UseCellViewProperties = false`.
- `RawValue` is the logical edit value; formatted `Text` is presentation only.
- Do not build a second type system from `FormatMode`.
- SourceGrid performs final conversion/validation.
- One adapter/control may serve many cells in one grid.
- Every concrete `EditorControlBase` adapter implements `OnSendCharToEditor(char)` from its first compilable version.
- No vendor patch, reflection, modal error UI, or per-cell default editor creation.

---

### Task 1: Implement the RawValue adapter and first-character contract

**Files:**
- Create: `src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapFormattedTextBoxEditor.cs`
- Create only if the pinned control requires a protected insertion seam: `src/MyDmsVn.BootstrapSourceGrid/Editors/Internal/BootstrapSourceGridFormattedTextBoxControl.cs`
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapFormattedTextBoxEditorTests.cs`

**Interfaces:**
- Produces: `BootstrapFormattedTextBoxEditor` with a strongly typed `BootstrapControl` property and a compile-complete SourceGrid first-character override.
- Consumed later by: public grid editor registry.

- [ ] **Step 1: Write failing raw-vs-display and first-character tests**

Cover:

```text
SetEditValueInitializesRawValue
GetEditedValueReturnsRawValueNotFormattedText
CommitUsesSourceGridTypedConversion
CancelRestoresOriginalLogicalValue
SendFirstCharacterUsesFormattedInputSemantics
```

Use a format where `Text` visibly differs from `RawValue` so the test fails if the adapter commits display text. For the first-character case, start editing through SourceGrid's `SendCharToEditor` path rather than directly assigning the final raw value.

- [ ] **Step 2: Implement the adapter skeleton, including the abstract first-character seam**

Target shape:

```csharp
public sealed class BootstrapFormattedTextBoxEditor : SourceGrid.Cells.Editors.EditorControlBase
{
    internal BootstrapFormattedTextBoxEditor(Type valueType)
        : base(valueType)
    {
        UseCellViewProperties = false;
    }

    public BootstrapFormattedTextBox BootstrapControl =>
        (BootstrapFormattedTextBox)Control;

    protected override Control CreateControl()
        => new BootstrapFormattedTextBox();

    protected override void OnSendCharToEditor(char key)
    {
        // Forward through the exact formatted-input insertion seam verified in Stage 0.
        // Do not leave this override as a no-op and do not bypass the control's raw/display mapping.
    }
}
```

The target shape is intentionally explicit that `OnSendCharToEditor(char)` exists in Task 1; replace the comment body with the real verified insertion call before the Task 1 build/test checkpoint. If `BootstrapFormattedTextBox` does not expose a safe public/protected insertion operation, create the smallest internal subclass seam in this task, analogous to the Stage 1 text-box control seam. Do not postpone the abstract override to Task 3.

- [ ] **Step 3: Implement value initialization**

Map the incoming SourceGrid edit value into the canonical raw representation expected by the Bootstrap control. Prefer SourceGrid's existing string/type-conversion helper when available so culture behavior stays aligned with SourceGrid.

- [ ] **Step 4: Implement value retrieval**

Return `BootstrapControl.RawValue` from `GetEditedValue()`. Never return formatted `Text` merely because it is visible to the user.

- [ ] **Step 5: Implement first-character forwarding**

Use the exact insertion/caret seam verified against the pinned `BootstrapFormattedTextBox`. The first typed character must flow through the control's own formatting/raw-value pipeline so `RawValue`, formatted `Text`, caret mapping, and undo state remain internally consistent. Do not implement this as a no-op, synthesize a commit, or copy the plain-text editor's native manipulation without verifying formatted caret/value behavior.

- [ ] **Step 6: Run focused tests on both TFMs**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --filter BootstrapFormattedTextBoxEditorTests --blame-hang --blame-hang-timeout 5m
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapFormattedTextBoxEditorTests --blame-hang --blame-hang-timeout 5m
```

Expected: PASS.

- [ ] **Step 7: Commit**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapFormattedTextBoxEditor.cs src/MyDmsVn.BootstrapSourceGrid/Editors/Internal tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapFormattedTextBoxEditorTests.cs
git commit -m "feat: add BootstrapFormattedTextBox SourceGrid editor"
```

---

### Task 2: Lock representative formatting modes and SourceGrid conversion

**Files:**
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapFormattedTextBoxEditorTests.cs`
- Modify only if required by a proven adapter defect: `src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapFormattedTextBoxEditor.cs`

**Interfaces:**
- Produces: evidence that formatting is interactive presentation, not a competing cell-type system.

- [ ] **Step 1: Add `None` and `General` cases**

Verify plain/raw input round-trips and SourceGrid still converts to the declared target type.

- [ ] **Step 2: Add `Numeral` case**

Configure a numeric format, type/edit a value that renders with formatting separators, assert `Text` is formatted, `RawValue` is canonical, and the stored SourceGrid cell value is the declared numeric type.

- [ ] **Step 3: Add `Date` and `Time` cases**

For each mode, initialize from a SourceGrid-compatible logical value, edit, commit, and assert the final cell type/value came from SourceGrid conversion of `RawValue`, not from formatted display text.

- [ ] **Step 4: Add invalid-conversion case**

Enter a raw value that the declared SourceGrid cell type rejects. Assert the edit remains under SourceGrid validation semantics and no Bootstrap adapter exception is swallowed or converted into a modal dialog.

- [ ] **Step 5: Run focused tests**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --filter BootstrapFormattedTextBoxEditorTests --blame-hang --blame-hang-timeout 5m
```

Expected: PASS on both TFMs.

- [ ] **Step 6: Commit**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapFormattedTextBoxEditor.cs tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapFormattedTextBoxEditorTests.cs
git commit -m "test: lock formatted editor value conversion"
```

---

### Task 3: Preserve caret, first-character edge cases, undo/redo, and cancel behavior

**Files:**
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapFormattedTextBoxEditorTests.cs`
- Modify if required: `src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapFormattedTextBoxEditor.cs`
- Modify if created in Task 1: `src/MyDmsVn.BootstrapSourceGrid/Editors/Internal/BootstrapSourceGridFormattedTextBoxControl.cs`

- [ ] **Step 1: Test normal edit-start selection/caret behavior**

Use the BootstrapFormattedTextBox public/protected APIs available at the pinned baseline. Prefer its own caret mapping rather than copying the plain-text adapter's native-editor manipulation if formatted text requires mapping.

- [ ] **Step 2: Harden first-character editing across representative formats**

Extend the Task 1 first-character contract across representative formats and verify the resulting raw/display state is valid. Keep using the control-owned insertion seam established in Task 1; do not introduce a second first-character path here.

- [ ] **Step 3: Test control-owned undo/redo**

Perform edits, invoke the BootstrapFormattedTextBox undo/redo behavior, and assert SourceGrid remains in the same edit session until commit/cancel.

- [ ] **Step 4: Test SourceGrid cancel after formatted edits**

After multiple formatted changes, call the normal SourceGrid cancel path and assert the original logical value/display state is restored through `SafeSetEditValue` rather than by a new adapter-side snapshot system.

- [ ] **Step 5: Run tests on both TFMs**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --filter BootstrapFormattedTextBoxEditorTests --blame-hang --blame-hang-timeout 5m
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid/Editors tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapFormattedTextBoxEditorTests.cs
git commit -m "test: harden formatted editor interaction semantics"
```

---

### Task 4: Theme, sizing, sharing, and disposal hardening

**Files:**
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapFormattedTextBoxEditorTests.cs`
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapEditorSizingTests.cs`
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapEditorOwnershipTests.cs`

- [ ] **Step 1: Verify runtime theme switch during edit**

Assert the control follows Bootstrap theme changes without SourceGrid View font/color injection.

- [ ] **Step 2: Verify same-grid sharing across cells with identical configuration**

Configure one adapter once, assign it to multiple cells, edit them sequentially, and assert formatting settings remain stable and values remain cell-specific.

- [ ] **Step 3: Verify preferred-height/compact-row behavior**

Ensure the integration does not change SourceGrid row heights to satisfy the formatted control's preferred size.

- [ ] **Step 4: Verify unused and used adapters are disposed with the grid**

Use the internal registry seam and assert deterministic control disposal in both cases.

- [ ] **Step 5: Run focused tests and build**

```powershell
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --filter "BootstrapFormattedTextBoxEditorTests|BootstrapEditorSizingTests|BootstrapEditorOwnershipTests" --blame-hang --blame-hang-timeout 5m
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add tests/MyDmsVn.BootstrapSourceGrid.Tests src/MyDmsVn.BootstrapSourceGrid/Editors
git commit -m "test: harden formatted editor lifecycle"
```

---

### Task 5: Stage gate

- [ ] **Step 1: Run full validation**

```powershell
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Expected: all commands exit `0`; vendor status is clean.

- [ ] **Step 2: Review for duplicated conversion logic**

Reject adapter code that parses numeric/date/time values into final cell types itself when SourceGrid's existing conversion path can do it. The adapter's durable responsibility is `RawValue` transport, not type ownership.
