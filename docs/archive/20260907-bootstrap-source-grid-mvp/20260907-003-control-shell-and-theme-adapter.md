# Control Shell and Theme Adapter Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the public `BootstrapSourceGrid : SourceGrid.Grid` shell and a deterministic Bootstrap theme adapter with safe runtime theme subscription, font ownership, and integration-owned DPI metrics.

**Architecture:** The control remains behaviorally SourceGrid. It reads one immutable integration snapshot from `BootstrapThemeManager.CurrentTheme`; later stages consume that snapshot to build SourceGrid Views. Runtime theme changes replace only integration-owned state and invalidate the control. The control follows the theme-font ownership pattern already used by `BootstrapDataGridView`.

**Tech Stack:** C#, Windows Forms, SourceGrid 5.0, MyDmsVn.Bootstrap5WinFormUI Theme/Rendering APIs, NUnit, `net48`, `net8.0-windows`.

**Spec:** `docs/PRD.md` FR-01/FR-02/FR-03/FR-08/FR-09/FR-10; `docs/ARCHITECTURE.md`; decisions D-001/D-004/D-006/D-012/D-013.

## Verified upstream APIs at pinned commits

Bootstrap baseline `95077df...` exposes:

```csharp
BootstrapThemeManager.CurrentTheme // get/set, safe default Light theme
BootstrapThemeManager.ThemeChanged
BootstrapTheme.CreateDefault(BootstrapThemeMode.Light | Dark)
BootstrapTheme.Colors
BootstrapTheme.Metrics
BootstrapTheme.Typography
ColorUtil.GetContrastingTextColor(...)
DpiScaler
```

Relevant color tokens are `Primary`, `Secondary`, `Light`, `Dark`, `Body`, `Surface`, `SurfaceSecondary`, `Border`, `Text`, `MutedText`, `Disabled`, `Focus`, `Hover`, and `Active`.

SourceGrid baseline `f4e457b...` exposes `SourceGrid.Grid` as the concrete grid control. This stage must not change its cell/view/editor behavior.

---

### Task 1: Lock public identity with failing tests

**Files:**
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridConstructionTests.cs`
- Create later in task: `src/MyDmsVn.BootstrapSourceGrid/Controls/BootstrapSourceGrid.cs`

**Interfaces:**
- Produces the compatibility contract that the primary public type is a direct SourceGrid subclass with the required full name.

- [ ] **Step 1: Add the failing public identity tests**

Create `BootstrapSourceGridConstructionTests.cs`:

```csharp
using System.Threading;
using MyDmsVn.Bootstrap5WinFormUI.Controls;
using NUnit.Framework;

namespace MyDmsVn.BootstrapSourceGrid.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class BootstrapSourceGridConstructionTests
{
    [OneTimeSetUp]
    public void ConfigureWinForms()
    {
        WinFormsTestGuard.Configure();
    }

    [Test]
    public void PublicTypeHasRequiredIdentity()
    {
        Assert.That(typeof(BootstrapSourceGrid).FullName,
            Is.EqualTo("MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid"));
        Assert.That(typeof(BootstrapSourceGrid).BaseType, Is.EqualTo(typeof(SourceGrid.Grid)));
    }

    [Test]
    public void CanConstructAndDisposeBeforeHandleCreation()
    {
        using (var grid = new BootstrapSourceGrid())
        {
            Assert.That(grid.IsHandleCreated, Is.False);
        }
    }
}
```

- [ ] **Step 2: Run the focused test and verify the intended compile failure**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapSourceGridConstructionTests
```

Expected: compile failure because `BootstrapSourceGrid` does not exist yet.

- [ ] **Step 3: Implement the minimal public shell**

Create `src/MyDmsVn.BootstrapSourceGrid/Controls/BootstrapSourceGrid.cs`:

```csharp
using System.ComponentModel;

namespace MyDmsVn.Bootstrap5WinFormUI.Controls;

/// <summary>
/// Provides a Bootstrap-themed SourceGrid while preserving SourceGrid grid behavior.
/// </summary>
[ToolboxItem(true)]
public class BootstrapSourceGrid : SourceGrid.Grid
{
    /// <summary>
    /// Initializes a new Bootstrap-themed SourceGrid.
    /// </summary>
    public BootstrapSourceGrid()
    {
        Name = nameof(BootstrapSourceGrid);
    }
}
```

Do not add wrapper properties for SourceGrid APIs.

