# BootstrapTextBox Editor Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Deliver the first production Bootstrap-native SourceGrid editor adapter using `BootstrapTextBox` as the reference implementation for value, caret, theme, lifecycle, and shared ownership behavior.

**Architecture:** `BootstrapTextBoxEditor` derives from SourceGrid `EditorControlBase`, creates one Bootstrap text control eagerly, opts out of SourceGrid View-property styling, and delegates final value conversion/commit/cancel to SourceGrid. A narrow internal BootstrapTextBox subclass exposes only the protected caret/selection operations required to reproduce SourceGrid text-editor behavior.

**Tech Stack:** C#, WinForms, SourceGrid 5.0, `BootstrapTextBox`, NUnit, `net48`, `net8.0-windows`.

**Spec:** `docs/EDITOR_REPLACEMENT.md` sections 8–9 and 12–14.

## Global Constraints

- `UseCellViewProperties = false` by default.
- `Text` is the adapter's edit value; SourceGrid performs final conversion/validation.
- One adapter/control is shared by many cells in one grid.
- No public access to the inner native `TextBox`.
- No reflection into BootstrapTextBox internals.
- SourceGrid owns commit/cancel/navigation.
- BootstrapTextBox owns theme/font/background/border/focus visuals.
- Both TFMs and non-modal STA test rules apply.

---

### Task 1: Add the narrow caret/selection control seam

**Files:**
- Create: `src/MyDmsVn.BootstrapSourceGrid/Editors/Internal/BootstrapSourceGridTextBoxControl.cs`
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapTextBoxEditorTests.cs`

**Interfaces:**
- Produces: internal methods needed by the adapter without exposing BootstrapTextBox's protected inner editor publicly.

- [ ] **Step 1: Write tests for required text-entry semantics**

Add tests that prove the target behavior:

```text
BeginEditSelectsAllText
SendFirstCharacterReplacesExistingText
SendFirstCharacterPlacesCaretAfterCharacter
CancelRestoresOriginalValue
CommitStoresEditedValue
```

Use one shared editor instance across at least two cells in the same test fixture.

- [ ] **Step 2: Implement the internal control subclass**

Create a sealed internal subclass of `BootstrapTextBox` with only these integration methods:

```csharp
internal sealed class BootstrapSourceGridTextBoxControl : BootstrapTextBox
{
    internal void SelectAllForGridEdit() => Editor.SelectAll();

    internal void ReplaceWithFirstEditCharacter(char value)
    {
        Editor.Text = value.ToString();
        Editor.SelectionStart = Editor.TextLength;
        Editor.SelectionLength = 0;
    }
}
```

If the pinned protected inner editor uses a property other than `TextLength`, use the equivalent property verified in Stage 0; keep the surface limited to these operations.

- [ ] **Step 3: Run the focused tests and verify they still fail because the production editor does not exist**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapTextBoxEditorTests
```

Expected: compile/test failure referring to missing `BootstrapTextBoxEditor`.

- [ ] **Step 4: Commit the internal seam only with its tests if the test project can compile independently; otherwise include it in Task 2's commit**

---

### Task 2: Implement BootstrapTextBoxEditor

**Files:**
- Create: `src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapTextBoxEditor.cs`
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapTextBoxEditorTests.cs`

**Interfaces:**
- Produces: `BootstrapTextBoxEditor` reference adapter and its typed `BootstrapControl` property.
- Consumed later by: public editor registry in Stage 4.

- [ ] **Step 1: Implement the adapter skeleton**

Target shape:

```csharp
public sealed class BootstrapTextBoxEditor : SourceGrid.Cells.Editors.EditorControlBase
{
    internal BootstrapTextBoxEditor(Type valueType)
        : base(valueType)
    {
        UseCellViewProperties = false;
    }

    public BootstrapTextBox BootstrapControl =>
        (BootstrapSourceGridTextBoxControl)Control;

