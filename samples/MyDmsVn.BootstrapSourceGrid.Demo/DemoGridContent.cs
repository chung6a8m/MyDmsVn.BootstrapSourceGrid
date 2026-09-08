using System;
using System.Drawing;
using BootstrapSourceGridControl = MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid;

namespace MyDmsVn.BootstrapSourceGrid.Demo;

internal static class DemoGridContent
{
    internal static void Populate(BootstrapSourceGridControl grid)
    {
        const int rows = 40;
        const int columns = 9;
        grid.Redim(rows, columns);
        grid.FixedRows = 1;
        grid.FixedColumns = 1;
        grid.Selection.EnableMultiSelection = true;
        grid[0, 0] = new SourceGrid.Cells.Header();
        grid[0, 1] = new SourceGrid.Cells.ColumnHeader("Name");
        grid[0, 2] = new SourceGrid.Cells.ColumnHeader("TextBox");
        grid[0, 3] = new SourceGrid.Cells.ColumnHeader("Numeric");
        grid[0, 4] = new SourceGrid.Cells.ColumnHeader("DateTime factory");
        grid[0, 5] = new SourceGrid.Cells.ColumnHeader("Boolean factory");
        grid[0, 6] = new SourceGrid.Cells.ColumnHeader("Enum list");
        grid[0, 7] = new SourceGrid.Cells.ColumnHeader("DateTimePicker");
        grid[0, 8] = new SourceGrid.Cells.ColumnHeader("Notes / scenarios");

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
                grid[row, 7] = new SourceGrid.Cells.Cell(
                    "Two-column editable span",
                    typeof(string))
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
                    "Theme, selection, and scrolling row",
                    typeof(string));
            }
        }

        var readOnlyCell = (SourceGrid.Cells.Cell)grid[1, 8];
        readOnlyCell.Value = "Read-only: editor disabled";
        readOnlyCell.Editor!.EnableEdit = false;

        var customView = new SourceGrid.Cells.Views.Cell
        {
            BackColor = Color.LemonChiffon,
            ForeColor = Color.DarkSlateBlue,
        };
        grid[2, 8].Value = "Consumer custom View (theme opt-out)";
        grid[2, 8].View = customView;

        grid.Rows[0].Height = 32;
        grid.Columns[0].Width = 54;
        grid.Columns[1].Width = 140;
        grid.Columns[2].Width = 130;
        grid.Columns[3].Width = 90;
        grid.Columns[4].Width = 150;
        grid.Columns[5].Width = 130;
        grid.Columns[6].Width = 120;
        grid.Columns[7].Width = 150;
        grid.Columns[8].Width = 250;
    }

    private enum DemoChoice
    {
        Ready,
        Pending,
        Blocked,
    }
}
