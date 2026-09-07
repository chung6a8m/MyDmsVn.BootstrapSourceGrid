using DevAge.Drawing;
using MyDmsVn.BootstrapSourceGrid.Internal;
using MyDmsVn.BootstrapSourceGrid.Theming;

namespace MyDmsVn.BootstrapSourceGrid.Views;

internal sealed class BootstrapSourceGridRowHeaderView : SourceGrid.Cells.Views.RowHeader
{
    private readonly DevAge.Drawing.VisualElements.RowHeader _background;

    internal BootstrapSourceGridRowHeaderView(
        BootstrapSourceGridThemeSnapshot snapshot,
        BootstrapSourceGridDpiMetrics dpiMetrics)
    {
        _background = new DevAge.Drawing.VisualElements.RowHeader
        {
            BackgroundColorStyle = BackgroundColorStyle.Solid,
        };
        Background = _background;
        ApplyTheme(snapshot, dpiMetrics);
    }

    internal void ApplyTheme(
        BootstrapSourceGridThemeSnapshot snapshot,
        BootstrapSourceGridDpiMetrics dpiMetrics)
    {
        var border = new RectangleBorder(
            new BorderLine(snapshot.BorderColor, dpiMetrics.CellBorderThickness));
        _background.BackColor = snapshot.HeaderBackColor;
        _background.Border = border;
        ForeColor = snapshot.HeaderForeColor;
        Padding = new Padding(dpiMetrics.CellPadding);
        Font = null;
    }
}
