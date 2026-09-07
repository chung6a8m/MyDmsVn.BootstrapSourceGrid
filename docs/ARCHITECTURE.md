# Architecture

## 1. Overview

`MyDmsVn.BootstrapSourceGrid` is an integration layer, not a new grid engine. The core control derives from `SourceGrid.Grid` and maps Bootstrap5WinFormUI theme semantics into SourceGrid's visual extension points.

```text
Application
    |
    v
BootstrapSourceGrid
    |
    +------------------------------+
    |                              |
    v                              v
Bootstrap5WinFormUI                SourceGrid
Theme/Rendering/DPI                Grid/Cells/Views/Editors/Selection
```

## 2. Inheritance model

The intended inheritance chain is:

```text
System.Windows.Forms.Panel
    -> SourceGrid.CustomScrollControl
    -> SourceGrid.GridVirtual
    -> SourceGrid.Grid
    -> MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid
```

This preserves the complete concrete SourceGrid API. The integration should avoid introducing a container control around `SourceGrid.Grid` because that would require forwarding a large API surface and would complicate event/focus/designer behavior.

## 3. Ownership boundaries

### 3.1 SourceGrid owns behavior

SourceGrid remains authoritative for:

- concrete cell storage and `GridVirtual` foundation;
- row/column collections;
- positions, ranges, spans;
- selection and active position;
- keyboard and mouse behavior;
- controllers and event dispatch;
- editor lifecycle;
- scrolling and scrollbar ownership;
- painting orchestration;
- Model/View/Editor/Controller cell composition.

BootstrapSourceGrid must not duplicate or replace these subsystems in MVP.

### 3.2 Bootstrap5WinFormUI owns design semantics

Bootstrap5WinFormUI remains authoritative for:

- current theme selection;
- semantic colors;
- theme metrics;
- typography tokens;
- DPI scaling helpers;
- Bootstrap visual conventions;
- runtime `ThemeChanged` notifications.

The integration consumes these APIs. It does not introduce an independent theme service.

### 3.3 BootstrapSourceGrid owns translation

The integration owns:

- converting Bootstrap theme tokens into SourceGrid visual state;
- applying default SourceGrid Views/styles to integration-created/default cells and headers;
- keeping those defaults synchronized when the theme changes;
- respecting consumer overrides;
- Bootstrap-owned font lifecycle;
- integration-specific tests, demo, docs, and package metadata.

## 4. Proposed project topology

```text
MyDmsVn.BootstrapSourceGrid.sln
|
+-- src/
|   `-- MyDmsVn.BootstrapSourceGrid/
|       +-- MyDmsVn.BootstrapSourceGrid.csproj
|       +-- Controls/
|       |   `-- BootstrapSourceGrid.cs
|       +-- Theming/
|       |   +-- BootstrapSourceGridThemeAdapter.cs
|       |   +-- BootstrapSourceGridThemeSnapshot.cs
|       |   `-- BootstrapSourceGridThemeFont.cs
|       +-- Views/
|       |   +-- BootstrapSourceGridCellView.cs
|       |   +-- BootstrapSourceGridColumnHeaderView.cs
|       |   `-- BootstrapSourceGridRowHeaderView.cs
|       +-- Editors/
|       |   `-- BootstrapSourceGridEditorStyler.cs
|       `-- Internal/
|           +-- BootstrapSourceGridStyleApplicator.cs
|           `-- BootstrapSourceGridDpiMetrics.cs
|
+-- tests/
|   `-- MyDmsVn.BootstrapSourceGrid.Tests/
|
+-- samples/
|   `-- MyDmsVn.BootstrapSourceGrid.Demo/
|
+-- vendor/
|   +-- Bootstrap5WinFormUI/  (git submodule)
|   `-- sourcegrid/           (git submodule)
|
`-- docs/
```

Names are implementation targets for the plans. If exact upstream type contracts prove a proposed helper unnecessary, remove the helper rather than creating an empty abstraction.

## 5. Theme translation model

The adapter should read one coherent snapshot from the current Bootstrap theme:

```text
BootstrapTheme
    |
    +-- Colors
    |   +-- Surface
    |   +-- SurfaceSecondary
    |   +-- Text
    |   +-- MutedText
    |   +-- Border
    |   `-- Primary
    |
    +-- Typography.Body
    `-- Metrics
```

and expose integration-ready values without leaking mutable theme reads throughout cell painting.

Conceptually:

```csharp
internal sealed class BootstrapSourceGridThemeSnapshot
{
    public Color CellBackColor { get; }
    public Color AlternateCellBackColor { get; }
    public Color CellForeColor { get; }
    public Color HeaderBackColor { get; }
    public Color HeaderForeColor { get; }
    public Color BorderColor { get; }
    public Color SelectionBackColor { get; }
    public Color SelectionForeColor { get; }
    public BootstrapFontToken BodyFont { get; }
}
```

The exact selection text color should use the same contrast logic already present in Bootstrap5WinFormUI where possible rather than duplicating color heuristics.

## 6. View strategy

SourceGrid separates View from Model, Editor, and Controller. Therefore the preferred integration point is SourceGrid Views/VisualModels.

Rules:

1. Reuse SourceGrid view classes through inheritance/composition when their API supports it.
2. Keep view instances shareable where SourceGrid allows safe sharing.
3. Do not store cell-specific mutable state in globally shared Views.
4. Consumer-assigned Views are authoritative and must not be overwritten during theme refresh unless they are integration-owned.
5. Theme updates mutate/replace only integration-owned visual objects and then invalidate the grid.

## 7. Default style application

`BootstrapSourceGrid` should establish integration defaults during construction without requiring application bootstrap.

Conceptual flow:

```text
constructor
  -> capture current theme
  -> create integration-owned Views/styles
  -> apply Bootstrap font if consumer has not overridden Font
  -> subscribe ThemeChanged
  -> establish accessibility defaults
