using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using MyDmsVn.BootstrapSourceGrid.Editors;
using NUnit.Framework;
using BootstrapSourceGridControl = MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid;

namespace MyDmsVn.BootstrapSourceGrid.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class BootstrapTextBoxEditorTests
{
    [OneTimeSetUp]
    public void ConfigureWinForms()
    {
        WinFormsTestGuard.Configure();
    }

    [Test]
    public void BeginEditSelectsAllText()
    {
        using (var fixture = new TextEditorFixture("first", "second"))
        {
            var context = fixture.StartEdit(0);

            try
            {
                var nativeEditor = fixture.Editor.BootstrapControl.Controls
                    .OfType<TextBox>()
                    .Single();

                Assert.That(nativeEditor.SelectionStart, Is.Zero);
                Assert.That(nativeEditor.SelectionLength, Is.EqualTo("first".Length));
            }
            finally
            {
                context.EndEdit(true);
            }
        }
    }

    [Test]
    public void SendFirstCharacterReplacesExistingText()
    {
        using (var fixture = new TextEditorFixture("first", "second"))
        {
            var context = fixture.StartEdit(0);

            try
            {
                fixture.Editor.SendCharToEditor('x');

                Assert.That(fixture.Editor.BootstrapControl.Text, Is.EqualTo("x"));
            }
            finally
            {
                context.EndEdit(true);
            }
        }
    }

    [Test]
    public void SendFirstCharacterPlacesCaretAfterCharacter()
    {
        using (var fixture = new TextEditorFixture("first", "second"))
        {
            var context = fixture.StartEdit(0);

            try
            {
                fixture.Editor.SendCharToEditor('x');
                var nativeEditor = fixture.Editor.BootstrapControl.Controls
                    .OfType<TextBox>()
                    .Single();

                Assert.That(nativeEditor.SelectionStart, Is.EqualTo(1));
                Assert.That(nativeEditor.SelectionLength, Is.Zero);
            }
            finally
            {
                context.EndEdit(true);
            }
        }
    }

    [Test]
    public void CancelRestoresOriginalValue()
    {
        using (var fixture = new TextEditorFixture("first", "second"))
        {
            var context = fixture.StartEdit(0);
            fixture.Editor.BootstrapControl.Text = "changed";

            Assert.That(context.EndEdit(true), Is.True);
            Assert.That(fixture.Cells[0].Value, Is.EqualTo("first"));
            Assert.That(fixture.Editor.BootstrapControl.Text, Is.EqualTo("first"));
        }
    }

    [Test]
    public void CommitStoresEditedValue()
    {
        using (var fixture = new TextEditorFixture("first", "second"))
        {
            var firstContext = fixture.StartEdit(0);
            fixture.Editor.BootstrapControl.Text = "first updated";
            Assert.That(firstContext.EndEdit(false), Is.True);

            var secondContext = fixture.StartEdit(1);
            fixture.Editor.BootstrapControl.Text = "second updated";
            Assert.That(secondContext.EndEdit(false), Is.True);

            Assert.That(fixture.Cells[0].Value, Is.EqualTo("first updated"));
            Assert.That(fixture.Cells[1].Value, Is.EqualTo("second updated"));
            Assert.That(fixture.Cells[0].Editor, Is.SameAs(fixture.Editor));
            Assert.That(fixture.Cells[1].Editor, Is.SameAs(fixture.Editor));
        }
    }

    private sealed class TextEditorFixture : System.IDisposable
    {
        internal TextEditorFixture(string firstValue, string secondValue)
        {
            Form = new Form
            {
                ClientSize = new Size(240, 90),
            };
            Grid = new BootstrapSourceGridControl
            {
                Dock = DockStyle.Fill,
            };
            Editor = Grid.EditorRegistry.Register(new BootstrapTextBoxEditor(typeof(string)));
            Cells = new[]
            {
                new SourceGrid.Cells.Cell(firstValue, typeof(string)) { Editor = Editor },
                new SourceGrid.Cells.Cell(secondValue, typeof(string)) { Editor = Editor },
            };

            Grid.Redim(2, 1);
            Grid.Rows[0].Height = 30;
            Grid.Rows[1].Height = 30;
            Grid[0, 0] = Cells[0];
            Grid[1, 0] = Cells[1];
            Form.Controls.Add(Grid);
            Form.Show();
            Grid.Focus();
        }

        internal Form Form { get; }

        internal BootstrapSourceGridControl Grid { get; }

        internal BootstrapTextBoxEditor Editor { get; }

        internal SourceGrid.Cells.Cell[] Cells { get; }

        internal SourceGrid.CellContext StartEdit(int row)
        {
            var position = new SourceGrid.Position(row, 0);
            Assert.That(Grid.Selection.Focus(position, true), Is.True);
            var context = new SourceGrid.CellContext(Grid, position, Cells[row]);
            Grid.GetCell(position).View.Measure(context, Size.Empty);
            context.StartEdit();
            Assert.That(Editor.IsEditing, Is.True);
            return context;
        }

        public void Dispose()
        {
            if (Editor.IsEditing)
            {
                Editor.EditCellContext.EndEdit(true);
            }

            Form.Close();
            Form.Dispose();
            Grid.Dispose();
        }
    }
}
