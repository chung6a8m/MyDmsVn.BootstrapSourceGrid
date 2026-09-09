# MyDmsVn.BootstrapSourceGrid

Bootstrap-themed SourceGrid control for native Windows Forms.

`BootstrapSourceGrid` derives directly from `SourceGrid.Grid`, preserving SourceGrid cells, views, editors, selection, spans, navigation, and scrolling while applying `MyDmsVn.Bootstrap5WinFormUI` theme, typography, and DPI semantics to the integration-owned visual layer.

## Status

The source-distributed MVP is implemented and validated for:

```text
net48;net8.0-windows
```

Public NuGet publication remains blocked by the vendor package/source-equivalence gate documented in [`docs/RELEASE.md`](./docs/RELEASE.md). Current supported consumption is source checkout with pinned vendor submodules and `ProjectReference`.

Bootstrap-native SourceGrid editor integration is implemented for:

- `BootstrapTextBox`
- `BootstrapFormattedTextBox`
- `BootstrapLookupBox`

The integration preserves SourceGrid's edit lifecycle and uses grid-owned shared adapters rather than one composite Bootstrap control per cell. See [`docs/EDITOR_REPLACEMENT.md`](./docs/EDITOR_REPLACEMENT.md).

For historical context, see [Archive](./docs/archive/).

## Fixed baselines

- Bootstrap framework: `chung6a8m/MyDmsVn.Bootstrap5WinFormUI@cceba3c969e28726935793a1c6ca3772bed60a35`
- SourceGrid: `chung6a8m/sourcegrid@f4e457b43582bf01892f50bdc74aa480531e5944`
- Integration license: MIT

Dependency direction remains:

```text
MyDmsVn.BootstrapSourceGrid
    +--> MyDmsVn.Bootstrap5WinFormUI
    `--> SourceGrid
```

Neither vendor depends on this integration or on the other vendor because of it.

## Quick start

```csharp
using MyDmsVn.Bootstrap5WinFormUI.Controls;

var grid = new BootstrapSourceGrid
{
    Dock = DockStyle.Fill,
};

grid.Redim(20, 4);
grid.FixedRows = 1;
grid[0, 0] = new SourceGrid.Cells.Header();
grid[0, 1] = new SourceGrid.Cells.ColumnHeader("Name");
grid[1, 0] = new SourceGrid.Cells.RowHeader(1);
grid[1, 1] = new SourceGrid.Cells.Cell("Northwind", typeof(string));
Controls.Add(grid);
```

Runtime theme changes use the framework theme manager and do not require rebuilding grid data:

```csharp
using MyDmsVn.Bootstrap5WinFormUI.Theme;

BootstrapThemeManager.CurrentTheme =
    BootstrapTheme.CreateDefault(BootstrapThemeMode.Dark);
```

Consumer-assigned SourceGrid Views remain authoritative. Consumer-assigned `grid.Font` also opts that grid instance out of integration-owned theme-font replacement.

## Bootstrap editors

Create one editor per grid/column/configuration, configure its strongly typed Bootstrap
control, and assign the shared editor to the applicable cells:

```csharp
var editor = grid.BootstrapEditors.CreateTextBox(typeof(string));
editor.BootstrapControl.PlaceholderText = "Customer name";

for (var row = 1; row < grid.RowsCount; row++)
{
    grid[row, 1].Editor = editor;
}
```

`BootstrapSourceGrid` owns and disposes every editor created by `BootstrapEditors`, including
editors that are never started. Do not share an editor with another grid. On the pinned
SourceGrid baseline this is a supported-use restriction, not a guaranteed fail-before-attach
runtime exception; strict enforcement requires the pre-attach seam described in
[`docs/UPSTREAM_API_SEAMS.md`](./docs/UPSTREAM_API_SEAMS.md).

Logical values returned to SourceGrid are:

```text
BootstrapTextBox          -> Text
BootstrapFormattedTextBox -> RawValue
BootstrapLookupBox        -> SelectedValue
```

SourceGrid performs final conversion and validation for all three.

## Build from source

```powershell
git clone --recurse-submodules https://github.com/chung6a8m/MyDmsVn.BootstrapSourceGrid.git
cd MyDmsVn.BootstrapSourceGrid
git submodule update --init --recursive
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
```

GitHub Actions is temporarily disabled for this repository as of 2026-09-09; local validation is the required automated gate until Actions is restored.

## Demo

```powershell
dotnet run --project samples/MyDmsVn.BootstrapSourceGrid.Demo/MyDmsVn.BootstrapSourceGrid.Demo.csproj -f net8.0-windows
```

The demo covers the completed MVP plus shared Bootstrap text, formatted, and lookup editor
columns, including a read-only lookup DisplayMember companion column, lookup search/navigation,
closed-popup Enter/Escape, and runtime theme switching scenarios.

## Documentation

Start here for current work:

1. [`AGENTS.md`](./AGENTS.md) — mandatory operating rules.
2. [`AI_CONTEXT.md`](./AI_CONTEXT.md) — compact project model.
3. [`docs/EDITOR_REPLACEMENT.md`](./docs/EDITOR_REPLACEMENT.md) — implemented editor architecture.
4. [`docs/DEVELOPMENT_PLAN.md`](./docs/DEVELOPMENT_PLAN.md) — current development status.
5. [`docs/plans/`](./docs/plans/) — active task plans only.

Canonical reference documents:

- [`docs/DECISIONS.md`](./docs/DECISIONS.md)
- [`docs/ARCHITECTURE.md`](./docs/ARCHITECTURE.md)
- [`docs/UPSTREAM_API_SEAMS.md`](./docs/UPSTREAM_API_SEAMS.md)
- [`docs/UPSTREAM.md`](./docs/UPSTREAM.md)
- [`docs/COMPATIBILITY.md`](./docs/COMPATIBILITY.md)
- [`docs/TESTING.md`](./docs/TESTING.md)
- [`docs/RELEASE.md`](./docs/RELEASE.md)

## Design boundary

This repository is an integration layer, not a new grid engine. SourceGrid remains authoritative for grid behavior. Bootstrap5WinFormUI remains authoritative for Bootstrap visual/control semantics. New integration code should be a narrow translation/adapter layer between those two established systems.
