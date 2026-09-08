# Editor and Interaction Hardening Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make representative SourceGrid editors visually coherent with BootstrapSourceGrid while preserving SourceGrid edit/focus/navigation semantics and preventing automated GUI hangs.

**Architecture:** Do not replace SourceGrid editors. `EditorControlBase.OnStartingEdit` already copies `BackColor`, `ForeColor`, and `Font` from the cell View when `UseCellViewProperties == true`; BootstrapSourceGrid relies on that built-in contract. An internal editor styler only refreshes an already-active editor when the Bootstrap theme changes and only when the editor opted into View properties.

**Tech Stack:** SourceGrid `EditorBase`/`EditorControlBase`/`TextBox`, WinForms controls, Bootstrap theme Views, NUnit STA tests.

**Spec:** PRD FR-06/FR-07/FR-11; decisions D-009/D-014/D-015; TESTING layers C/E.

## Verified SourceGrid editor behavior

At `f4e457b...`:

- `EditorBase.UseCellViewProperties` is public and defaults to `true`.
- `EditorControlBase.Control` exposes the actual WinForms editor control.
- `EditorControlBase.OnStartingEdit` copies `cellContext.Cell.View.BackColor`, `.ForeColor`, and `.Font` when `UseCellViewProperties` is true.
- SourceGrid's default text editor creates `DevAgeTextBox` with `BorderStyle.None` and delegates the edit lifecycle to SourceGrid.
- `CellContext.StartEdit()` and `EndEdit(cancel)` are the supported start/commit/cancel surface.

---

### Task 1: Prove built-in editor View propagation before adding integration code

**Files:**
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridEditorTests.cs`

- [ ] **Step 1: Create a reusable STA editor fixture**

Build a `BootstrapSourceGrid` with a concrete editable string cell:

```csharp
var cell = new SourceGrid.Cells.Cell("before", typeof(string));
grid.Redim(1, 1);
grid[0, 0] = cell;
var context = new SourceGrid.CellContext(grid, new SourceGrid.Position(0, 0), cell);
```

Host the grid in a temporary non-modal `Form` only when a handle is required. Do not use `ShowDialog()`.

- [ ] **Step 2: Force Bootstrap View resolution before edit**

Call:

```csharp
grid.GetCell(0, 0);
```

Assert the cell now uses the integration-owned ordinary cell View.

- [ ] **Step 3: Start edit and inspect the SourceGrid editor control**

```csharp
context.StartEdit();
var editor = (SourceGrid.Cells.Editors.EditorControlBase)cell.Editor;
```

Assert:

```text
editor.IsEditing == true
editor.Control.BackColor == cell.View.BackColor
editor.Control.ForeColor == cell.View.ForeColor
editor.UseCellViewProperties == true
```

For Font, accept SourceGrid/WinForms inheritance semantics: compare effective `editor.Control.Font` with `grid.Font`, not raw `cell.View.Font`, because the themed cell View intentionally keeps `Font = null` to inherit the grid font.

- [ ] **Step 4: End edit in `finally`**

Every test that starts editing must end editing or dispose host/control in `finally` so no hidden editor control survives a failed assertion.

- [ ] **Step 5: Run both TFMs**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --filter BootstrapSourceGridEditorTests --blame-hang --blame-hang-timeout 5m
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapSourceGridEditorTests --blame-hang --blame-hang-timeout 5m
```

Expected: baseline propagation works without integration-specific editor classes.

---

### Task 2: Add active-editor theme refresh bridge

**Files:**
- Create: `src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapSourceGridEditorStyler.cs`
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Controls/BootstrapSourceGrid.cs`
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridEditorTests.cs`

**Interfaces:**
- Consumes current active SourceGrid cell/editor.
- Updates only the visible WinForms editor control's appearance on theme change.

- [ ] **Step 1: Add failing active-edit theme-switch test**

Arrange Light theme, start editing, capture editor control, switch to Dark theme, and assert:

```text
same editor instance remains editing
editor value remains unchanged
BackColor/ForeColor/effective Font update to current Bootstrap cell View
selection active position remains same
```

Always restore global theme and end edit in `finally`.

- [ ] **Step 2: Implement a narrow styler**

Conceptual code:

