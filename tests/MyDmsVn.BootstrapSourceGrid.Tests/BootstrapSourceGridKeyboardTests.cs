using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using MyDmsVn.Bootstrap5WinFormUI.Theme;
using NUnit.Framework;
using BootstrapSourceGridControl = MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid;

namespace MyDmsVn.BootstrapSourceGrid.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class BootstrapSourceGridKeyboardTests
{
    [OneTimeSetUp]
    public void ConfigureWinForms()
    {
        WinFormsTestGuard.Configure();
    }

    [Test]
    public void SelectionFocusRemainsUsableAfterThemeSwitch()
    {
        var originalTheme = BootstrapThemeManager.CurrentTheme;
        try
        {
            using (var fixture = new KeyboardFixture())
            {
                var active = new SourceGrid.Position(2, 1);
                Assert.That(fixture.Grid.Selection.Focus(active, true), Is.True);

                BootstrapThemeManager.CurrentTheme =
                    BootstrapTheme.CreateDefault(BootstrapThemeMode.Dark);

                Assert.That(fixture.Grid.Selection.ActivePosition, Is.EqualTo(active));
                PressSpecialKey(fixture.Grid, Keys.Right);
                Assert.That(
                    fixture.Grid.Selection.ActivePosition,
                    Is.EqualTo(new SourceGrid.Position(2, 2)));
            }
        }
        finally
        {
            BootstrapThemeManager.CurrentTheme = originalTheme;
        }
    }

    [Test]
    public void ArrowAndTabKeysRetainSourceGridNavigation()
    {
        using (var fixture = new KeyboardFixture())
        {
            Assert.That(
                fixture.Grid.Selection.Focus(new SourceGrid.Position(2, 1), true),
                Is.True);

            PressSpecialKey(fixture.Grid, Keys.Right);
            AssertActivePosition(fixture.Grid, 2, 2);
            PressSpecialKey(fixture.Grid, Keys.Left);
            AssertActivePosition(fixture.Grid, 2, 1);
            PressSpecialKey(fixture.Grid, Keys.Down);
            AssertActivePosition(fixture.Grid, 3, 1);
            PressSpecialKey(fixture.Grid, Keys.Up);
            AssertActivePosition(fixture.Grid, 2, 1);
            PressSpecialKey(fixture.Grid, Keys.Tab);
            AssertActivePosition(fixture.Grid, 2, 2);
            PressSpecialKey(fixture.Grid, Keys.Tab | Keys.Shift);
            AssertActivePosition(fixture.Grid, 2, 1);
        }
    }

    [Test]
    public void PageKeysMoveWithinGridAndPreserveActiveColumn()
    {
        using (var fixture = new KeyboardFixture())
        {
            Assert.That(
                fixture.Grid.Selection.Focus(new SourceGrid.Position(0, 1), true),
                Is.True);

            var pageDown = PressSpecialKey(fixture.Grid, Keys.PageDown);
            var afterPageDown = fixture.Grid.Selection.ActivePosition;
            Assert.That(pageDown.Handled, Is.True);
            Assert.That(afterPageDown.Row, Is.GreaterThan(0));
            Assert.That(afterPageDown.Column, Is.EqualTo(1));

            var pageUp = PressSpecialKey(fixture.Grid, Keys.PageUp);
            var afterPageUp = fixture.Grid.Selection.ActivePosition;
            Assert.That(pageUp.Handled, Is.True);
            Assert.That(afterPageUp.Row, Is.LessThan(afterPageDown.Row));
            Assert.That(afterPageUp.Column, Is.EqualTo(1));
        }
    }

    [Test]
    public void F2StartsEditingAndEscapeCancelsThroughSourceGridControllers()
    {
        using (var fixture = new KeyboardFixture())
        {
            var position = new SourceGrid.Position(1, 1);
            Assert.That(fixture.Grid.Selection.Focus(position, true), Is.True);
            var cell = fixture.Grid.GetCell(position);
            var context = new SourceGrid.CellContext(fixture.Grid, position, cell);
            var editor = (SourceGrid.Cells.Editors.TextBox)cell.Editor;
            var f2 = new KeyEventArgs(Keys.F2);

            fixture.Grid.Controller.OnKeyDown(context, f2);
            try
            {
                Assert.That(f2.Handled, Is.True);
                Assert.That(editor.IsEditing, Is.True);
                editor.SetEditValue("pending");

                var escape = PressSpecialKey(fixture.Grid, Keys.Escape);

                Assert.That(escape.Handled, Is.True);
                Assert.That(editor.IsEditing, Is.False);
                Assert.That(cell.Model.ValueModel.GetValue(context), Is.EqualTo("1:1"));
            }
            finally
            {
                context.EndEdit(true);
            }
        }
    }

    private static KeyEventArgs PressSpecialKey(
        BootstrapSourceGridControl grid,
        Keys keys)
    {
        var args = new KeyEventArgs(keys);
        grid.ProcessSpecialGridKey(args);
        return args;
    }

    private static void AssertActivePosition(
        BootstrapSourceGridControl grid,
        int row,
        int column)
    {
        Assert.That(
            grid.Selection.ActivePosition,
            Is.EqualTo(new SourceGrid.Position(row, column)));
    }

    private sealed class KeyboardFixture : System.IDisposable
    {
        internal KeyboardFixture()
        {
            Form = new Form
            {
                ClientSize = new Size(260, 110),
            };
            Grid = new BootstrapSourceGridControl
            {
                Dock = DockStyle.Fill,
            };
            Grid.Redim(5, 3);
            for (var row = 0; row < Grid.RowsCount; row++)
            {
                Grid.Rows[row].Height = 28;
                for (var column = 0; column < Grid.ColumnsCount; column++)
                {
                    Grid[row, column] = new SourceGrid.Cells.Cell(
                        $"{row}:{column}",
                        typeof(string));
                    Grid.GetCell(row, column);
                }
            }

            Form.Controls.Add(Grid);
            Form.Show();
            Grid.Focus();
        }

        internal Form Form { get; }

        internal BootstrapSourceGridControl Grid { get; }

        public void Dispose()
        {
            var active = Grid.Selection.ActivePosition;
            if (!active.IsEmpty())
            {
                new SourceGrid.CellContext(Grid, active).EndEdit(true);
            }

            Form.Close();
            Form.Dispose();
            Grid.Dispose();
        }
    }
}
