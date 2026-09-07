using MyDmsVn.BootstrapSourceGrid.Theming;
using MyDmsVn.BootstrapSourceGrid.Views;

namespace MyDmsVn.BootstrapSourceGrid.Internal;

internal sealed class BootstrapSourceGridStyleApplicator
{
    private readonly BootstrapSourceGridCellView _cellView;

    internal BootstrapSourceGridStyleApplicator(
        BootstrapSourceGridThemeSnapshot snapshot,
        BootstrapSourceGridDpiMetrics dpiMetrics)
    {
        _cellView = new BootstrapSourceGridCellView(snapshot, dpiMetrics);
    }

    internal void ApplyDefaultView(SourceGrid.Cells.ICellVirtual? cell)
    {
        if (cell is null || ReferenceEquals(cell.View, _cellView))
        {
            return;
        }

        if (ReferenceEquals(cell.View, SourceGrid.Cells.Views.Cell.Default))
        {
            cell.View = _cellView;
        }
    }

    internal void ApplyTheme(
        BootstrapSourceGridThemeSnapshot snapshot,
        BootstrapSourceGridDpiMetrics dpiMetrics)
    {
        _cellView.ApplyTheme(snapshot, dpiMetrics);
    }
}
