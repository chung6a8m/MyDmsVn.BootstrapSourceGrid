# BootstrapLookupBox Editor Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Integrate `BootstrapLookupBox` as a SourceGrid editor using `SelectedValue` as the logical cell value while preserving lookup popup behavior and SourceGrid commit/cancel/navigation semantics.

**Architecture:** `BootstrapLookupBoxEditor` is a thin `EditorControlBase` adapter. The lookup control owns search, result grid, popup, highlight, pending text, unmatched-text policy, display text, and the first-character-to-pending-text transition; SourceGrid owns the cell edit session and final validation/conversion. The main engineering risk is focus/popup event ordering around SourceGrid's `Control.Validated` auto-commit path.

**Tech Stack:** C#, WinForms, SourceGrid 5.0, `BootstrapLookupBox`, NUnit, `net48`, `net8.0-windows`.

**Spec:** `docs/EDITOR_REPLACEMENT.md` sections 8, 11–14.

## Global Constraints

- `UseCellViewProperties = false`.
- `SelectedValue` is the logical edit value.
- `Text`/display text is presentation and must not be committed as logical value when `ValueMember` is used.
- Lookup configuration exists before `SetEditValue` initializes selection.
- Every concrete `EditorControlBase` adapter implements `OnSendCharToEditor(char)` from its first compilable version.
- Do not reimplement lookup search/popup/highlight logic in this repository.
- SourceGrid owns the cell edit session, final validation/conversion, and grid navigation.
- Every popup/focus GUI test is STA, bounded, and non-modal.
- Do not modify the Bootstrap vendor unless a standalone vendor bug is proven and separately approved.

---

### Task 1: Implement the SelectedValue and first-character adapter contract

**Files:**
- Create: `src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapLookupBoxEditor.cs`
- Create only if the pinned lookup needs a protected text/caret insertion seam: `src/MyDmsVn.BootstrapSourceGrid/Editors/Internal/BootstrapSourceGridLookupBoxControl.cs`
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapLookupBoxEditorTests.cs`

**Interfaces:**
- Produces: `BootstrapLookupBoxEditor`, typed `BootstrapControl` property, and a compile-complete SourceGrid first-character override.
- Consumed later by: public editor registry.

- [ ] **Step 1: Write failing logical-value and first-character tests**

Create a small in-memory lookup source with separate identifier and display fields and cover:

```text
SetEditValueSelectsBySelectedValue
GetEditedValueReturnsSelectedValue
DisplayTextIsNotLogicalValue
CancelRestoresOriginalSelectedValue
CommitPassesSelectedValueThroughSourceGridConversion
SendFirstCharacterCreatesLookupPendingText
```

Use deliberately different values such as ID `42` and display text `Northwind` so misuse of `Text` is obvious. For the first-character test, initialize a committed lookup value, start edit by typing a different character through SourceGrid's `SendCharToEditor` path, and assert the lookup enters its normal pending-text/search flow rather than committing display text or leaving the old selection text untouched.

- [ ] **Step 2: Implement the adapter skeleton, including the abstract first-character seam**

Target shape:

```csharp
public sealed class BootstrapLookupBoxEditor : SourceGrid.Cells.Editors.EditorControlBase
{
    internal BootstrapLookupBoxEditor(Type valueType)
        : base(valueType)
    {
        UseCellViewProperties = false;
    }

    public BootstrapLookupBox BootstrapControl =>
        (BootstrapLookupBox)Control;

    protected override Control CreateControl()
        => new BootstrapLookupBox();

    protected override void OnSendCharToEditor(char key)
    {
        // Forward through the exact lookup text-entry seam verified in Stage 0.
        // The key must enter BootstrapLookupBox's normal pending-text/search pipeline.
    }
}
```

The target shape is intentionally explicit that `OnSendCharToEditor(char)` exists in Task 1; replace the comment body with the real verified text-entry call before the Task 1 build/test checkpoint. If direct `Text` assignment does not preserve the required inner-editor caret/selection behavior, create the smallest internal lookup subclass seam in this task. Do not use a no-op override merely to satisfy the compiler and do not bypass `BootstrapLookupBox` search/pending-text state.

- [ ] **Step 3: Implement value initialization**

`SetEditValue` assigns the incoming logical value to `BootstrapControl.SelectedValue`. Do not assign the cell value directly to `Text`.

- [ ] **Step 4: Implement value retrieval**

`GetEditedValue` returns `BootstrapControl.SelectedValue` and lets SourceGrid perform final conversion/validation.

- [ ] **Step 5: Implement first-character forwarding**

Use the exact lookup text-entry/caret seam verified against the pinned control. Starting edit by typing must replace the visible committed edit text in the same way normal lookup typing does, mark lookup text pending, and allow the lookup's own debounce/search/popup policy to react. It must not select a result, commit `SelectedValue`, or invoke SourceGrid navigation directly.

- [ ] **Step 6: Run focused tests on both TFMs**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --filter BootstrapLookupBoxEditorTests --blame-hang --blame-hang-timeout 5m
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapLookupBoxEditorTests --blame-hang --blame-hang-timeout 5m
```

