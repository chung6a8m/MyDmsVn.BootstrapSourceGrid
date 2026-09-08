using System;
using System.Drawing;
using System.Windows.Forms;
using MyDmsVn.Bootstrap5WinFormUI.Theme;

namespace MyDmsVn.BootstrapSourceGrid.Demo;

public sealed partial class MainForm : Form
{
    private Font? _consumerFont;

    public MainForm()
    {
        InitializeComponent();
        DemoGridContent.Populate(_grid);
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
            _consumerFont?.Dispose();
            _consumerFont = null;
        }

        base.Dispose(disposing);
    }

    private void OnResetGridClick(object? sender, EventArgs e)
    {
        DemoGridContent.Populate(_grid);
        _grid.Selection.Focus(new SourceGrid.Position(1, 1), true);
        UpdateDiagnostics();
    }

    private void OnConsumerFontClick(object? sender, EventArgs e)
    {
        if (_consumerFont is not null)
        {
            return;
        }

        _consumerFont = new Font(FontFamily.GenericMonospace, _grid.Font.SizeInPoints);
        _grid.Font = _consumerFont;
        _consumerFontButton.Enabled = false;
        _consumerFontButton.Text = "Consumer font active";
        UpdateDiagnostics();
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
        _interactionHelp.BackColor = theme.Colors.SurfaceSecondary;
        _interactionHelp.ForeColor = theme.Colors.Text;
        _toolbar.BackColor = theme.Colors.Surface;
        BackColor = theme.Colors.Surface;
    }
}
