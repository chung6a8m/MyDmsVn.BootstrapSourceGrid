using System;
using System.Windows.Forms;
using MyDmsVn.Bootstrap5WinFormUI.Theme;
using BootstrapSourceGridControl = MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid;

namespace MyDmsVn.BootstrapSourceGrid.Demo;

public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        PopulateGrid(_grid);
        _grid.DpiChangedAfterParent += OnGridDpiChangedAfterParent;
        BootstrapThemeManager.ThemeChanged += OnThemeChanged;
        UpdateDiagnostics();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _grid.Selection.Focus(new SourceGrid.Position(1, 1), true);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            BootstrapThemeManager.ThemeChanged -= OnThemeChanged;
            _grid.DpiChangedAfterParent -= OnGridDpiChangedAfterParent;
            _components.Dispose();
        }

        base.Dispose(disposing);
    }

    private static void PopulateGrid(BootstrapSourceGridControl grid)
    {
        const int rows = 32;
        const int columns = 9;
        grid.Redim(rows, columns);
        grid.FixedRows = 1;
        grid.FixedColumns = 1;
        grid[0, 0] = new SourceGrid.Cells.Header();
        grid[0, 1] = new SourceGrid.Cells.ColumnHeader("Name");
        grid[0, 2] = new SourceGrid.Cells.ColumnHeader("TextBox");
        grid[0, 3] = new SourceGrid.Cells.ColumnHeader("Numeric");
        grid[0, 4] = new SourceGrid.Cells.ColumnHeader("DateTime factory");
        grid[0, 5] = new SourceGrid.Cells.ColumnHeader("Boolean factory");
        grid[0, 6] = new SourceGrid.Cells.ColumnHeader("Enum list");
        grid[0, 7] = new SourceGrid.Cells.ColumnHeader("DateTimePicker");
        grid[0, 8] = new SourceGrid.Cells.ColumnHeader("Notes");

        for (var row = 1; row < rows; row++)
        {
            grid[row, 0] = new SourceGrid.Cells.RowHeader(row);
            grid[row, 1] = new SourceGrid.Cells.Cell($"Item {row:00}", typeof(string));
            grid[row, 2] = new SourceGrid.Cells.Cell($"Edit {row}", typeof(string));
            grid[row, 3] = new SourceGrid.Cells.Cell(row * 10, typeof(int));
            grid[row, 4] = new SourceGrid.Cells.Cell(
                new DateTime(2026, 9, 1).AddDays(row),
                typeof(DateTime));
            grid[row, 5] = new SourceGrid.Cells.Cell((row & 1) == 0, typeof(bool));
            grid[row, 6] = new SourceGrid.Cells.Cell(
                (row & 1) == 0 ? DemoChoice.Ready : DemoChoice.Pending,
                typeof(DemoChoice));

            if (row == 3)
            {
                grid[row, 7] = new SourceGrid.Cells.Cell("Two-column span for DPI alignment")
                {
                    ColumnSpan = 2,
                };
            }
            else
            {
                grid[row, 7] = new SourceGrid.Cells.Cell(
                    new DateTime(2026, 9, 1).AddDays(row),
                    new SourceGrid.Cells.Editors.DateTimePicker());
                grid[row, 8] = new SourceGrid.Cells.Cell(
                    "Theme and DPI diagnostic row",
                    typeof(string));
            }
        }

        grid.Rows[0].Height = 32;
        grid.Columns[0].Width = 54;
        grid.Columns[1].Width = 140;
        grid.Columns[2].Width = 130;
        grid.Columns[3].Width = 90;
        grid.Columns[4].Width = 150;
        grid.Columns[5].Width = 130;
        grid.Columns[6].Width = 120;
        grid.Columns[7].Width = 150;
        grid.Columns[8].Width = 240;
    }

    private void OnLightThemeClick(object? sender, EventArgs e)
    {
        BootstrapThemeManager.CurrentTheme = BootstrapTheme.CreateDefault(BootstrapThemeMode.Light);
    }

    private void OnDarkThemeClick(object? sender, EventArgs e)
    {
        BootstrapThemeManager.CurrentTheme = BootstrapTheme.CreateDefault(BootstrapThemeMode.Dark);
    }

    private void OnThemeChanged(object? sender, BootstrapThemeChangedEventArgs e)
    {
        UpdateDiagnostics();
    }

    private void OnGridDpiChangedAfterParent(object? sender, EventArgs e)
    {
        UpdateDiagnostics();
    }

    private void UpdateDiagnostics()
    {
        if (_diagnostics.IsDisposed || _grid.IsDisposed)
        {
            return;
        }

        var theme = BootstrapThemeManager.CurrentTheme;
        var metrics = _grid.CurrentDpiMetrics;
        var targetFramework = AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName ?? "unknown TFM";
        var fontMode = _grid.OwnedThemeFont is null ? "consumer font" : "theme font";
        _diagnostics.Text =
            $"Runtime: {targetFramework}; CLR {Environment.Version} | " +
            $"DeviceDpi: {_grid.CurrentDpi} | Theme: {theme.Mode} | " +
            $"Padding: {metrics.CellPadding}; Border: {metrics.CellBorderThickness}; " +
            $"Focus: {metrics.FocusThickness} | Font ownership: {fontMode}";
        _diagnostics.BackColor = theme.Colors.Surface;
        _diagnostics.ForeColor = theme.Colors.Text;
    }

    private enum DemoChoice
    {
        Ready,
        Pending,
        Blocked,
    }
}