- [ ] **Step 4: Run identity/construction tests on both TFMs**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --filter BootstrapSourceGridConstructionTests
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapSourceGridConstructionTests
```

Expected: PASS.

- [ ] **Step 5: Commit the public shell**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid/Controls/BootstrapSourceGrid.cs tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridConstructionTests.cs
git commit -m "feat: add BootstrapSourceGrid control shell"
```

---

### Task 2: Add deterministic theme snapshot and adapter

**Files:**
- Create: `src/MyDmsVn.BootstrapSourceGrid/Theming/BootstrapSourceGridThemeSnapshot.cs`
- Create: `src/MyDmsVn.BootstrapSourceGrid/Theming/BootstrapSourceGridThemeAdapter.cs`
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridThemeAdapterTests.cs`

**Interfaces:**
- Consumes `BootstrapTheme`.
- Produces immutable integration color/font token values used by later View factories.

- [ ] **Step 1: Write failing light/dark mapping tests**

Test the adapter against framework-created themes rather than hard-coded desktop colors:

```csharp
using MyDmsVn.Bootstrap5WinFormUI.Theme;
using NUnit.Framework;

namespace MyDmsVn.BootstrapSourceGrid.Tests;

[TestFixture]
public sealed class BootstrapSourceGridThemeAdapterTests
{
    [TestCase(BootstrapThemeMode.Light)]
    [TestCase(BootstrapThemeMode.Dark)]
    public void MapsFrameworkSemanticTokens(BootstrapThemeMode mode)
    {
        var theme = BootstrapTheme.CreateDefault(mode);

        var snapshot = BootstrapSourceGridThemeAdapter.CreateSnapshot(theme);

        Assert.Multiple(() =>
        {
            Assert.That(snapshot.CellBackColor, Is.EqualTo(theme.Colors.Surface));
            Assert.That(snapshot.AlternateCellBackColor, Is.EqualTo(theme.Colors.SurfaceSecondary));
            Assert.That(snapshot.CellForeColor, Is.EqualTo(theme.Colors.Text));
            Assert.That(snapshot.BorderColor, Is.EqualTo(theme.Colors.Border));
            Assert.That(snapshot.SelectionBackColor, Is.EqualTo(theme.Colors.Primary));
            Assert.That(snapshot.FocusColor, Is.EqualTo(theme.Colors.Focus));
            Assert.That(snapshot.DisabledColor, Is.EqualTo(theme.Colors.Disabled));
            Assert.That(snapshot.HoverColor, Is.EqualTo(theme.Colors.Hover));
            Assert.That(snapshot.ActiveColor, Is.EqualTo(theme.Colors.Active));
            Assert.That(snapshot.BodyFont, Is.SameAs(theme.Typography.Body));
        });
    }
}
```

The exact header mapping for MVP is:

```text
HeaderBackColor = SurfaceSecondary
HeaderForeColor = Text
```

Selection foreground must be calculated with the Bootstrap framework's existing contrast helper, not duplicated local luminance logic.

- [ ] **Step 2: Run the tests and verify compile failure**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapSourceGridThemeAdapterTests
```

Expected: compile failure because adapter/snapshot types do not exist.

- [ ] **Step 3: Implement immutable snapshot**

Create `BootstrapSourceGridThemeSnapshot.cs` with internal get-only properties for:

```text
CellBackColor
AlternateCellBackColor
CellForeColor
MutedForeColor
HeaderBackColor
HeaderForeColor
BorderColor
SelectionBackColor
SelectionForeColor
FocusColor
DisabledColor
HoverColor
ActiveColor
BodyFont
```

Use `System.Drawing.Color` and the framework's `BootstrapFontToken`. Do not store a mutable reference back to `BootstrapThemeColors`; snapshot individual values so one apply cycle is coherent.

- [ ] **Step 4: Implement adapter**

Create `BootstrapSourceGridThemeAdapter.cs`:

```csharp
using System;
using MyDmsVn.Bootstrap5WinFormUI.Rendering;
using MyDmsVn.Bootstrap5WinFormUI.Theme;

namespace MyDmsVn.BootstrapSourceGrid.Theming;

internal static class BootstrapSourceGridThemeAdapter
{
    public static BootstrapSourceGridThemeSnapshot CreateSnapshot(BootstrapTheme theme)
    {
        if (theme is null)
        {
            throw new ArgumentNullException(nameof(theme));
        }

        var colors = theme.Colors;
        var selectionForeColor = ColorUtil.GetContrastingTextColor(
            colors.Primary,
            colors.Light,
            colors.Dark);

        return new BootstrapSourceGridThemeSnapshot(
            colors.Surface,
            colors.SurfaceSecondary,
            colors.Text,
            colors.MutedText,
            colors.SurfaceSecondary,
            colors.Text,
            colors.Border,
            colors.Primary,
            selectionForeColor,
            colors.Focus,
            colors.Disabled,
            colors.Hover,
            colors.Active,
            theme.Typography.Body);
    }
}
```