    protected override Control CreateControl()
        => new BootstrapSourceGridTextBoxControl();
}
```

Use the exact base constructor and override visibility established in Stage 0. Keep the constructor internal so ownership creation flows through the grid registry once exposed publicly.

- [ ] **Step 2: Implement value initialization and retrieval**

Implement the SourceGrid seams so:

```text
SetEditValue(null)      -> BootstrapControl.Text = string.Empty
SetEditValue(non-null)  -> SourceGrid-compatible string representation
GetEditedValue()        -> BootstrapControl.Text
```

Use SourceGrid's existing conversion helper/validator path where available instead of inventing culture/type conversion in this adapter.

- [ ] **Step 3: Reproduce SourceGrid text-editor start behavior**

In the appropriate edit-start override, call base first/at the verified order, then call `SelectAllForGridEdit()` so normal edit start selects the current text.

- [ ] **Step 4: Reproduce first-character edit behavior**

Override the SourceGrid first-character seam and call `ReplaceWithFirstEditCharacter(...)`. Do not synthesize key events or directly commit the cell.

- [ ] **Step 5: Run focused tests on both TFMs**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --filter BootstrapTextBoxEditorTests --blame-hang --blame-hang-timeout 5m
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapTextBoxEditorTests --blame-hang --blame-hang-timeout 5m
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapTextBoxEditor.cs src/MyDmsVn.BootstrapSourceGrid/Editors/Internal/BootstrapSourceGridTextBoxControl.cs tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapTextBoxEditorTests.cs
git commit -m "feat: add BootstrapTextBox SourceGrid editor"
```

---

### Task 3: Lock keyboard, focus, validation, and SourceGrid conversion behavior

**Files:**
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapTextBoxEditorTests.cs`
- Modify if required by a proven integration bug: `src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapTextBoxEditor.cs`

**Interfaces:**
- Produces: compatibility evidence that Bootstrap hosting did not replace SourceGrid semantics.

- [ ] **Step 1: Add keyboard lifecycle tests**

Cover:

```text
EnterCommitsAccordingToSourceGridPolicy
EscapeCancelsAndRestoresOriginalValue
TabCommitsAndMovesToNextEditableCell
ShiftTabCommitsAndMovesToPreviousEditableCell
ArrowNavigationAfterCommitMatchesSourceGrid
```

Derive expected movement from existing SourceGrid compatibility tests rather than hard-coding a new navigation policy.

- [ ] **Step 2: Add typed conversion tests**

Use the same BootstrapTextBox editor adapter with a numeric declared cell type. Enter a valid numeric string and assert SourceGrid stores the typed numeric value. Enter invalid text and assert the existing SourceGrid validation/conversion path rejects it without the adapter converting it manually.

- [ ] **Step 3: Add `Control.Validated` regression coverage**

Move focus away using a deterministic test host and assert SourceGrid ends/commits the edit exactly once. Verify no duplicate commit event is introduced by the Bootstrap composite control.

- [ ] **Step 4: Run focused tests**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --filter BootstrapTextBoxEditorTests --blame-hang --blame-hang-timeout 5m
```

Expected: PASS on both TFMs.

- [ ] **Step 5: Commit**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapTextBoxEditor.cs tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapTextBoxEditorTests.cs
git commit -m "test: lock BootstrapTextBox edit compatibility"
```

---

### Task 4: Lock theme, enabled/read-only, sizing, and disposal behavior

**Files:**
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapTextBoxEditorTests.cs`
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapEditorSizingTests.cs`
- Modify if required: `src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapTextBoxEditor.cs`

- [ ] **Step 1: Add runtime theme-switch test during active edit**

Start edit, switch Light -> Dark, and assert the Bootstrap control follows BootstrapThemeManager while `BootstrapSourceGridEditorStyler` does not force the cell View font/background/foreground into the control.

- [ ] **Step 2: Add read-only/enabled-state tests**

Verify consumer configuration of `BootstrapControl.ReadOnly` and control/grid enabled state does not get overwritten at edit start. SourceGrid cell editability still decides whether editing starts at all.

- [ ] **Step 3: Add compact-row sizing test**

Assign the editor to a row shorter than BootstrapTextBox preferred height. Assert no automatic row-height mutation occurs and the control remains bounded by SourceGrid placement rules.

- [ ] **Step 4: Add disposal test**

Create the editor through the internal registry seam, never start editing, dispose the grid, and assert the Bootstrap control is disposed. Repeat after the editor has been attached/used.

- [ ] **Step 5: Run tests and full build**

```powershell
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --filter "BootstrapTextBoxEditorTests|BootstrapEditorSizingTests|BootstrapEditorOwnershipTests" --blame-hang --blame-hang-timeout 5m
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid tests/MyDmsVn.BootstrapSourceGrid.Tests
git commit -m "test: harden BootstrapTextBox editor lifecycle"
```

---

### Task 5: Stage gate

- [ ] **Step 1: Run the complete repository validation**

```powershell
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Expected: commands exit `0`; vendor status commands are empty.

- [ ] **Step 2: Confirm the adapter remains thin**

Review the diff and reject any code that reimplements SourceGrid navigation, validation, final type conversion, popup management, or a second theme system.
