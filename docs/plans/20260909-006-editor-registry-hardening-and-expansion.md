# Editor Registry, Hardening, and Expansion Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Stabilize the three proven Bootstrap editor adapters behind a grid-owned public registry, enforce lifetime/cross-grid rules, add demo/documentation, and define the repeatable pattern for future Bootstrap editors.

**Architecture:** `BootstrapSourceGrid` exposes one `BootstrapSourceGridEditorRegistry`. Consumers explicitly create a small number of grid-owned editor adapters, configure their typed Bootstrap controls, and assign the shared editor instances to SourceGrid cells. The registry never replaces SourceGrid's global editor factory and never touches consumer custom editors.

**Tech Stack:** C#, WinForms, SourceGrid 5.0, MyDmsVn.Bootstrap5WinFormUI, NUnit, `net48`, `net8.0-windows`.

**Spec:** `docs/EDITOR_REPLACEMENT.md`, especially sections 6–8 and 14–16.

## Global Constraints

- Public property: `BootstrapSourceGrid.BootstrapEditors`.
- Public create methods: `CreateTextBox(Type)`, `CreateFormattedTextBox(Type)`, `CreateLookupBox(Type)`.
- Returned adapters expose read-only strongly typed `BootstrapControl` properties.
- Adapter constructors remain internal so normal creation flows through the owning grid registry.
- Registry owns adapter/control disposal.
- Cross-grid reuse must fail deterministically.
- No automatic SourceGrid factory replacement and no per-cell default Bootstrap editor creation.
- Consumer custom editors remain authoritative.
- Both TFMs and bounded non-modal GUI test rules apply.

---

### Task 1: Publish the grid-owned registry API

**Files:**
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapSourceGridEditorRegistry.cs`
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Controls/BootstrapSourceGrid.cs`
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapTextBoxEditor.cs`
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapFormattedTextBoxEditor.cs`
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Editors/BootstrapLookupBoxEditor.cs`
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapEditorRegistryTests.cs`

**Interfaces:**
- Produces the supported public creation/ownership surface for all initial Bootstrap editors.

- [ ] **Step 1: Write public API tests**

Assert the exact surface:

```csharp
BootstrapSourceGrid.BootstrapEditors
BootstrapSourceGridEditorRegistry.CreateTextBox(Type)
BootstrapSourceGridEditorRegistry.CreateFormattedTextBox(Type)
BootstrapSourceGridEditorRegistry.CreateLookupBox(Type)
BootstrapTextBoxEditor.BootstrapControl
BootstrapFormattedTextBoxEditor.BootstrapControl
BootstrapLookupBoxEditor.BootstrapControl
```

Also assert adapter constructors are not public.

- [ ] **Step 2: Make the registry public but keep ownership mutation internal**

Target shape:

```csharp
public sealed class BootstrapSourceGridEditorRegistry : IDisposable
{
    private readonly BootstrapSourceGrid _owner;
    private readonly List<EditorControlBase> _ownedEditors = new();

    internal BootstrapSourceGridEditorRegistry(BootstrapSourceGrid owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    public BootstrapTextBoxEditor CreateTextBox(Type valueType)
        => Register(new BootstrapTextBoxEditor(_owner, valueType));

    public BootstrapFormattedTextBoxEditor CreateFormattedTextBox(Type valueType)
        => Register(new BootstrapFormattedTextBoxEditor(_owner, valueType));

    public BootstrapLookupBoxEditor CreateLookupBox(Type valueType)
        => Register(new BootstrapLookupBoxEditor(_owner, valueType));
}
```

Keep `Register` private/internal. Validate null `Type` immediately with `ArgumentNullException`.

- [ ] **Step 3: Expose one read-only registry property from the grid**

```csharp
public BootstrapSourceGridEditorRegistry BootstrapEditors { get; }
```

Initialize it once per grid instance. Do not name the property `Editors`, because SourceGrid editor concepts must remain unshadowed.

- [ ] **Step 4: Add XML documentation**

Document that callers create an editor once per grid/column/configuration and assign that shared instance to multiple cells; the grid owns disposal; the same instance must not be used by another grid.