Expected: PASS for logical-value and first-character scenarios.

- [ ] **Step 7: Commit**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapLookupBoxEditor.cs src/MyDmsVn.BootstrapSourceGrid/Editors/Internal tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapLookupBoxEditorTests.cs
git commit -m "feat: add BootstrapLookupBox SourceGrid editor"
```

---

### Task 2: Prove popup focus does not accidentally commit through `Validated`

**Files:**
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapLookupBoxInteractionTests.cs`
- Modify if a proven integration fix is required: `src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapLookupBoxEditor.cs`

**Interfaces:**
- Produces: the event-ordering contract between SourceGrid edit focus and BootstrapLookupBox popup focus behavior.

- [ ] **Step 1: Build a deterministic WinForms interaction host**

Use the existing repository WinForms test guard/host pattern. Host one `BootstrapSourceGrid` on a Form, create a lookup cell plus another focusable cell/control, create handles explicitly, and process only bounded message-loop synchronization points.

- [ ] **Step 2: Add popup-open regression test**

Start SourceGrid edit and open the lookup popup. Assert opening the popup/search result surface does not end the SourceGrid cell edit merely because child/popup focus changes.

- [ ] **Step 3: Add result-grid focus regression test**

Exercise the lookup's result navigation/focus path and assert its built-in focus restoration does not trigger a duplicate SourceGrid commit.

- [ ] **Step 4: Add popup-close-without-selection test**

Open then close/cancel the lookup popup without selecting a result. Assert the SourceGrid edit remains active or cancels according to the explicit Escape policy established in Task 3; it must never commit an unintended highlighted value.

- [ ] **Step 5: If SourceGrid `Validated` fires too early, fix only the adapter boundary**

Prefer a narrow suppression/deferral mechanism in `BootstrapLookupBoxEditor` tied to the lookup's own popup/focus state. Do not modify `BootstrapLookupDropDownController` unless a standalone lookup control test proves the vendor itself is wrong outside SourceGrid.

- [ ] **Step 6: Run interaction tests with hang diagnostics**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --filter BootstrapLookupBoxInteractionTests --blame-hang --blame-hang-timeout 5m
```

Expected: PASS on both TFMs without modal UI.

- [ ] **Step 7: Commit**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapLookupBoxEditor.cs tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapLookupBoxInteractionTests.cs
git commit -m "fix: integrate lookup popup with SourceGrid edit focus"
```

---

### Task 3: Lock keyboard precedence and commit/cancel behavior

**Files:**
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapLookupBoxInteractionTests.cs`
- Modify if required: `src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapLookupBoxEditor.cs`

**Interfaces:**
- Produces: explicit keyboard semantics instead of relying on accidental event order.

- [ ] **Step 1: Lock Enter behavior**

Cover both states:

```text
popup closed -> SourceGrid normal commit policy
popup open with highlighted result -> lookup commits highlighted result first, then SourceGrid stores SelectedValue exactly once
```

- [ ] **Step 2: Lock Escape behavior**

Use this two-level policy:

```text
popup open -> first Escape invokes lookup pending-edit/popup cancellation and keeps the SourceGrid cell edit active
popup closed -> Escape uses SourceGrid cancel and restores the original cell value
```

If the pinned BootstrapLookupBox already consumes Escape exactly this way, adapt without duplicating it; test the observable contract.

- [ ] **Step 3: Lock Tab and Shift+Tab**

With a selected/valid lookup value, Tab commits once and moves to the next editable SourceGrid cell; Shift+Tab moves to the previous editable cell. Popup state must be resolved before grid navigation.

- [ ] **Step 4: Lock result navigation keys**

While popup is open, Up/Down/PageUp/PageDown affect lookup highlight/result navigation rather than SourceGrid active-cell navigation. After popup closes/commit completes, SourceGrid navigation resumes normally.

- [ ] **Step 5: Run focused tests on both TFMs**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --filter BootstrapLookupBoxInteractionTests --blame-hang --blame-hang-timeout 5m
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapLookupBoxEditor.cs tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapLookupBoxInteractionTests.cs
git commit -m "test: lock lookup editor keyboard semantics"
```

