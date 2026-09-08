using System.Threading;
using System.Windows.Forms;
using MyDmsVn.Bootstrap5WinFormUI.Theme;
using NUnit.Framework;
using BootstrapSourceGridControl = MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid;

namespace MyDmsVn.BootstrapSourceGrid.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class BootstrapSourceGridEditLifecycleTests
{
    [OneTimeSetUp]
    public void ConfigureWinForms()
    {
        WinFormsTestGuard.Configure();
    }

    [Test]
    public void EndEditWithoutCancelCommitsTextEditorValue()
    {
        using (var fixture = new EditorFixture("before"))
        {
            fixture.Context.StartEdit();
            try
            {
                fixture.Editor.SetEditValue("after");

                Assert.That(fixture.Context.EndEdit(false), Is.True);
                Assert.That(fixture.Cell.Value, Is.EqualTo("after"));
                Assert.That(fixture.Editor.IsEditing, Is.False);
            }
            finally
            {
                fixture.Context.EndEdit(true);
            }
        }
    }

    [Test]
    public void EndEditWithCancelPreservesOriginalCellValue()
    {
        using (var fixture = new EditorFixture("before"))
        {
            fixture.Context.StartEdit();
            try
            {
                fixture.Editor.SetEditValue("after");

                Assert.That(fixture.Context.EndEdit(true), Is.True);
                Assert.That(fixture.Cell.Value, Is.EqualTo("before"));
                Assert.That(fixture.Editor.IsEditing, Is.False);
            }
            finally
            {
                fixture.Context.EndEdit(true);
            }
        }
    }

    [Test]
    public void ThemeChangeDoesNotCommitOrCancelPendingEdit()
    {
        var originalTheme = BootstrapThemeManager.CurrentTheme;
        try
        {
            BootstrapThemeManager.CurrentTheme = BootstrapTheme.CreateDefault(BootstrapThemeMode.Light);
            using (var fixture = new EditorFixture("before"))
            {
                fixture.Context.StartEdit();
                try
                {
                    fixture.Editor.SetEditValue("pending");
                    BootstrapThemeManager.CurrentTheme =
                        BootstrapTheme.CreateDefault(BootstrapThemeMode.Dark);

                    Assert.That(fixture.Editor.IsEditing, Is.True);
                    Assert.That(fixture.Editor.GetEditedValue(), Is.EqualTo("pending"));
                    Assert.That(fixture.Cell.Value, Is.EqualTo("before"));
                }
                finally
                {
                    fixture.Context.EndEdit(true);
                }
            }
        }
        finally
        {
            BootstrapThemeManager.CurrentTheme = originalTheme;
        }
    }

    [Test]
    public void DisabledEditorCannotEnterEditStateAfterTheming()
    {
        var originalTheme = BootstrapThemeManager.CurrentTheme;
        try
        {
            using (var fixture = new EditorFixture("before"))
            {
                fixture.Editor.EnableEdit = false;

                BootstrapThemeManager.CurrentTheme =
                    BootstrapTheme.CreateDefault(BootstrapThemeMode.Dark);
                fixture.Context.StartEdit();

                Assert.That(fixture.Editor.EnableEdit, Is.False);
                Assert.That(fixture.Editor.IsEditing, Is.False);
                Assert.That(fixture.Cell.Value, Is.EqualTo("before"));
            }
        }
        finally
        {
            BootstrapThemeManager.CurrentTheme = originalTheme;
        }
    }

    [Test]
    public void CoveredSpanPositionStartsEditingAtCanonicalCellAndSupportsCommitAndCancel()
    {
        using (var form = new Form())
        using (var grid = new EditLifecycleTestGrid())
        {
            var cell = new SourceGrid.Cells.Cell("before", typeof(string))
            {
                ColumnSpan = 2,
            };
            var covered = new SourceGrid.Position(0, 1);
            var canonical = new SourceGrid.Position(0, 0);
            grid.Redim(1, 2);
            grid[0, 0] = cell;
            form.Controls.Add(grid);
            form.Show();
            Assert.That(grid.Selection.Focus(covered, true), Is.True);

            var canonicalContext = new SourceGrid.CellContext(grid, canonical);
            var editor = (SourceGrid.Cells.Editors.TextBox)cell.Editor;
            grid.DispatchKeyDown(Keys.F2);

            Assert.That(editor.IsEditing, Is.True);
            Assert.That(editor.EditPosition, Is.EqualTo(canonical));
            editor.SetEditValue("committed");
            Assert.That(canonicalContext.EndEdit(false), Is.True);
            Assert.That(cell.Value, Is.EqualTo("committed"));

            Assert.That(grid.Selection.Focus(covered, true), Is.True);
            grid.DispatchKeyDown(Keys.F2);
            editor.SetEditValue("cancelled");
            Assert.That(canonicalContext.EndEdit(true), Is.True);
            Assert.That(cell.Value, Is.EqualTo("committed"));
        }
    }

    [Test]
    public void CoveredSpanPositionStartsAnyKeyEditingAtCanonicalCell()
    {
        using (var form = new Form())
        using (var grid = new EditLifecycleTestGrid())
        {
            var cell = new SourceGrid.Cells.Cell("before", typeof(string))
            {
                ColumnSpan = 2,
            };
            var covered = new SourceGrid.Position(0, 1);
            var canonical = new SourceGrid.Position(0, 0);
            grid.Redim(1, 2);
            grid[0, 0] = cell;
            form.Controls.Add(grid);
            form.Show();
            Assert.That(grid.Selection.Focus(covered, true), Is.True);

            var canonicalContext = new SourceGrid.CellContext(grid, canonical);
            var editor = (SourceGrid.Cells.Editors.TextBox)cell.Editor;
            grid.DispatchKeyDown(Keys.X);
            grid.DispatchKeyPress('x');

            try
            {
                Assert.That(editor.IsEditing, Is.True);
                Assert.That(editor.EditPosition, Is.EqualTo(canonical));
                Assert.That(canonicalContext.IsEditing(), Is.True);
            }
            finally
            {
                canonicalContext.EndEdit(true);
            }
        }
    }

    private sealed class EditLifecycleTestGrid : BootstrapSourceGridControl
    {
        internal void DispatchKeyDown(Keys keys)
        {
            base.OnKeyDown(new KeyEventArgs(keys));
        }

        internal void DispatchKeyPress(char keyChar)
        {
            base.OnKeyPress(new KeyPressEventArgs(keyChar));
        }
    }

    private sealed class EditorFixture : System.IDisposable
    {
        internal EditorFixture(string value)
        {
            Form = new Form();
            Grid = new BootstrapSourceGridControl();
            Cell = new SourceGrid.Cells.Cell(value, typeof(string));
            Grid.Redim(1, 1);
            Grid[0, 0] = Cell;
            Form.Controls.Add(Grid);
            Form.Show();
            Context = new SourceGrid.CellContext(
                Grid,
                new SourceGrid.Position(0, 0),
                Cell);
            Grid.GetCell(0, 0);
            Editor = (SourceGrid.Cells.Editors.TextBox)Cell.Editor;
        }

        internal Form Form { get; }

        internal BootstrapSourceGridControl Grid { get; }

        internal SourceGrid.Cells.Cell Cell { get; }

        internal SourceGrid.CellContext Context { get; }

        internal SourceGrid.Cells.Editors.TextBox Editor { get; }

        public void Dispose()
        {
            Context.EndEdit(true);
            Form.Close();
            Form.Dispose();
            Grid.Dispose();
        }
    }
}
