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
