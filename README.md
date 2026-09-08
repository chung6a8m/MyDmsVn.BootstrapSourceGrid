# MyDmsVn.BootstrapSourceGrid

`MyDmsVn.BootstrapSourceGrid` is a Bootstrap-themed SourceGrid control for native Windows Forms applications. It derives directly from `SourceGrid.Grid`, so existing SourceGrid cells, views, editors, selection, spans, keyboard behavior, and scrolling remain available while default visuals follow `MyDmsVn.Bootstrap5WinFormUI` themes.

## Status

The source-distributed MVP is implemented and validated on both supported TFMs. Public NuGet publication is currently blocked by D-017 because exact vendor packages and complete license/source-equivalence evidence are not yet available. The supported consumption model today is source checkout with the pinned vendor submodules and `ProjectReference`; do not use `dotnet add package MyDmsVn.BootstrapSourceGrid` until the release gate is documented as complete.

## Platform and dependencies

- Windows Forms on `net48` and `net8.0-windows`.
- `MyDmsVn.Bootstrap5WinFormUI` for theme, typography, color, and DPI semantics.
- SourceGrid 5.0 for the grid engine.
- MIT license for this integration; see [LICENSE](LICENSE).

Development builds use pinned vendor submodules and `ProjectReference`:

- `chung6a8m/MyDmsVn.Bootstrap5WinFormUI@95077df0c8bad8593143c2190606d2f444bfc653`
- `chung6a8m/sourcegrid@f4e457b43582bf01892f50bdc74aa480531e5944`

Public NuGet publication is intentionally blocked until exact, resolvable vendor packages are verified against these source baselines, including both TFMs and license/notice obligations. See [release process](docs/RELEASE.md) and [upstream policy](docs/UPSTREAM.md).

## Consume from source today

Add this repository to your application's source tree and initialize its nested vendor submodules:

```powershell
git submodule add https://github.com/chung6a8m/MyDmsVn.BootstrapSourceGrid.git vendor/MyDmsVn.BootstrapSourceGrid
git submodule update --init --recursive
dotnet sln add vendor/MyDmsVn.BootstrapSourceGrid/src/MyDmsVn.BootstrapSourceGrid/MyDmsVn.BootstrapSourceGrid.csproj
dotnet add path/to/Your.WinFormsApp.csproj reference vendor/MyDmsVn.BootstrapSourceGrid/src/MyDmsVn.BootstrapSourceGrid/MyDmsVn.BootstrapSourceGrid.csproj
```

The integration project resolves its pinned Bootstrap5WinFormUI and SourceGrid projects from its own `vendor/` submodules. Commit the parent repository's integration-submodule pointer so every consumer build uses the reviewed integration revision. If you prefer a standalone clone instead, run `git submodule update --init --recursive` in that clone and reference the same product `.csproj` by its relative or absolute path.

## Quick start

Reference the integration and its approved vendor dependencies, then use normal SourceGrid APIs:

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
grid.Selection.EnableMultiSelection = true;
Controls.Add(grid);
```

Switch themes at runtime without recreating the grid or its data:

```csharp
using MyDmsVn.Bootstrap5WinFormUI.Theme;

BootstrapThemeManager.CurrentTheme =
    BootstrapTheme.CreateDefault(BootstrapThemeMode.Dark);
```

Only exact SourceGrid default View singletons are substituted automatically. A consumer View remains authoritative across theme changes:

```csharp
var customView = new SourceGrid.Cells.Views.Cell
{
    BackColor = Color.LemonChiffon,
    ForeColor = Color.DarkSlateBlue,
};

grid[1, 1].View = customView;
```

The control initially owns a font created from the Bootstrap body typography token. Assigning `grid.Font` opts that grid instance into consumer-font mode; later theme changes keep the exact assigned Font, and the application remains responsible for disposing it after the grid is disposed.

## Demo

The [demo application](samples/MyDmsVn.BootstrapSourceGrid.Demo) includes light/dark switching, reset, theme/DPI diagnostics, editable text/numeric/date/bool/enum cells, a read-only cell, sortable headers, a span, multi-selection, a custom View opt-out, a consumer Font opt-out, keyboard instructions, and enough data to scroll.

```powershell
dotnet run --project samples/MyDmsVn.BootstrapSourceGrid.Demo/MyDmsVn.BootstrapSourceGrid.Demo.csproj -f net8.0-windows
```

## Build from source

```powershell
git clone --recurse-submodules https://github.com/chung6a8m/MyDmsVn.BootstrapSourceGrid.git
cd MyDmsVn.BootstrapSourceGrid
git submodule update --init --recursive
dotnet restore MyDmsVn.BootstrapSourceGrid.sln
dotnet build MyDmsVn.BootstrapSourceGrid.sln -c Release
dotnet test tests/MyDmsVn.BootstrapSourceGrid.Tests/MyDmsVn.BootstrapSourceGrid.Tests.csproj -c Release --no-build --blame-hang --blame-hang-timeout 5m
```

## Documentation

- [Package overview](docs/PACKAGE_README.md)
- [Known MVP limitations](docs/KNOWN_LIMITATIONS.md)
- [Release process](docs/RELEASE.md)
- [Product requirements](docs/PRD.md)
- [Architecture](docs/ARCHITECTURE.md)
- [Compatibility contract](docs/COMPATIBILITY.md)
- [Testing strategy](docs/TESTING.md)
- [Pinned upstream dependencies](docs/UPSTREAM.md)
- [Architectural decisions](docs/DECISIONS.md)

## Design boundaries

`BootstrapSourceGrid` is an integration layer, not a new grid engine. It does not wrap SourceGrid rows, columns, cells, ranges, or selection; it does not replace SourceGrid's editor architecture or native scrollbar subsystem; and neither vendor depends on this integration or on the other vendor because of it.