```csharp
internal static class BootstrapSourceGridEditorStyler
{
    public static void RefreshActiveEditor(BootstrapSourceGrid grid)
    {
        var position = grid.Selection.ActivePosition;
        if (position.IsEmpty())
        {
            return;
        }

        var cell = grid.GetCell(position.Row, position.Column);
        var editor = cell?.Editor as SourceGrid.Cells.Editors.EditorControlBase;
        if (editor == null || !editor.IsEditing || !editor.UseCellViewProperties)
        {
            return;
        }

        var control = editor.Control;
        control.BackColor = cell.View.BackColor;
        control.ForeColor = cell.View.ForeColor;
        control.Font = cell.View.Font ?? grid.Font;
    }
}
```

Use exact SourceGrid `Position` API names if `IsEmpty()`/Row/Column differ. Do not use reflection.

- [ ] **Step 3: Respect consumer editor opt-out**

Add test:

```csharp
editor.UseCellViewProperties = false;
editor.Control.BackColor = custom;
```

Switch theme and assert custom color remains unchanged. `UseCellViewProperties == false` is the SourceGrid-native ownership signal; do not invent another public flag.

- [ ] **Step 4: Call styler after shared Views update on theme change**

In `ApplyBootstrapTheme()` order:

```text
update shared Views
update selection style
update theme font
refresh active editor
invalidate grid
```

This ensures editor receives new View colors/font.

- [ ] **Step 5: Run tests and commit**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --filter BootstrapSourceGridEditorTests --blame-hang --blame-hang-timeout 5m
git add src/MyDmsVn.BootstrapSourceGrid/Editors src/MyDmsVn.BootstrapSourceGrid/Controls tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridEditorTests.cs
git commit -m "feat: refresh active SourceGrid editor theme"
```

---

### Task 3: Lock commit/cancel behavior

**Files:**
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridEditLifecycleTests.cs`

- [ ] **Step 1: Test commit path**

Start editing a string cell, set value through the concrete editor control/editor API, then:

```csharp
Assert.That(context.EndEdit(false), Is.True);
```

Assert cell value changed exactly as SourceGrid would change it.

- [ ] **Step 2: Test cancel path**

Start edit, change editor value, then:

```csharp
Assert.That(context.EndEdit(true), Is.True);
```

Assert original cell value is restored.

- [ ] **Step 3: Test theme switch does not implicitly commit/cancel**

Start edit, modify pending editor text, switch theme, assert editor remains in edit state and cell stored value has not changed solely because of theming.

- [ ] **Step 4: Test disabled editor**

Set `cell.Editor.EnableEdit = false`; `StartEdit()` must not enter edit state. Styling must not change that rule.

- [ ] **Step 5: Run and commit**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --filter BootstrapSourceGridEditLifecycleTests --blame-hang --blame-hang-timeout 5m
git add tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridEditLifecycleTests.cs
git commit -m "test: preserve SourceGrid edit lifecycle"
```

---

### Task 4: Lock keyboard/focus/navigation compatibility

**Files:**
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridKeyboardTests.cs`
- Modify production only if a regression caused by integration is found.

- [ ] **Step 1: Build a 5x3 focusable editable grid fixture**

Use concrete cells and let Bootstrap Views resolve. Host in an STA Form where input/focus requires a handle.

- [ ] **Step 2: Test SourceGrid selection focus API before raw key simulation**

Verify `Selection.Focus(new Position(...), resetSelection)` and active position operate correctly after theming and after theme switching.

- [ ] **Step 3: Test representative navigation**

Use SourceGrid's public controller/input seams or bounded WinForms key dispatch used by SourceGrid's own tests. Cover where supported:

```text
Arrow Up/Down/Left/Right
Tab / Shift+Tab
Home / End
PageUp / PageDown
Enter/F2 edit activation if SourceGrid maps them
Esc cancel while editing
```

Do not add a Bootstrap keyboard controller just to make a test easier.

- [ ] **Step 4: Theme switch during focused grid**

Focus a non-first cell, switch theme, assert `Selection.ActivePosition` is unchanged and subsequent navigation continues from that position.

- [ ] **Step 5: Guard all message pumping**

If `Application.DoEvents()` is necessary, call it only a finite documented number of times in a helper such as `PumpMessagesOnce()`. No waiting loops without a deadline.