- [ ] **Step 5: Run public API tests on both TFMs**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --filter BootstrapEditorRegistryTests
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapEditorRegistryTests
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapEditorRegistryTests.cs
git commit -m "feat: expose grid-owned Bootstrap editor registry"
```

---

### Task 2: Enforce cross-grid ownership deterministically

**Files:**
- Create: `src/MyDmsVn.BootstrapSourceGrid/Editors/Internal/BootstrapEditorOwnershipGuard.cs`
- Modify: all three Bootstrap editor adapter files
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapEditorRegistryTests.cs`

**Interfaces:**
- Produces: one documented failure mode for accidental adapter reuse in another grid.

- [ ] **Step 1: Write cross-grid failure tests**

Create `gridA` and `gridB`, create an editor through `gridA.BootstrapEditors`, assign it to a cell in `gridB`, and attempt to start edit. Assert a deterministic `InvalidOperationException` before the control is rebound/reparented to the second grid.

Repeat for TextBox, FormattedTextBox, and LookupBox editors.

- [ ] **Step 2: Implement the ownership guard**

Target behavior:

```csharp
internal static class BootstrapEditorOwnershipGuard
{
    internal static void EnsureOwner(
        BootstrapSourceGrid owner,
        SourceGrid.CellContext cellContext)
    {
        if (!ReferenceEquals(owner, cellContext.Grid))
        {
            throw new InvalidOperationException(
                "A BootstrapSourceGrid editor can only be used by the grid that created it.");
        }
    }
}
```

Use the exact `CellContext` grid property verified in Stage 0. Call the guard at the earliest SourceGrid edit-start override that receives the current cell context, before control attachment/reparenting.

- [ ] **Step 3: Store owner in each adapter**

Pass the owning `BootstrapSourceGrid` through each internal constructor and validate it for null. Do not expose a setter.

- [ ] **Step 4: Run cross-grid and same-grid sharing tests**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --filter "BootstrapEditorRegistryTests|BootstrapEditorOwnershipTests" --blame-hang --blame-hang-timeout 5m
```

Expected: cross-grid attempts fail deterministically; same-grid sequential editing passes.

- [ ] **Step 5: Commit**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid/Editors tests/MyDmsVn.BootstrapSourceGrid.Tests
git commit -m "fix: enforce Bootstrap editor grid ownership"
```

---

### Task 3: Prove large-grid allocation and disposal characteristics

**Files:**
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapEditorRegistryTests.cs`
- Modify: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapEditorOwnershipTests.cs`

- [ ] **Step 1: Add a large-grid allocation test**

Create a grid with at least 10,000 cells. Create one text editor for one column/configuration and assign that same editor to all applicable cells. Assert only one Bootstrap editor control instance was created by the registry for that scenario.

- [ ] **Step 2: Add multiple-configuration test**

Create three explicit registry editors for three different column configurations and assign them across thousands of cells. Assert the registry owns exactly those three editor instances rather than one per cell.

- [ ] **Step 3: Add disposal test after large assignment**

Dispose the grid without opening every editor. Assert all registry-owned controls are disposed exactly once, including unused ones.

- [ ] **Step 4: Run performance/lifetime tests without brittle timing assertions**

Do not assert wall-clock milliseconds. Assert object counts, ownership, and disposal behavior; use manual profiling only as supplementary evidence.

- [ ] **Step 5: Commit**

```powershell
git add tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapEditorRegistryTests.cs tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapEditorOwnershipTests.cs
git commit -m "test: prove shared Bootstrap editor allocation model"
```

---

### Task 4: Add consumer-facing demo scenarios

**Files:**
- Modify: `samples/MyDmsVn.BootstrapSourceGrid.Demo/MainForm.cs`
- Modify as needed for focused demo helpers under the same sample project

**Interfaces:**
- Produces: visual/manual validation for all three shared Bootstrap editor types.

- [ ] **Step 1: Add shared BootstrapTextBox column**

Create one text editor through `grid.BootstrapEditors`, configure placeholder/clear-button behavior through `BootstrapControl`, and assign the same editor to all cells in the demo text column.

- [ ] **Step 2: Add shared formatted column**

