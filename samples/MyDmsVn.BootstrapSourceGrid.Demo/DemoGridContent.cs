using System;
using System.Drawing;
using MyDmsVn.BootstrapSourceGrid.Editors;
using BootstrapSourceGridControl = MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid;

namespace MyDmsVn.BootstrapSourceGrid.Demo;

internal static class DemoGridContent
{
    internal static void Populate(
        BootstrapSourceGridControl grid,
        BootstrapTextBoxEditor textEditor,
        BootstrapFormattedTextBoxEditor formattedEditor,
        BootstrapLookupBoxEditor lookupEditor)
    {
        const int rows = 40;
        const int columns = 9;
        grid.Redim(rows, columns);
        grid.FixedRows = 1;
        grid.FixedColumns = 1;
        grid.Selection.EnableMultiSelection = true;
        grid[0, 0] = new SourceGrid.Cells.Header();
        grid[0, 1] = new SourceGrid.Cells.ColumnHeader("Name");
        grid[0, 2] = new SourceGrid.Cells.ColumnHeader("Bootstrap text");
        grid[0, 3] = new SourceGrid.Cells.ColumnHeader("Bootstrap formatted");
        grid[0, 4] = new SourceGrid.Cells.ColumnHeader("DateTime factory");
        grid[0, 5] = new SourceGrid.Cells.ColumnHeader("Boolean factory");
        grid[0, 6] = new SourceGrid.Cells.ColumnHeader("Bootstrap lookup");
        grid[0, 7] = new SourceGrid.Cells.ColumnHeader("DateTimePicker");
        grid[0, 8] = new SourceGrid.Cells.ColumnHeader("Notes / scenarios");

        for (var row = 1; row < rows; row++)
        {
            grid[row, 0] = new SourceGrid.Cells.RowHeader(row);
            grid[row, 1] = new SourceGrid.Cells.Cell($"Item {row:00}", typeof(string));
            grid[row, 2] = new SourceGrid.Cells.Cell($"Customer {row:00}", typeof(string))
            {
                Editor = textEditor,
            };
            grid[row, 3] = new SourceGrid.Cells.Cell(row * 1234.5m, typeof(decimal))
            {
                Editor = formattedEditor,
            };
            grid[row, 4] = new SourceGrid.Cells.Cell(
                new DateTime(2026, 9, 1).AddDays(row),
                typeof(DateTime));
            grid[row, 5] = new SourceGrid.Cells.Cell((row & 1) == 0, typeof(bool));
            grid[row, 6] = new SourceGrid.Cells.Cell(((row - 1) % 12) + 1, typeof(int))
            {
                Editor = lookupEditor,
            };

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
        grid.Columns[2].Width = 150;
        grid.Columns[3].Width = 160;
        grid.Columns[4].Width = 150;
        grid.Columns[5].Width = 130;
        grid.Columns[6].Width = 170;
        grid.Columns[7].Width = 150;
        grid.Columns[8].Width = 250;
    }

}

internal sealed class DemoLookupItem
{
    private DemoLookupItem(int id, string name, string region)
    {
        Id = id;
        Name = name;
        Region = region;
    }

    public int Id { get; }

    public string Name { get; }

    public string Region { get; }

    internal static DemoLookupItem[] CreateSampleData()
    {
        return new[]
        {
            new DemoLookupItem(1, "Northwind Traders", "Europe"),
            new DemoLookupItem(2, "Contoso", "North America"),
            new DemoLookupItem(3, "Adventure Works", "North America"),
            new DemoLookupItem(4, "Tailspin Toys", "Asia"),
            new DemoLookupItem(5, "Fabrikam", "Europe"),
            new DemoLookupItem(6, "Woodgrove Bank", "Oceania"),
            new DemoLookupItem(7, "Litware", "Asia"),
            new DemoLookupItem(8, "Proseware", "Europe"),
            new DemoLookupItem(9, "Wide World Importers", "Africa"),
            new DemoLookupItem(10, "Humongous Insurance", "North America"),
            new DemoLookupItem(11, "Consolidated Messenger", "Asia"),
            new DemoLookupItem(12, "Fourth Coffee", "South America"),
        };
    }
}