- [ ] **Step 6: Run with hang diagnostics and commit**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --filter BootstrapSourceGridKeyboardTests --blame-hang --blame-hang-timeout 5m
git add tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridKeyboardTests.cs src/MyDmsVn.BootstrapSourceGrid
git commit -m "test: lock SourceGrid keyboard and focus compatibility"
```

---

### Task 5: Exercise representative editor families without wrapping them

**Files:**
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridEditorTests.cs`
- Modify: `samples/MyDmsVn.BootstrapSourceGrid.Demo/MainForm.cs`
- Create: `docs/verification/20260907-stage-4-editors.md`

- [ ] **Step 1: Inventory factory-supported editors at the pinned SourceGrid commit**

Read `Cells/Editors/Factory.cs` and existing SourceGrid examples/tests. Select at least:

```text
string/TextBox
numeric
DateTime if factory-supported
bool/CheckBox or equivalent editor if factory-supported
one list/drop-down editor if factory-supported
```

Do not create missing editor families in this stage.

- [ ] **Step 2: Add smoke tests for each selected editor**

For each:

```text
StartEdit succeeds
editor control is attached/visible as SourceGrid expects
View properties propagate when UseCellViewProperties=true
EndEdit(false) works
EndEdit(true) works
```

Where a particular editor has inherently OS-native rendering that cannot safely accept Bootstrap border styling, leave that portion native and record it.

- [ ] **Step 3: Add demo editor matrix**

Demo must show each supported representative editor in both light and dark theme with a theme switch control.

- [ ] **Step 4: Record limitations**

Create `docs/verification/20260907-stage-4-editors.md` with a table:

```text
Editor type | View color propagation | Font | Border | Commit | Cancel | Theme switch active | Result/limitation
```

- [ ] **Step 5: Commit editor matrix**

```powershell
git add tests samples docs/verification/20260907-stage-4-editors.md
git commit -m "docs: verify SourceGrid editor integration"
```

---

### Task 6: Verify automated failure paths cannot show default modal UI

**Files:**
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/WinFormsTestGuard.cs`
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/WinFormsTestGuardTests.cs`
- Modify: `docs/TESTING.md` if actual harness behavior differs.

- [ ] **Step 1: Review SourceGrid user/edit exception paths**

Identify paths where editor validation or `Grid.OnUserException` could produce UI in the application host. Tests must attach handlers/guards necessary to fail deterministically.

- [ ] **Step 2: Keep exception mode configured before handles are created**

`Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException)` must be invoked by the test fixture guard before GUI fixtures create controls.

- [ ] **Step 3: Add a bounded negative validation test**

Use an editor validator/value that fails. Assert the test receives a failure/event/exception through SourceGrid's API and exits; no message box/manual confirmation may be required.

- [ ] **Step 4: Run all tests with hang detection**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --blame-hang --blame-hang-timeout 5m
```

Expected: process exits without human interaction.

- [ ] **Step 5: Commit test-safety hardening**

```powershell
git add tests/MyDmsVn.BootstrapSourceGrid.Tests/WinFormsTestGuard.cs tests/MyDmsVn.BootstrapSourceGrid.Tests/WinFormsTestGuardTests.cs docs/TESTING.md
git commit -m "test: harden unattended editor failure paths"
```

---

### Task 7: Full Stage 4 gate

- [ ] **Step 1: Restore/build/test**

```powershell
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
```

- [ ] **Step 2: Manually exercise demo keyboard/editor matrix**

Verify Tab/Shift+Tab, arrows, start edit, commit, cancel, theme switch while editing, focus leave/return, scrolling while selection exists.

- [ ] **Step 3: Verify vendor cleanliness**

```powershell
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Expected: no output.

## Stage completion criteria

- [ ] No SourceGrid editor class has been replaced solely for theming.
- [ ] SourceGrid-native `UseCellViewProperties` drives ownership.
- [ ] Active editor updates safely on runtime theme change.
- [ ] Commit/cancel semantics are unchanged.
- [ ] Keyboard/focus/navigation remain SourceGrid-compatible.
- [ ] Representative editor families are tested and documented.
- [ ] Automated error paths do not wait on modal UI.
- [ ] Both TFMs pass.
- [ ] Native/unsupported editor visuals are documented rather than behaviorally rewritten.