Keep constructor/property order explicit and readable; named factory methods are acceptable if they reduce argument-order risk.

- [ ] **Step 5: Run adapter tests on both TFMs**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --filter BootstrapSourceGridThemeAdapterTests
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapSourceGridThemeAdapterTests
```

Expected: PASS.

- [ ] **Step 6: Commit theme adapter**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid/Theming tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridThemeAdapterTests.cs
git commit -m "feat: add Bootstrap SourceGrid theme adapter"
```

---

### Task 3: Add theme-font ownership lifecycle

**Files:**
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Controls/BootstrapSourceGrid.cs`
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridFontTests.cs`

**Interfaces:**
- Consumes `BootstrapThemeManager.CurrentTheme.Typography.Body`.
- Produces a theme-owned `Font` only while the consumer has not explicitly overridden `Font`.

- [ ] **Step 1: Add failing default-font and theme-switch tests**

Every test that mutates global `BootstrapThemeManager.CurrentTheme` must save and restore the original theme in `try/finally` so tests do not leak global state.

Minimum tests:

```text
DefaultConstructionUsesCurrentThemeBodyFont
ThemeSwitchReplacesThemeOwnedFont
ConsumerAssignedFontSurvivesThemeSwitch
DisposeDoesNotDisposeConsumerFont
```

Use:

```csharp
var original = BootstrapThemeManager.CurrentTheme;
try
{
    BootstrapThemeManager.CurrentTheme = BootstrapTheme.CreateDefault(BootstrapThemeMode.Light);
    // arrange/act/assert
}
finally
{
    BootstrapThemeManager.CurrentTheme = original;
}
```

For the consumer-font test, assert `ReferenceEquals(grid.Font, consumerFont)` after switching Light -> Dark.

- [ ] **Step 2: Run and verify failures**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapSourceGridFontTests
```

Expected: failures because theme-font lifecycle is not implemented.

- [ ] **Step 3: Implement font ownership following BootstrapDataGridView's proven pattern**

Add fields to `BootstrapSourceGrid`:

```csharp
private bool _initialized;
private bool _settingThemeFont;
private bool _useThemeFont = true;
private Font? _themeFont;
```

Constructor sequence:

```text
1. create current theme snapshot
2. mark initialized at the point OnFontChanged can safely observe fields
3. ApplyThemeFont()
4. later tasks subscribe theme/change visual state
```

Implement `OnFontChanged` so an external assignment exits theme-font mode:

```csharp
protected override void OnFontChanged(EventArgs e)
{
    base.OnFontChanged(e);

    if (!_initialized)
    {
        return;
    }

    if (!_settingThemeFont)
    {
        _useThemeFont = false;
        DisposeThemeFont();
    }

    Invalidate();
}
```

Implement `ApplyThemeFont()` and `ThemeFontMatches(BootstrapFontToken)` using the exact pattern from the pinned framework's `BootstrapDataGridView`: construct a new `Font(token.FontFamilyName, token.SizeInPoints, token.Style)`, set `_settingThemeFont` around `Font = nextFont`, then dispose only the previous owned instance.

- [ ] **Step 4: Implement owned-font disposal**

```csharp
private void DisposeThemeFont()
{
    var font = _themeFont;
    _themeFont = null;
    font?.Dispose();
}
```

Do not call `Font.Dispose()` because `Font` may be consumer-owned.

- [ ] **Step 5: Run font tests on both TFMs**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --filter BootstrapSourceGridFontTests
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapSourceGridFontTests
```

Expected: PASS.

- [ ] **Step 6: Commit font lifecycle**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid/Controls/BootstrapSourceGrid.cs tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridFontTests.cs
git commit -m "feat: add theme font ownership lifecycle"
```

---

### Task 4: Add runtime ThemeChanged subscription and immutable current snapshot

**Files:**
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Controls/BootstrapSourceGrid.cs`
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridThemeLifecycleTests.cs`
- Modify: `src/MyDmsVn.BootstrapSourceGrid/Properties/AssemblyInfo.cs` if `InternalsVisibleTo` is not already available through project configuration.

**Interfaces:**
- Consumes `BootstrapThemeManager.ThemeChanged`.
- Produces current integration snapshot and a virtual/internal refresh seam for later View application.

- [ ] **Step 1: Expose internals to the test assembly without public test-only API**

Prefer in project file:

```xml
<ItemGroup>
  <InternalsVisibleTo Include="MyDmsVn.BootstrapSourceGrid.Tests" />