```

When cells/headers are created or when the grid is redimensioned, integration defaults must be applied only where doing so does not erase explicit consumer customization.

Because SourceGrid allows consumers to assign Views directly, the implementation must define ownership detection clearly. The recommended rule is identity-based: only Views created and tracked by this integration are automatically replaced/refreshed.

## 8. Runtime theme update flow

```text
BootstrapThemeManager.ThemeChanged
        |
        v
BootstrapSourceGrid.OnThemeChanged
        |
        +--> capture new ThemeSnapshot
        +--> update/recreate integration-owned Views
        +--> update Bootstrap-owned Font if still in theme-font mode
        +--> update editor style bridge state
        +--> recalculate Bootstrap-owned DPI metrics if needed
        `--> Invalidate / Refresh layout only as required
```

Must not:

- recreate the grid;
- clear cells;
- reset selection;
- change editing/navigation semantics;
- reset consumer-assigned Views or Fonts.

## 9. Font lifecycle

Bootstrap5WinFormUI's existing controls provide the pattern:

- control begins in theme-font mode;
- it creates a font from `Typography.Body` and owns that instance;
- theme changes may replace the owned font;
- if consumer code assigns `Font`, the control exits theme-font mode;
- consumer-owned fonts are never disposed by the control;
- owned font is disposed in `Dispose(bool)`.

BootstrapSourceGrid should follow the same model.

## 10. DPI model

SourceGrid owns its own layout/scaling behavior. BootstrapSourceGrid should scale only metrics introduced by the integration.

Examples of Bootstrap-owned metrics:

- additional padding used by integration Views;
- focus/selection visual thickness if newly introduced;
- editor border/padding adjustments introduced by the integration.

Use `DpiScaler` with `DeviceDpi` when available. Avoid double-scaling SourceGrid-owned dimensions such as row heights or column widths unless SourceGrid explicitly delegates those values to the View.

## 11. Editor integration

MVP editor strategy is conservative.

```text
SourceGrid Editor lifecycle remains unchanged
            |
            v
BootstrapSourceGridEditorStyler
   applies safe appearance alignment only
```

The styler may set properties on editor controls when SourceGrid exposes them safely, for example font, foreground/background, selection color, or border-related properties. It must not replace commit/cancel/navigation behavior.

Any editor type that cannot be styled without behavioral risk remains native for MVP and is documented as such.

## 12. Selection and focus

Selection behavior remains SourceGrid-owned. The integration changes only visual representation.

Requirements:

- selected cells remain distinguishable in light and dark themes;
- focused/active position remains discoverable;
- selection foreground has adequate contrast;
- keyboard navigation tests prove styling did not change active-position semantics;
- theme changes while selection exists preserve selected ranges and active position.

## 13. Scrollbars

SourceGrid's `CustomScrollControl` owns horizontal/vertical scrollbars and layout. MVP leaves this system intact.

The integration may align surrounding grid surface/background so native scrollbars do not appear visually broken, but it must not replace scrollbar classes or scrolling mechanics.

## 14. Designer lifecycle

Constructor and property getters/setters must be safe when:

- no application theme initialization has run;
- no handle exists;
- `Site?.DesignMode` behavior is inconsistent during nested construction;
- the Designer serializes public properties;
- the control is repeatedly created/disposed by the designer host.

Avoid runtime-only services in constructors.

## 15. Error handling

Theme adaptation should fail only for genuine programming/configuration errors. Normal control construction must not throw because optional runtime state is unavailable.

Vendor/API mismatch should be caught at compile time by project references and tests rather than through reflection-based late binding.

Do not swallow SourceGrid exceptions or replace its error semantics with Bootstrap-specific exceptions unless a future requirement explicitly introduces such behavior.

## 16. Performance principles

- No grid-data rebuild on theme changes.
- No full collection walk on every paint if the same result can be applied once per theme change.
- Prefer shared views/styles over one new GDI-heavy object per cell when safe.
- Avoid allocating `Font`, `Pen`, `Brush`, or helper objects in hot cell painting paths.
- Measure before introducing caching that complicates ownership/lifecycle.

## 17. Packaging boundary

The integration ships as its own assembly/package. It does not merge vendor binaries or source into one assembly.

During development the vendor projects are referenced from pinned submodules. Release packaging must ensure package dependency strategy is explicit and reproducible; if package references replace project references, both TFMs and API compatibility must be revalidated first.
