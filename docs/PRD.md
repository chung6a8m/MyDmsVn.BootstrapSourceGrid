# Product Requirements Document

## 1. Product

`MyDmsVn.BootstrapSourceGrid` provides a Bootstrap-inspired SourceGrid control for native WinForms applications. It combines SourceGrid's grid engine with the visual system of `MyDmsVn.Bootstrap5WinFormUI` without replacing SourceGrid's programming model.

## 2. Problem statement

SourceGrid provides a mature WinForms grid engine with cells, Views, Editors, Controllers, selection, spans, scrolling, and virtual-grid foundations. `MyDmsVn.Bootstrap5WinFormUI` provides a Bootstrap-inspired WinForms design system with semantic colors, typography, DPI helpers, and runtime theme switching.

Applications using both currently lack a SourceGrid control that visually and behaviorally belongs to the Bootstrap5WinFormUI family. Reimplementing a grid would duplicate mature SourceGrid functionality; modifying SourceGrid to depend on Bootstrap would damage vendor independence.

## 3. Goal

Create `BootstrapSourceGrid : SourceGrid.Grid` so existing SourceGrid code can retain its grid APIs while receiving Bootstrap5WinFormUI visual semantics and theme lifecycle.

## 4. Users

Primary users are WinForms developers who:

- already use or plan to use `MyDmsVn.Bootstrap5WinFormUI`;
- need SourceGrid's cell/view/editor/selection capabilities;
- support legacy .NET Framework 4.8 applications and/or modern .NET 8 Windows applications;
- require Designer-friendly native WinForms controls.

## 5. Required public identity

- Assembly/package: `MyDmsVn.BootstrapSourceGrid`
- Main type: `BootstrapSourceGrid`
- Namespace: `MyDmsVn.Bootstrap5WinFormUI.Controls`
- Base type: `SourceGrid.Grid`
- TFMs: `net48;net8.0-windows`

## 6. MVP functional requirements

### FR-01 — Native SourceGrid compatibility

A consumer must be able to use normal SourceGrid APIs through `BootstrapSourceGrid`, including common operations such as `Redim`, assigning `SourceGrid.Cells.Cell`, row/column sizing, selection configuration, editing, and spans.

### FR-02 — Bootstrap theme adoption

The control must derive visual defaults from `BootstrapThemeManager.CurrentTheme` rather than hard-coded Bootstrap colors.

Minimum token categories:

- surface/background;
- text and muted text;
- borders;
- primary/selection color;
- secondary/alternate surface;
- body typography;
- spacing used by Bootstrap-owned integration visuals.

### FR-03 — Runtime theme switching

When `BootstrapThemeManager.ThemeChanged` fires, an existing grid must update Bootstrap-owned visual state and repaint without replacing the grid instance or rebuilding application data.

### FR-04 — Cell visual integration

Provide Bootstrap-aligned defaults for ordinary cells and alternating rows while preserving consumer-specified SourceGrid Views where the consumer intentionally overrides them.

### FR-05 — Header visual integration

Provide Bootstrap-aligned defaults for generic headers, column headers, and row headers using SourceGrid-compatible visual extension points.

### FR-06 — Selection and focus visuals

Selection and active/focused cell visuals must remain clearly distinguishable in supported themes. Styling must not alter SourceGrid selection semantics.

### FR-07 — Read-only and disabled visuals

Where SourceGrid exposes safe visual hooks, read-only and disabled states must remain legible and semantically distinct without changing editability rules.

### FR-08 — Typography

Default grid typography follows `BootstrapThemeManager.CurrentTheme.Typography.Body`. Explicit consumer font assignment overrides automatic theme-font replacement.

### FR-09 — DPI awareness

Bootstrap-owned pixel metrics must scale through the framework's `DpiScaler`. The control must tolerate runtime DPI changes and repaint/layout appropriately.

### FR-10 — WinForms Designer safety

The control must be constructible and renderable in the WinForms Designer without requiring application startup initialization, service registration, or a running message loop beyond normal Designer behavior.

### FR-11 — Editor appearance hardening

Default SourceGrid editors used in MVP demos must remain usable and visually coherent with the themed grid for font/foreground/background/selection/border where the SourceGrid editor API safely permits it. The editor subsystem itself is not replaced.

### FR-12 — Accessibility baseline

The control must preserve SourceGrid/native accessibility behavior and add integration-specific metadata only when it improves semantics without breaking existing automation behavior.

### FR-13 — Demo

Provide a demo application that shows:

- light and dark themes;
- normal and alternating rows;
- row/column headers;
- selection and focus;
- editing;
- read-only cells;
- spans;
- keyboard navigation;
- scrolling;
- runtime theme switching;
- representative DPI scaling.

## 7. Non-functional requirements

### NFR-01 — Compatibility

Both `net48` and `net8.0-windows` are first-class. Shared code is preferred; target-specific code must be isolated and justified.

### NFR-02 — Vendor independence

SourceGrid and Bootstrap5WinFormUI remain independent repositories. No integration change may create cross-vendor dependency.

### NFR-03 — Upstream patch minimization

MVP target is zero vendor source patches. Any unavoidable patch follows `docs/DECISIONS.md` and `docs/UPSTREAM.md`.

### NFR-04 — Resource safety

Owned GDI resources and event subscriptions must be disposed/unsubscribed correctly. Consumer-owned resources must never be disposed by the integration.

### NFR-05 — Performance

The integration must not add avoidable per-cell/per-paint allocations or rebuild grid data on theme changes. Shared immutable visual objects are preferred where SourceGrid allows safe sharing.

### NFR-06 — Automation-safe tests

No automated WinForms test may hang waiting for modal UI. Tests use STA where required and bounded hang diagnostics.

### NFR-07 — Documentation

Public API, architecture, compatibility rules, vendor baselines, test instructions, and release expectations remain documented and synchronized with code.

## 8. Out of scope for MVP

- custom Bootstrap scrollbar implementation;
- wholesale editor replacement with BootstrapTextBox/BootstrapComboBox/etc.;
- new data-binding or ORM layer;
- replacing SourceGrid selection/navigation engines;
- Bootstrap wrappers for SourceGrid rows/columns/cells/ranges;
- a new virtual-grid abstraction beyond SourceGrid's existing `GridVirtual` model;
- cross-platform UI support;
- WebView/HTML/CSS rendering.

## 9. Compatibility success criterion

This style of existing SourceGrid code must remain natural:

```csharp
var grid = new BootstrapSourceGrid();
grid.Redim(10, 4);
grid[0, 0] = new SourceGrid.Cells.Cell("Northwind");
grid.Rows[0].Height = 32;
grid.Columns[0].Width = 200;
grid.Selection.EnableMultiSelection = true;
```

The integration may provide Bootstrap-specific configuration, but it must not require consumers to replace these SourceGrid concepts with integration-specific duplicates.

## 10. MVP acceptance gates

MVP is accepted only when all are true:

1. solution restores/builds for both TFMs;
2. common SourceGrid API compatibility tests pass;
3. light/dark runtime theme switching updates an existing grid correctly;
4. cell/header/selection typography and colors are covered by deterministic tests where possible;
5. handle-based tests are STA and non-interactive;
6. representative editor, keyboard, focus, span, and scrolling scenarios remain functional;
7. Designer construction is manually verified on supported Visual Studio tooling;
8. demo exercises the required scenarios;
9. no unauthorized SourceGrid/Bootstrap vendor patch exists;
10. package metadata and docs identify both vendor baselines and supported TFMs.
