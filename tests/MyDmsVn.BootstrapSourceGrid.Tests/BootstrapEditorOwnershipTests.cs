using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using MyDmsVn.BootstrapSourceGrid.Tests.EditorProbes;
using NUnit.Framework;
using BootstrapSourceGridControl = MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid;

namespace MyDmsVn.BootstrapSourceGrid.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class BootstrapEditorOwnershipTests
{
    [OneTimeSetUp]
    public void ConfigureWinForms()
    {
        WinFormsTestGuard.Configure();
    }

    [Test]
    public void BootstrapControlIsCreatedBeforeAnyGridEditStarts()
    {
        var editor = new BootstrapTextBoxProbeEditor();
        try
        {
            Assert.That(editor.BootstrapControl, Is.Not.Null);
            Assert.That(editor.Control, Is.SameAs(editor.BootstrapControl));
            Assert.That(editor.Grid, Is.Null);
            Assert.That(editor.IsEditing, Is.False);
        }
        finally
        {
            editor.Control.Dispose();
            editor.Dispose();
        }
    }

    [Test]
    public void OneEditorEditsMultipleCellsSequentiallyInOneGrid()
    {
        using (var form = new Form())
        using (var grid = new BootstrapSourceGridControl())
        {
            var editor = new BootstrapTextBoxProbeEditor();
            var control = editor.BootstrapControl;
            var firstCell = new SourceGrid.Cells.Cell("first") { Editor = editor };
            var secondCell = new SourceGrid.Cells.Cell("second") { Editor = editor };
            grid.Redim(2, 1);
            grid.Rows[0].Height = 30;
            grid.Rows[1].Height = 30;
            grid[0, 0] = firstCell;
            grid[1, 0] = secondCell;
            form.ClientSize = new Size(240, 100);
            form.Controls.Add(grid);
            grid.Dock = DockStyle.Fill;
            form.Show();

            try
            {
                EditAndCommit(grid, firstCell, new SourceGrid.Position(0, 0), editor, "first updated");
                EditAndCommit(grid, secondCell, new SourceGrid.Position(1, 0), editor, "second updated");

                Assert.That(firstCell.Value, Is.EqualTo("first updated"));
                Assert.That(secondCell.Value, Is.EqualTo("second updated"));
                Assert.That(editor.BootstrapControl, Is.SameAs(control));
                Assert.That(editor.Grid, Is.SameAs(grid));
            }
            finally
            {
                form.Close();
                editor.Control.Dispose();
                editor.Dispose();
            }
        }
    }

    private static void EditAndCommit(
        BootstrapSourceGridControl grid,
        SourceGrid.Cells.Cell cell,
        SourceGrid.Position position,
        BootstrapTextBoxProbeEditor editor,
        string editedValue)
    {
        var context = new SourceGrid.CellContext(grid, position, cell);
        grid.GetCell(position).View.Measure(context, Size.Empty);
        context.StartEdit();
        Assert.That(editor.IsEditing, Is.True);
        editor.BootstrapControl.Text = editedValue;
        Assert.That(context.EndEdit(false), Is.True);
        Assert.That(editor.IsEditing, Is.False);
    }
}