</ItemGroup>
```

If the SDK/version used by both TFMs does not support that item form, use:

```csharp
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("MyDmsVn.BootstrapSourceGrid.Tests")]
```

Do not make theme snapshot state public merely for tests.

- [ ] **Step 2: Write failing lifecycle tests**

Minimum cases:

```text
CurrentSnapshotMatchesThemeAtConstruction
ThemeChangedUpdatesSnapshotWithoutCreatingHandle
DisposedGridStopsReactingToThemeChanges
MultipleThemeSwitchesRemainStable
```

For disposed behavior, capture `CurrentThemeSnapshot` before disposal, dispose, change global theme, then assert the disposed object's internal snapshot reference/value did not change. Always restore original global theme.

- [ ] **Step 3: Add fields and subscription**

Conceptual implementation:

```csharp
private bool _themeSubscribed;
private BootstrapSourceGridThemeSnapshot _themeSnapshot;

internal BootstrapSourceGridThemeSnapshot CurrentThemeSnapshot => _themeSnapshot;
```

Constructor:

```csharp
_themeSnapshot = BootstrapSourceGridThemeAdapter.CreateSnapshot(
    BootstrapThemeManager.CurrentTheme);

BootstrapThemeManager.ThemeChanged += OnThemeChanged;
_themeSubscribed = true;
```

`OnThemeChanged`:

```csharp
private void OnThemeChanged(object? sender, BootstrapThemeChangedEventArgs e)
{
    if (IsDisposed)
    {
        return;
    }

    _themeSnapshot = BootstrapSourceGridThemeAdapter.CreateSnapshot(e.NewTheme);

    if (_useThemeFont)
    {
        ApplyThemeFont();
    }

    ApplyBootstrapTheme();
    Invalidate();
}
```

Use the actual event args property names from the pinned `BootstrapThemeChangedEventArgs` source; if they are not `NewTheme`, either use the correct property or read `BootstrapThemeManager.CurrentTheme`. Do not use reflection.

Add an internal/virtually narrow `ApplyBootstrapTheme()` method that Stage 2 can fill; in this stage it should update only control-level background/foreground properties that are safe and must not iterate/rewrite cells.

- [ ] **Step 4: Unsubscribe in Dispose before base disposal completes**

```csharp
protected override void Dispose(bool disposing)
{
    if (disposing)
    {
        if (_themeSubscribed)
        {
            BootstrapThemeManager.ThemeChanged -= OnThemeChanged;
            _themeSubscribed = false;
        }

        DisposeThemeFont();
    }

    base.Dispose(disposing);
}
```

- [ ] **Step 5: Run lifecycle tests on both TFMs**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --filter BootstrapSourceGridThemeLifecycleTests
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapSourceGridThemeLifecycleTests
```

Expected: PASS, no WinForms handle required by test setup.

- [ ] **Step 6: Commit theme lifecycle**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridThemeLifecycleTests.cs
git commit -m "feat: react to Bootstrap runtime theme changes"
```

---

### Task 5: Add Bootstrap-owned DPI metric adapter without double-scaling SourceGrid

**Files:**
- Create: `src/MyDmsVn.BootstrapSourceGrid/Internal/BootstrapSourceGridDpiMetrics.cs`
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridDpiMetricsTests.cs`

**Interfaces:**
- Consumes framework `BootstrapTheme.Metrics` and `DpiScaler`.
- Produces only metrics introduced by this integration.

- [ ] **Step 1: Inspect exact pinned metric token names used for small/default spacing**

Read:

```text
vendor/Bootstrap5WinFormUI/src/MyDmsVn.Bootstrap5WinFormUI/Theme/BootstrapThemeMetrics.cs
vendor/Bootstrap5WinFormUI/src/MyDmsVn.Bootstrap5WinFormUI/Rendering/DpiScaler.cs
```

Record the exact available property names in code comments only where they clarify a non-obvious mapping. Do not invent a metric token.

- [ ] **Step 2: Write failing scale tests**

Test at DPI 96, 120, 144, and 192. Expected values must be computed with the framework's `DpiScaler.Scale(...)`, not duplicated rounding logic.