---

### Task 4: Lock mouse, outside-click, deactivation, and unmatched-text behavior

**Files:**
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapLookupBoxInteractionTests.cs`
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapLookupBoxEditorTests.cs`
- Modify if required: `src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapLookupBoxEditor.cs`

- [ ] **Step 1: Add mouse selection test**

Open popup, click a result through the lookup's supported test seam, assert `SelectedValue` is committed to the SourceGrid cell exactly once and focus/navigation remains valid.

- [ ] **Step 2: Add outside-click test**

Click outside the lookup/grid while editing. Assert popup closes according to lookup policy and SourceGrid commit/cancel occurs exactly once according to its normal focus-validation contract.

- [ ] **Step 3: Add application deactivation/Alt+Tab-equivalent test**

Drive the closest deterministic deactivation seam available to the pinned lookup controller/Form without actually automating the OS task switcher. Assert the popup does not remain incorrectly topmost and the SourceGrid edit session is left in a defined state without hanging.

- [ ] **Step 4: Add unmatched-text tests**

For each supported unmatched-text policy used by the project, type text that has no matching item and assert the lookup's policy decides selection/pending state while SourceGrid only stores the resulting `SelectedValue` if the edit is valid.

- [ ] **Step 5: Add validation-failure test**

Use a SourceGrid declared value/validator that rejects the resulting lookup value. Assert lookup UI remains coherent and the adapter does not bypass SourceGrid validation.

- [ ] **Step 6: Run focused tests**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --filter "BootstrapLookupBoxEditorTests|BootstrapLookupBoxInteractionTests" --blame-hang --blame-hang-timeout 5m
```

Expected: PASS on both TFMs.

- [ ] **Step 7: Commit**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapLookupBoxEditor.cs tests/MyDmsVn.BootstrapSourceGrid.Tests
git commit -m "test: harden lookup editor focus and selection"
```

---

### Task 5: Theme, sizing, sharing, and disposal hardening

**Files:**
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapLookupBoxInteractionTests.cs`
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapEditorSizingTests.cs`
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapEditorOwnershipTests.cs`

- [ ] **Step 1: Theme switch while popup is open**

Open the popup, switch Light <-> Dark, and assert both editor and popup remain usable and themed without SourceGrid View-property injection or cell edit reset.

- [ ] **Step 2: Same-grid shared adapter test**

Configure one lookup editor once and assign it to multiple cells. Edit/commit several cells sequentially and assert selection state is initialized from each cell rather than leaking the previous cell's value.

- [ ] **Step 3: Compact-row/preferred-size test**

Assert SourceGrid row height remains consumer-owned even when BootstrapLookupBox prefers a taller control.

- [ ] **Step 4: Disposal with popup/control created**

Dispose the grid after creating/using the lookup editor and after opening/closing its popup. Assert the BootstrapLookupBox and integration-owned registry references are released/disposed deterministically; no message filter/popup survives the test host.

- [ ] **Step 5: Run focused tests and build**

```powershell
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --filter "BootstrapLookupBox|BootstrapEditorSizingTests|BootstrapEditorOwnershipTests" --blame-hang --blame-hang-timeout 5m
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add tests/MyDmsVn.BootstrapSourceGrid.Tests src/MyDmsVn.BootstrapSourceGrid/Editors
git commit -m "test: harden lookup editor lifecycle"
```

---

### Task 6: Stage gate

- [ ] **Step 1: Run full validation**

```powershell
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Expected: all commands exit `0`; vendor worktrees are clean.

- [ ] **Step 2: Review event ordering**

Reject any solution that makes lookup integration work by globally disabling SourceGrid validation/focus behavior, patching the SourceGrid editor engine, or forking BootstrapLookupBox popup logic. The fix must remain local to the adapter boundary.
