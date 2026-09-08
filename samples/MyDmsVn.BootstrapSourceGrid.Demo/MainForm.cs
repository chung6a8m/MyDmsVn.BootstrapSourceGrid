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
        const int columns = 5;
        grid.Redim(rows, columns);
        grid.FixedRows = 1;
        grid.FixedColumns = 1;
        grid[0, 0] = new SourceGrid.Cells.Header();
        grid[0, 1] = new SourceGrid.Cells.ColumnHeader("Name");
        grid[0, 2] = new SourceGrid.Cells.ColumnHeader("Status");
        grid[0, 3] = new SourceGrid.Cells.ColumnHeader("Notes");
        grid[0, 4] = new SourceGrid.Cells.ColumnHeader("Editable value");

        for (var row = 1; row < rows; row++)
        {
            grid[row, 0] = new SourceGrid.Cells.RowHeader(row);
            grid[row, 1] = new SourceGrid.Cells.Cell($"Item {row:00}", typeof(string));
            grid[row, 2] = new SourceGrid.Cells.Cell((row & 1) == 0 ? "Ready" : "Pending", typeof(string));

            if (row == 3)
            {
                grid[row, 3] = new SourceGrid.Cells.Cell("Two-column span for DPI alignment")
                {
                    ColumnSpan = 2,
                };
            }
            else
            {
                grid[row, 3] = new SourceGrid.Cells.Cell("Theme and DPI diagnostic row", typeof(string));
                grid[row, 4] = new SourceGrid.Cells.Cell($"Edit {row}", typeof(string));
            }
        }

        grid.Rows[0].Height = 32;
        grid.Columns[0].Width = 54;
        grid.Columns[1].Width = 140;
        grid.Columns[2].Width = 110;
        grid.Columns[3].Width = 280;
        grid.Columns[4].Width = 170;
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
}
