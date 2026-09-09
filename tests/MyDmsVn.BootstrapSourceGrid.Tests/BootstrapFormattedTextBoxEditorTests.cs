using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using MyDmsVn.Bootstrap5WinFormUI.Formatting;
using MyDmsVn.BootstrapSourceGrid.Editors;
using NUnit.Framework;
using BootstrapSourceGridControl = MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid;

namespace MyDmsVn.BootstrapSourceGrid.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class BootstrapFormattedTextBoxEditorTests
{
    [OneTimeSetUp]
    public void ConfigureWinForms()
    {
        WinFormsTestGuard.Configure();
    }

    [Test]
    public void SetEditValueInitializesRawValue()
    {
        using (var fixture = new FormattedEditorFixture("12345678", "87654321"))
        {
            fixture.Editor.BootstrapControl.FormatMode = BootstrapInputFormatMode.General;
            fixture.Editor.BootstrapControl.GeneralOptions.Blocks = new[] { 4, 4 };

            fixture.Editor.SetEditValue("12345678");

            Assert.That(fixture.Editor.BootstrapControl.RawValue, Is.EqualTo("12345678"));
            Assert.That(fixture.Editor.BootstrapControl.Text, Is.EqualTo("1234 5678"));
        }
    }

    [Test]
    public void GetEditedValueReturnsRawValueNotFormattedText()
    {
        using (var fixture = new FormattedEditorFixture("12345678", "87654321"))
        {
            fixture.Editor.BootstrapControl.FormatMode = BootstrapInputFormatMode.General;
            fixture.Editor.BootstrapControl.GeneralOptions.Blocks = new[] { 4, 4 };
            fixture.Editor.BootstrapControl.RawValue = "12345678";

            Assert.That(fixture.Editor.BootstrapControl.Text, Is.EqualTo("1234 5678"));
            Assert.That(fixture.Editor.GetEditedValue(), Is.EqualTo("12345678"));
        }
    }

    [Test]
    public void CommitUsesSourceGridTypedConversion()
    {
        using (var fixture = new FormattedEditorFixture(1, 2, typeof(int)))
        {
            fixture.Editor.BootstrapControl.FormatMode = BootstrapInputFormatMode.Numeral;
            var context = fixture.StartEdit(0);
            fixture.Editor.BootstrapControl.RawValue = "42";

            Assert.That(context.EndEdit(false), Is.True);
            Assert.That(fixture.Cells[0].Value, Is.TypeOf<int>());
            Assert.That(fixture.Cells[0].Value, Is.EqualTo(42));
        }
    }

    [Test]
    public void CancelRestoresOriginalLogicalValue()
    {
        using (var fixture = new FormattedEditorFixture("12345678", "87654321"))
        {
            fixture.Editor.BootstrapControl.FormatMode = BootstrapInputFormatMode.General;
            fixture.Editor.BootstrapControl.GeneralOptions.Blocks = new[] { 4, 4 };
            var context = fixture.StartEdit(0);
            fixture.Editor.BootstrapControl.RawValue = "99998888";

            Assert.That(context.EndEdit(true), Is.True);
            Assert.That(fixture.Cells[0].Value, Is.EqualTo("12345678"));
            Assert.That(fixture.Editor.BootstrapControl.RawValue, Is.EqualTo("12345678"));
            Assert.That(fixture.Editor.BootstrapControl.Text, Is.EqualTo("1234 5678"));
        }
    }

    [Test]
    public void SendFirstCharacterUsesFormattedInputSemantics()
    {
        using (var fixture = new FormattedEditorFixture("12345678", "87654321"))
        {
            fixture.Editor.BootstrapControl.FormatMode = BootstrapInputFormatMode.General;
            fixture.Editor.BootstrapControl.GeneralOptions.Blocks = new[] { 4, 4 };
            var context = fixture.StartEdit(0);

            try
            {
                fixture.Editor.SendCharToEditor('9');

                Assert.That(fixture.Editor.BootstrapControl.RawValue, Is.EqualTo("9"));
                Assert.That(fixture.Editor.BootstrapControl.Text, Is.EqualTo("9"));
            }
            finally
            {
                context.EndEdit(true);
            }
        }
    }

    private sealed class FormattedEditorFixture : IDisposable
    {
        internal FormattedEditorFixture(object firstValue, object secondValue, Type? valueType = null)
        {
            Form = new Form
            {
                ClientSize = new Size(240, 90),
            };
            Grid = new BootstrapSourceGridControl
            {
                Dock = DockStyle.Fill,
            };
            var declaredType = valueType ?? typeof(string);
            Editor = Grid.EditorRegistry.Register(new BootstrapFormattedTextBoxEditor(declaredType));
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
            Form.Controls.Add(Grid);
            Form.Show();
            Grid.Focus();
        }

        internal Form Form { get; }

        internal BootstrapSourceGridControl Grid { get; }

        internal BootstrapFormattedTextBoxEditor Editor { get; }

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
