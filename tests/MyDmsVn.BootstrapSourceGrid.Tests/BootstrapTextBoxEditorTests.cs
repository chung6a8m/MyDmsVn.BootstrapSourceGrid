using System;
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

    [Test]
    public void EnterCommitsAccordingToSourceGridPolicy()
    {
        using (var fixture = new TextEditorFixture("first", "second"))
        {
            fixture.StartEdit(0);
            fixture.Editor.BootstrapControl.Text = "committed";

            Assert.That(fixture.Grid.DispatchCommandKey(Keys.Enter), Is.True);

            Assert.That(fixture.Editor.IsEditing, Is.False);
            Assert.That(fixture.Cells[0].Value, Is.EqualTo("committed"));
            Assert.That(fixture.Grid.Selection.ActivePosition, Is.EqualTo(new SourceGrid.Position(0, 0)));
        }
    }

    [Test]
    public void EscapeCancelsAndRestoresOriginalValue()
    {
        using (var fixture = new TextEditorFixture("first", "second"))
        {
            fixture.StartEdit(0);
            fixture.Editor.BootstrapControl.Text = "pending";

            Assert.That(fixture.Grid.DispatchCommandKey(Keys.Escape), Is.True);

            Assert.That(fixture.Editor.IsEditing, Is.False);
            Assert.That(fixture.Cells[0].Value, Is.EqualTo("first"));
            Assert.That(fixture.Editor.BootstrapControl.Text, Is.EqualTo("first"));
        }
    }

    [Test]
    public void TabCommitsAndMovesToNextEditableCell()
    {
        using (var fixture = new TextEditorFixture("first", "second"))
        {
            fixture.StartEdit(0);
            fixture.Editor.BootstrapControl.Text = "committed";

            Assert.That(fixture.Grid.DispatchCommandKey(Keys.Tab), Is.True);

            Assert.That(fixture.Editor.IsEditing, Is.False);
            Assert.That(fixture.Cells[0].Value, Is.EqualTo("committed"));
            Assert.That(fixture.Grid.Selection.ActivePosition, Is.EqualTo(new SourceGrid.Position(1, 0)));
        }
    }

    [Test]
    public void ShiftTabCommitsAndMovesToPreviousEditableCell()
    {
        using (var fixture = new TextEditorFixture("first", "second"))
        {
            fixture.StartEdit(1);
            fixture.Editor.BootstrapControl.Text = "committed";
            fixture.Grid.DispatchCommandKey(Keys.Shift | Keys.ShiftKey);

            Assert.That(fixture.Grid.DispatchCommandKey(Keys.Shift | Keys.Tab), Is.True);

            Assert.That(fixture.Editor.IsEditing, Is.False);
            Assert.That(fixture.Cells[1].Value, Is.EqualTo("committed"));
            Assert.That(fixture.Grid.Selection.ActivePosition, Is.EqualTo(new SourceGrid.Position(0, 0)));
        }
    }

    [Test]
    public void ArrowNavigationAfterCommitMatchesSourceGrid()
    {
        using (var fixture = new TextEditorFixture("first", "second"))
        {
            fixture.StartEdit(0);
            fixture.Editor.BootstrapControl.Text = "committed";
            fixture.Grid.DispatchCommandKey(Keys.Enter);

            var right = fixture.Grid.DispatchSpecialKey(Keys.Down);

            Assert.That(right.Handled, Is.True);
            Assert.That(fixture.Grid.Selection.ActivePosition, Is.EqualTo(new SourceGrid.Position(1, 0)));
        }
    }

    [Test]
    public void ValidNumericTextUsesSourceGridTypedConversion()
    {
        using (var fixture = new TextEditorFixture(1, 2, typeof(int)))
        {
            var context = fixture.StartEdit(0);
            fixture.Editor.BootstrapControl.Text = "42";

            Assert.That(context.EndEdit(false), Is.True);
            Assert.That(fixture.Cells[0].Value, Is.TypeOf<int>());
            Assert.That(fixture.Cells[0].Value, Is.EqualTo(42));
        }
    }

    [Test]
    public void InvalidNumericTextIsRejectedBySourceGridConversion()
    {
        using (var fixture = new TextEditorFixture(1, 2, typeof(int)))
        {
            var editExceptionCount = 0;
            fixture.Editor.EditException += (_, _) => editExceptionCount++;
            var context = fixture.StartEdit(0);
            fixture.Editor.BootstrapControl.Text = "not-a-number";

            Assert.That(context.EndEdit(false), Is.False);
            Assert.That(fixture.Editor.IsEditing, Is.True);
            Assert.That(fixture.Cells[0].Value, Is.EqualTo(1));
            Assert.That(editExceptionCount, Is.EqualTo(1));
        }
    }

    [Test]
    public void ControlValidatedCommitsExactlyOnce()
    {
        using (var fixture = new TextEditorFixture("first", "second"))
        {
            var valueChangedCount = 0;
            var events = new SourceGrid.Cells.Controllers.CustomEvents();
            events.ValueChanged += (_, _) => valueChangedCount++;
            fixture.Cells[0].AddController(events);
            fixture.StartEdit(0);
            fixture.Editor.BootstrapControl.Text = "committed by validation";

            Assert.That(fixture.FocusTarget.Focus(), Is.True);
            Application.DoEvents();

            Assert.That(fixture.Editor.IsEditing, Is.False);
            Assert.That(fixture.Cells[0].Value, Is.EqualTo("committed by validation"));
            Assert.That(valueChangedCount, Is.EqualTo(1));
        }
    }

    private sealed class TextEditorFixture : System.IDisposable
    {
        internal TextEditorFixture(object firstValue, object secondValue, Type? valueType = null)
        {
            Form = new Form
            {
                ClientSize = new Size(240, 90),
            };
            Grid = new TextEditorTestGrid
            {
                Dock = DockStyle.Top,
                Height = 60,
            };
            var declaredType = valueType ?? typeof(string);
            Editor = Grid.EditorRegistry.Register(new BootstrapTextBoxEditor(declaredType));
            Cells = new[]
            {
                new SourceGrid.Cells.Cell(firstValue, declaredType) { Editor = Editor },
                new SourceGrid.Cells.Cell(secondValue, declaredType) { Editor = Editor },
            };

            Grid.Redim(2, 1);
            Grid.Rows[0].Height = 30;
            Grid.Rows[1].Height = 30;
            Grid[0, 0] = Cells[0];
            Grid[1, 0] = Cells[1];
            FocusTarget = new Button
            {
                Dock = DockStyle.Bottom,
                Text = "Focus target",
            };
            Form.Controls.Add(Grid);
            Form.Controls.Add(FocusTarget);
            Form.Show();
            Grid.Focus();
        }

        internal Form Form { get; }

        internal TextEditorTestGrid Grid { get; }

        internal Button FocusTarget { get; }

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

    private sealed class TextEditorTestGrid : BootstrapSourceGridControl
    {
        internal bool DispatchCommandKey(Keys keys)
        {
            var message = new Message();
            return base.ProcessCmdKey(ref message, keys);
        }

        internal KeyEventArgs DispatchSpecialKey(Keys keys)
        {
            var args = new KeyEventArgs(keys);
            ProcessSpecialGridKey(args);
            return args;
        }
    }
}
