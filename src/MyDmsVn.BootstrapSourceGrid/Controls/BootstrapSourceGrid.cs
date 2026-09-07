using System;
using System.ComponentModel;
using System.Drawing;
using MyDmsVn.Bootstrap5WinFormUI.Theme;
using MyDmsVn.BootstrapSourceGrid.Internal;
using MyDmsVn.BootstrapSourceGrid.Theming;

namespace MyDmsVn.Bootstrap5WinFormUI.Controls;

/// <summary>
/// Provides a Bootstrap-themed SourceGrid while preserving SourceGrid grid behavior.
/// </summary>
[ToolboxItem(true)]
public class BootstrapSourceGrid : SourceGrid.Grid
{
    private bool _initialized;
    private bool _settingThemeFont;
    private bool _themeSubscribed;
    private bool _useThemeFont = true;
    private Font? _themeFont;
    private BootstrapSourceGridThemeSnapshot _themeSnapshot;
    private readonly BootstrapSourceGridStyleApplicator _styleApplicator;
    private readonly BootstrapSourceGridSelectionStyle _selectionStyle;

    /// <summary>
    /// Initializes a new Bootstrap-themed SourceGrid.
    /// </summary>
    public BootstrapSourceGrid()
    {
        Name = nameof(BootstrapSourceGrid);
        var theme = BootstrapThemeManager.CurrentTheme;
        _themeSnapshot = BootstrapSourceGridThemeAdapter.CreateSnapshot(theme);
        _styleApplicator = new BootstrapSourceGridStyleApplicator(
            _themeSnapshot,
            BootstrapSourceGridDpiMetrics.FromTheme(theme, CurrentDpi));
        _selectionStyle = new BootstrapSourceGridSelectionStyle();
        _initialized = true;
        BootstrapThemeManager.ThemeChanged += OnThemeChanged;
        _themeSubscribed = true;
        ApplyThemeFont(_themeSnapshot.BodyFont);
        ApplyBootstrapTheme();
    }

    internal BootstrapSourceGridThemeSnapshot CurrentThemeSnapshot => _themeSnapshot;

    /// <inheritdoc />
    public override SourceGrid.Cells.ICellVirtual GetCell(int row, int column)
    {
        var cell = base.GetCell(row, column);
        _styleApplicator.ApplyDefaultView(cell);
        return cell;
    }

    /// <inheritdoc />
    protected override SourceGrid.Selection.SelectionBase CreateSelectionObject()
    {
        var selection = base.CreateSelectionObject();
        if (_initialized)
        {
            selection.BindToGrid(this);
            try
            {
                ApplySelectionTheme(selection, BootstrapThemeManager.CurrentTheme);
            }
            finally
            {
                selection.UnBindToGrid();
            }
        }

        return selection;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    private void OnThemeChanged(object? sender, BootstrapThemeChangedEventArgs e)
    {
        if (IsDisposed || Disposing)
        {
            return;
        }

        if (InvokeRequired)
        {
            try
            {
                BeginInvoke((Action)(() => ApplyThemeChange(e.NewTheme)));
            }
            catch (InvalidOperationException) when (IsDisposed || Disposing)
            {
                // Disposal can race a queued application-level theme change.
            }

            return;
        }

        ApplyThemeChange(e.NewTheme);
    }

    private void ApplyThemeChange(BootstrapTheme theme)
    {
        if (IsDisposed || Disposing)
        {
            return;
        }

        _themeSnapshot = BootstrapSourceGridThemeAdapter.CreateSnapshot(theme);
        if (_useThemeFont)
        {
            ApplyThemeFont(_themeSnapshot.BodyFont);
        }

        ApplyBootstrapTheme();
        Invalidate();
    }

    internal virtual void ApplyBootstrapTheme()
    {
        var theme = BootstrapThemeManager.CurrentTheme;
        var dpiMetrics = BootstrapSourceGridDpiMetrics.FromTheme(theme, CurrentDpi);
        BackColor = _themeSnapshot.CellBackColor;
        ForeColor = _themeSnapshot.CellForeColor;
        _styleApplicator.ApplyTheme(_themeSnapshot, dpiMetrics);
        _selectionStyle.ApplyTheme(
            (SourceGrid.Selection.SelectionBase)Selection,
            _themeSnapshot,
            dpiMetrics);
    }

    private void ApplySelectionTheme(
        SourceGrid.Selection.SelectionBase selection,
        BootstrapTheme theme)
    {
        _selectionStyle.ApplyTheme(
            selection,
            _themeSnapshot,
            BootstrapSourceGridDpiMetrics.FromTheme(theme, CurrentDpi));
    }

    private int CurrentDpi => DeviceDpi > 0
        ? DeviceDpi
        : MyDmsVn.Bootstrap5WinFormUI.Rendering.DpiScaler.DefaultDpi;

    private void ApplyThemeFont(BootstrapFontToken token)
    {
        if (ThemeFontMatches(token))
        {
            return;
        }

        var nextFont = new Font(token.FontFamilyName, token.SizeInPoints, token.Style);
        var previous = _themeFont;
        _themeFont = nextFont;
        _settingThemeFont = true;
        try
        {
            Font = nextFont;
        }
        finally
        {
            _settingThemeFont = false;
        }

        previous?.Dispose();
    }

    private bool ThemeFontMatches(BootstrapFontToken token)
    {
        return _themeFont is not null &&
            string.Equals(_themeFont.Name, token.FontFamilyName, StringComparison.OrdinalIgnoreCase) &&
            Math.Abs(_themeFont.SizeInPoints - token.SizeInPoints) < 0.01f &&
            _themeFont.Style == token.Style;
    }

    private void DisposeThemeFont()
    {
        var font = _themeFont;
        _themeFont = null;
        font?.Dispose();
    }
}
