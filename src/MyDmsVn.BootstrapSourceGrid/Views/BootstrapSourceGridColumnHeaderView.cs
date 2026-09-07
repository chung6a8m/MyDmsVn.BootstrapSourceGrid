using DevAge.Drawing;
using MyDmsVn.BootstrapSourceGrid.Internal;
using MyDmsVn.BootstrapSourceGrid.Theming;

namespace MyDmsVn.BootstrapSourceGrid.Views;

internal sealed class BootstrapSourceGridColumnHeaderView : SourceGrid.Cells.Views.ColumnHeader
{
    private readonly DevAge.Drawing.VisualElements.ColumnHeader _background;

    internal BootstrapSourceGridColumnHeaderView(
        BootstrapSourceGridThemeSnapshot snapshot,
        BootstrapSourceGridDpiMetrics dpiMetrics)
    {
        _background = new DevAge.Drawing.VisualElements.ColumnHeader
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
