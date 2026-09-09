using System;
using System.Drawing;
using System.Windows.Forms;
using MyDmsVn.Bootstrap5WinFormUI.Controls;
using MyDmsVn.Bootstrap5WinFormUI.Formatting;
using MyDmsVn.Bootstrap5WinFormUI.Theme;
using MyDmsVn.BootstrapSourceGrid.Editors;

namespace MyDmsVn.BootstrapSourceGrid.Demo;

public sealed partial class MainForm : Form
{
    private Font? _consumerFont;
    private readonly BootstrapTextBoxEditor _textEditor;
    private readonly BootstrapFormattedTextBoxEditor _formattedEditor;
    private readonly BootstrapLookupBoxEditor _lookupEditor;
    private readonly DemoLookupItem[] _lookupItems;
    private readonly SourceGrid.Cells.Controllers.CustomEvents _lookupValueEvents = new();

    public MainForm()
    {
        InitializeComponent();
        _textEditor = _grid.BootstrapEditors.CreateTextBox(typeof(string));
        _textEditor.BootstrapControl.PlaceholderText = "Customer name";
        _textEditor.BootstrapControl.ShowClearButton = true;

        _formattedEditor = _grid.BootstrapEditors.CreateFormattedTextBox(typeof(decimal));
        _formattedEditor.BootstrapControl.FormatMode = BootstrapInputFormatMode.Numeral;
        _formattedEditor.BootstrapControl.NumeralOptions.DecimalScale = 2;
        _formattedEditor.BootstrapControl.NumeralOptions.Prefix = "$";

        _lookupItems = DemoLookupItem.CreateSampleData();
        _lookupValueEvents.ValueChanged += OnLookupValueChanged;
        _lookupEditor = _grid.BootstrapEditors.CreateLookupBox(typeof(int));
        ConfigureLookup(_lookupEditor.BootstrapControl, _lookupItems);

        DemoGridContent.Populate(
            _grid,
            _textEditor,
            _formattedEditor,
            _lookupEditor,
            _lookupItems,
            _lookupValueEvents);
        _textEditor.Control.Validated += OnEditorValidated;
        _formattedEditor.Control.Validated += OnEditorValidated;
        _lookupEditor.Control.Validated += OnEditorValidated;
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
            _textEditor.Control.Validated -= OnEditorValidated;
            _formattedEditor.Control.Validated -= OnEditorValidated;
            _lookupEditor.Control.Validated -= OnEditorValidated;
            _lookupValueEvents.ValueChanged -= OnLookupValueChanged;
            _components.Dispose();
            _consumerFont?.Dispose();
            _consumerFont = null;
        }

        base.Dispose(disposing);
    }

    private void OnResetGridClick(object? sender, EventArgs e)
    {
        DemoGridContent.Populate(
            _grid,
            _textEditor,
            _formattedEditor,
            _lookupEditor,
            _lookupItems,
            _lookupValueEvents);
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

    private void OnLookupValueChanged(object? sender, EventArgs e)
    {
        if (sender is not SourceGrid.CellContext context || context.Position.Row <= 0)
        {
            return;
        }

        var lookupValue = _grid[context.Position.Row, 6].Value;
        var displayValue = string.Empty;
        foreach (var item in _lookupItems)
        {
            if (Equals(item.Id, lookupValue))
            {
                displayValue = item.Name;
                break;
            }
        }

        _grid[context.Position.Row, 7].Value = displayValue;
    }

    private void OnEditorValidated(object? sender, EventArgs e)
    {
        if (IsDisposed || Disposing || !IsHandleCreated)
        {
            return;
        }

        BeginInvoke((MethodInvoker)UpdateDiagnostics);
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
        var activeValue = GetActiveLogicalValue();
        _diagnostics.Text =
            $"Runtime: {targetFramework}; CLR {Environment.Version} | " +
            $"DeviceDpi: {_grid.CurrentDpi} | Theme: {theme.Mode} | " +
            $"Padding: {metrics.CellPadding}; Border: {metrics.CellBorderThickness}; " +
            $"Focus: {metrics.FocusThickness} | Font ownership: {fontMode} | " +
            $"Active logical value: {activeValue}";
        _diagnostics.BackColor = theme.Colors.Surface;
        _diagnostics.ForeColor = theme.Colors.Text;
        _interactionHelp.BackColor = theme.Colors.SurfaceSecondary;
        _interactionHelp.ForeColor = theme.Colors.Text;
        _toolbar.BackColor = theme.Colors.Surface;
        BackColor = theme.Colors.Surface;
    }

    private string GetActiveLogicalValue()
    {
        var active = _grid.Selection.ActivePosition;
        if (active.IsEmpty())
        {
            return "none";
        }

        var value = _grid[active.Row, active.Column].Value;
        return value is null ? "null" : $"{value} ({value.GetType().Name})";
    }

    private static void ConfigureLookup(BootstrapLookupBox lookup, DemoLookupItem[] lookupItems)
    {
        lookup.DisplayMember = nameof(DemoLookupItem.Name);
        lookup.ValueMember = nameof(DemoLookupItem.Id);
        lookup.DataSource = lookupItems;
        lookup.Columns.Add(new BootstrapLookupColumnDefinition
        {
            DataPropertyName = nameof(DemoLookupItem.Id),
            HeaderText = "ID",
            Width = 55,
        });
        lookup.Columns.Add(new BootstrapLookupColumnDefinition
        {
            DataPropertyName = nameof(DemoLookupItem.Name),
            HeaderText = "Customer",
            Width = 150,
        });
        lookup.Columns.Add(new BootstrapLookupColumnDefinition
        {
            DataPropertyName = nameof(DemoLookupItem.Region),
            HeaderText = "Region",
            Width = 90,
        });
        lookup.SearchMembers.Add(nameof(DemoLookupItem.Name));
        lookup.SearchMembers.Add(nameof(DemoLookupItem.Region));
        lookup.MinimumSearchLength = 0;
        lookup.SearchDebounceMilliseconds = 0;
        lookup.DropDownWidth = 320;
    }
}