The MVP adapter should at minimum expose integration cell padding and focus-indicator thickness if Stage 2 needs them. Do not include row height, column width, scrollbar size, or any SourceGrid-owned dimensions.

- [ ] **Step 3: Implement the narrow metric type**

Example shape after exact tokens are verified:

```csharp
internal readonly struct BootstrapSourceGridDpiMetrics
{
    public BootstrapSourceGridDpiMetrics(int cellPadding, int focusThickness)
    {
        CellPadding = cellPadding;
        FocusThickness = focusThickness;
    }

    public int CellPadding { get; }
    public int FocusThickness { get; }

    public static BootstrapSourceGridDpiMetrics FromTheme(BootstrapTheme theme, int dpi)
    {
        // Use exact framework metrics + DpiScaler here.
    }
}
```

If SourceGrid's own `DevAge.Drawing.Padding` cannot safely consume the scaled integer without changing established measurement behavior, defer padding application to Stage 2 and keep this type to the metrics actually proven necessary.

- [ ] **Step 4: Run focused tests on both TFMs**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --filter BootstrapSourceGridDpiMetricsTests
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --filter BootstrapSourceGridDpiMetricsTests
```

- [ ] **Step 5: Commit DPI adapter**

```powershell
git add src/MyDmsVn.BootstrapSourceGrid/Internal/BootstrapSourceGridDpiMetrics.cs tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridDpiMetricsTests.cs
git commit -m "feat: add Bootstrap SourceGrid DPI metrics"
```

---

### Task 6: Stage gate — prove shell changes do not alter common SourceGrid behavior

**Files:**
- Create: `tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridCompatibilityTests.cs`
- Modify docs only if verified behavior differs from canonical docs.

**Interfaces:**
- Consumes implemented control shell.
- Produces first compatibility regression suite.

- [ ] **Step 1: Add common API tests**

In STA tests, verify:

```csharp
using (var grid = new BootstrapSourceGrid())
{
    grid.Redim(4, 3);
    grid[0, 0] = new SourceGrid.Cells.Cell("Northwind");

    Assert.That(grid.RowsCount, Is.EqualTo(4));
    Assert.That(grid.ColumnsCount, Is.EqualTo(3));
    Assert.That(grid[0, 0].Value, Is.EqualTo("Northwind"));

    grid.Rows[0].Height = 32;
    grid.Columns[0].Width = 120;

    Assert.That(grid.Rows[0].Height, Is.EqualTo(32));
    Assert.That(grid.Columns[0].Width, Is.EqualTo(120));
}
```

Add a span smoke case using a concrete `SourceGrid.Cells.Cell`, assign it to a grid position first, then set its `RowSpan`/`ColumnSpan` according to SourceGrid's supported ordering and verify the span remains retrievable from covered positions.

- [ ] **Step 2: Run all Stage 1 tests on both TFMs**

```powershell
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net48 --blame-hang --blame-hang-timeout 5m
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release -f net8.0-windows --blame-hang --blame-hang-timeout 5m
```

Expected: PASS.

- [ ] **Step 3: Build the entire solution**

```powershell
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
```

Expected: exit `0`.

- [ ] **Step 4: Verify no vendor source changes**

```powershell
git -C vendor/Bootstrap5WinFormUI status --short
git -C vendor/sourcegrid status --short
```

Expected: no output.

- [ ] **Step 5: Review public API surface**

Confirm the only new product public type/members are those intentionally required. Theme snapshot, adapter, DPI metrics, and ownership helpers remain internal.

- [ ] **Step 6: Commit compatibility tests**

```powershell
git add tests/MyDmsVn.BootstrapSourceGrid.Tests/BootstrapSourceGridCompatibilityTests.cs
git commit -m "test: lock SourceGrid shell compatibility"
```

## Stage completion criteria

- [ ] Public type has exact namespace/name and direct `SourceGrid.Grid` base type.
- [ ] Construction/disposal works before handle creation.
- [ ] Theme adapter maps semantic tokens for Light and Dark.
- [ ] Theme selection foreground uses framework contrast logic.
- [ ] Automatic runtime `ThemeChanged` handling works.
- [ ] Theme subscription is removed on disposal.
- [ ] Consumer-assigned font wins and is never disposed by the control.
- [ ] Integration-owned theme font is replaced/disposed correctly.
- [ ] DPI helper contains only integration-owned metrics.
- [ ] Common SourceGrid API smoke tests pass on both TFMs.
- [ ] No vendor submodule is dirty.