Create one formatted editor, configure a representative `FormatMode`/options, and assign it across the formatted-value column. Show both formatted presentation and typed committed values in diagnostics where practical.

- [ ] **Step 3: Add shared lookup column**

Create one lookup editor, configure `DataSource`, `DisplayMember`, `ValueMember`, columns/search behavior, and assign it across lookup cells. Include enough data to exercise search and keyboard navigation.

- [ ] **Step 4: Extend on-form instructions**

Document Enter/Escape/Tab/Shift+Tab, lookup navigation, theme switching, and the fact that editors are shared per column/configuration rather than created per cell.

- [ ] **Step 5: Build demo on both TFMs**

```powershell
dotnet build samples/MyDmsVn.BootstrapSourceGrid.Demo/MyDmsVn.BootstrapSourceGrid.Demo.csproj -c Release -f net48
dotnet build samples/MyDmsVn.BootstrapSourceGrid.Demo/MyDmsVn.BootstrapSourceGrid.Demo.csproj -c Release -f net8.0-windows
```

Expected: both builds exit `0`.

- [ ] **Step 6: Commit**

```powershell
git add samples/MyDmsVn.BootstrapSourceGrid.Demo
git commit -m "demo: showcase shared Bootstrap grid editors"
```

---

### Task 5: Synchronize canonical and consumer documentation

**Files:**
- Modify: `README.md`
- Modify: `AI_CONTEXT.md`
- Modify: `AGENTS.md` only if a durable execution rule changed
- Modify: `docs/ARCHITECTURE.md`
- Modify: `docs/EDITOR_REPLACEMENT.md`
- Modify: `docs/UPSTREAM_API_SEAMS.md`
- Modify: `docs/COMPATIBILITY.md`
- Modify: `docs/TESTING.md`
- Modify: `docs/KNOWN_LIMITATIONS.md`
- Modify: `docs/PACKAGE_README.md` when public package docs should expose these editors

- [ ] **Step 1: Document the public usage pattern**

Include a concise example:

```csharp
var editor = grid.BootstrapEditors.CreateTextBox(typeof(string));
editor.BootstrapControl.PlaceholderText = "Customer name";

for (var row = 1; row < grid.RowsCount; row++)
{
    grid[row, 1].Editor = editor;
}
```

State explicitly that the grid owns disposal and the editor must not be shared with another grid.

- [ ] **Step 2: Document logical-value rules**

Record:

```text
BootstrapTextBox          -> Text
BootstrapFormattedTextBox -> RawValue
BootstrapLookupBox        -> SelectedValue
```

SourceGrid performs final conversion/validation for all three.

- [ ] **Step 3: Document future expansion pattern**

Future Bootstrap editor work such as `BootstrapComboBox`, date/time, or numeric inputs should follow the same sequence:

```text
prove value contract
prove SourceGrid lifecycle/first-key behavior
use grid-owned shared lifetime
lock popup/focus semantics if applicable
add registry creator only after adapter is stable
```

Do not implement those future controls in this stage.

- [ ] **Step 4: Commit docs**

```powershell
git add README.md AGENTS.md AI_CONTEXT.md docs
git commit -m "docs: document Bootstrap editor integration"
```

---

### Task 6: Final initiative gate

- [ ] **Step 1: Run full automated validation**

```powershell
git submodule update --init --recursive
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Expected: build/tests exit `0`; vendor status commands return no output.

- [ ] **Step 2: Run manual interaction matrix**

On the demo, verify Light/Dark, 100/150/200% DPI where available, text/formatted/lookup edit, Enter, Escape, Tab, Shift+Tab, lookup arrows/PageUp/PageDown, mouse selection, outside click, and application deactivation.

- [ ] **Step 3: API review**

Reject any public API that duplicates SourceGrid cell/grid abstractions, exposes inner native text controls, makes adapter ownership ambiguous, or implies global factory replacement.

- [ ] **Step 4: Archive this roadmap only after all Stage 0–4 gates are complete**

Move the completed 20260909 plan set from `docs/plans/` into a dated subdirectory under `docs/archive/`. Keep `docs/plans/` reserved for active work.
