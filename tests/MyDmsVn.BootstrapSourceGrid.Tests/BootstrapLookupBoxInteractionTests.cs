using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using MyDmsVn.BootstrapSourceGrid.Editors;
using NUnit.Framework;
using BootstrapSourceGridControl = MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapSourceGrid;

namespace MyDmsVn.BootstrapSourceGrid.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class BootstrapLookupBoxInteractionTests
{
    [OneTimeSetUp]
    public void ConfigureWinForms()
    {
        WinFormsTestGuard.Configure();
    }

    [Test]
    public void OpeningPopupDoesNotEndSourceGridEdit()
    {
        using (var fixture = new LookupInteractionFixture())
        {
            fixture.StartEdit();

            fixture.Editor.BootstrapControl.OpenDropDown();
            Application.DoEvents();

            Assert.That(fixture.Editor.BootstrapControl.IsDropDownOpen, Is.True);
            Assert.That(fixture.Editor.IsEditing, Is.True);
            Assert.That(fixture.Cell.Value, Is.EqualTo(42));
            Assert.That(fixture.ValueChangedCount, Is.Zero);
        }
    }

    [Test]
    public void ResultGridFocusDoesNotCommitSourceGridEdit()
    {
        using (var fixture = new LookupInteractionFixture())
        {
            fixture.StartEdit();
            fixture.Editor.BootstrapControl.Text = string.Empty;
            fixture.Editor.BootstrapControl.OpenDropDown();
            Application.DoEvents();

            Assert.That(fixture.Editor.BootstrapControl.ResultsGrid.Focus(), Is.True);
            Application.DoEvents();

            Assert.That(fixture.Editor.BootstrapControl.IsDropDownOpen, Is.True);
            Assert.That(fixture.Editor.IsEditing, Is.True);
            Assert.That(fixture.Cell.Value, Is.EqualTo(42));
            Assert.That(fixture.ValueChangedCount, Is.Zero);
        }
    }

    [Test]
    public void ClosingPopupWithoutSelectionKeepsOriginalEditActive()
    {
        using (var fixture = new LookupInteractionFixture())
        {
            fixture.StartEdit();
            fixture.Editor.BootstrapControl.Text = string.Empty;
            fixture.Editor.BootstrapControl.OpenDropDown();
            Application.DoEvents();

            fixture.Editor.BootstrapControl.CloseDropDown();
            Application.DoEvents();

            Assert.That(fixture.Editor.BootstrapControl.IsDropDownOpen, Is.False);
            Assert.That(fixture.Editor.IsEditing, Is.True);
            Assert.That(fixture.Editor.BootstrapControl.SelectedValue, Is.EqualTo(42));
            Assert.That(fixture.Cell.Value, Is.EqualTo(42));
            Assert.That(fixture.ValueChangedCount, Is.Zero);
        }
    }

    [Test]
    public void EnterWithPopupClosedUsesSourceGridCommitPolicy()
    {
        using (var fixture = new LookupInteractionFixture())
        {
            fixture.StartEdit();
            fixture.Editor.BootstrapControl.SelectValue(7);

            Assert.That(fixture.DispatchEditorCommandKey(Keys.Enter), Is.True);

            Assert.That(fixture.Editor.IsEditing, Is.False);
            Assert.That(fixture.Cell.Value, Is.EqualTo(7));
            Assert.That(fixture.ValueChangedCount, Is.EqualTo(1));
        }
    }

    [Test]
    public void EnterWithHighlightedResultCommitsSelectedValueExactlyOnce()
    {
        using (var fixture = new LookupInteractionFixture())
        {
            fixture.StartEdit();
            fixture.Editor.BootstrapControl.Text = string.Empty;
            fixture.Editor.BootstrapControl.OpenDropDown();
            Assert.That(fixture.DispatchEditorCommandKey(Keys.Down), Is.True);
            Assert.That(((LookupItem)fixture.Editor.BootstrapControl.HighlightedItem!).Id, Is.EqualTo(7));

            Assert.That(fixture.DispatchEditorCommandKey(Keys.Enter), Is.True);
            Application.DoEvents();

            Assert.That(fixture.Editor.IsEditing, Is.False);
            Assert.That(fixture.Cell.Value, Is.EqualTo(7));
            Assert.That(fixture.ValueChangedCount, Is.EqualTo(1));
        }
    }

    [Test]
    public void EscapeWithPopupOpenCancelsPendingLookupOnly()
    {
        using (var fixture = new LookupInteractionFixture())
        {
            fixture.StartEdit();
            fixture.Editor.BootstrapControl.Text = "Cont";
            fixture.Editor.BootstrapControl.OpenDropDown();

            Assert.That(fixture.DispatchEditorCommandKey(Keys.Escape), Is.True);

            Assert.That(fixture.Editor.IsEditing, Is.True);
            Assert.That(fixture.Editor.BootstrapControl.IsDropDownOpen, Is.False);
            Assert.That(fixture.Editor.BootstrapControl.HasPendingText, Is.False);
            Assert.That(fixture.Editor.BootstrapControl.SelectedValue, Is.EqualTo(42));
            Assert.That(fixture.Cell.Value, Is.EqualTo(42));
            Assert.That(fixture.ValueChangedCount, Is.Zero);
        }
    }

    [Test]
    public void EscapeWithPopupClosedUsesSourceGridCancelPolicy()
    {
        using (var fixture = new LookupInteractionFixture())
        {
            fixture.StartEdit();
            fixture.Editor.BootstrapControl.SelectValue(7);

            Assert.That(fixture.DispatchEditorCommandKey(Keys.Escape), Is.True);

            Assert.That(fixture.Editor.IsEditing, Is.False);
            Assert.That(fixture.Cell.Value, Is.EqualTo(42));
            Assert.That(fixture.Editor.BootstrapControl.SelectedValue, Is.EqualTo(42));
            Assert.That(fixture.ValueChangedCount, Is.Zero);
        }
    }

    [Test]
    public void TabCommitsOnceAndMovesToNextEditableCell()
    {
        using (var fixture = new LookupInteractionFixture())
        {
            fixture.StartEdit();
            fixture.Editor.BootstrapControl.SelectValue(7);

            Assert.That(fixture.DispatchEditorCommandKey(Keys.Tab), Is.True);

            Assert.That(fixture.Editor.IsEditing, Is.False);
            Assert.That(fixture.Cell.Value, Is.EqualTo(7));
            Assert.That(fixture.ValueChangedCount, Is.EqualTo(1));
            Assert.That(fixture.Grid.Selection.ActivePosition, Is.EqualTo(new SourceGrid.Position(1, 0)));
        }
    }

    [Test]
    public void ShiftTabCommitsOnceAndMovesToPreviousEditableCell()
    {
        using (var fixture = new LookupInteractionFixture())
        {
            fixture.StartEdit(1);
            fixture.Editor.BootstrapControl.SelectValue(42);

            Assert.That(fixture.DispatchEditorCommandKey(Keys.Shift | Keys.Tab), Is.True);

            Assert.That(fixture.Editor.IsEditing, Is.False);
            Assert.That(fixture.Cells[1].Value, Is.EqualTo(42));
            Assert.That(fixture.ValueChangedCount, Is.EqualTo(1));
            Assert.That(fixture.Grid.Selection.ActivePosition, Is.EqualTo(new SourceGrid.Position(0, 0)));
        }
    }

    [TestCase(Keys.Down, 7)]
    [TestCase(Keys.PageDown, 7)]
    public void PopupResultNavigationDoesNotMoveSourceGridActiveCell(Keys key, int highlightedId)
    {
        using (var fixture = new LookupInteractionFixture())
        {
            fixture.StartEdit();
            fixture.Editor.BootstrapControl.Text = string.Empty;
            fixture.Editor.BootstrapControl.OpenDropDown();

            Assert.That(fixture.DispatchEditorCommandKey(key), Is.True);

            Assert.That(((LookupItem)fixture.Editor.BootstrapControl.HighlightedItem!).Id, Is.EqualTo(highlightedId));
            Assert.That(fixture.Grid.Selection.ActivePosition, Is.EqualTo(new SourceGrid.Position(0, 0)));
            Assert.That(fixture.Editor.IsEditing, Is.True);
        }
    }

    [Test]
    public void SourceGridNavigationResumesAfterLookupCommit()
    {
        using (var fixture = new LookupInteractionFixture())
        {
            fixture.StartEdit();
            fixture.Editor.BootstrapControl.Text = string.Empty;
            fixture.Editor.BootstrapControl.OpenDropDown();
            fixture.DispatchEditorCommandKey(Keys.Down);
            fixture.DispatchEditorCommandKey(Keys.Enter);

            var navigation = fixture.Grid.DispatchSpecialKey(Keys.Down);

            Assert.That(navigation.Handled, Is.True);
            Assert.That(fixture.Grid.Selection.ActivePosition, Is.EqualTo(new SourceGrid.Position(1, 0)));
        }
    }

    private sealed class LookupInteractionFixture : IDisposable
    {
        private readonly SourceGrid.Cells.Controllers.CustomEvents _events;

        internal LookupInteractionFixture()
        {
            Form = new Form
            {
                ClientSize = new Size(320, 150),
                ShowInTaskbar = false,
            };
            Grid = new LookupInteractionGrid
            {
                Dock = DockStyle.Top,
                Height = 80,
            };
            Editor = Grid.EditorRegistry.Register(new BootstrapLookupBoxEditor(typeof(int)));
            Editor.BootstrapControl.DisplayMember = nameof(LookupItem.Name);
            Editor.BootstrapControl.ValueMember = nameof(LookupItem.Id);
            Editor.BootstrapControl.DataSource = new BindingList<LookupItem>
            {
                new LookupItem(42, "Northwind"),
                new LookupItem(7, "Contoso"),
            };
            Editor.BootstrapControl.SearchDebounceMilliseconds = 0;
            Cell = new SourceGrid.Cells.Cell(42, typeof(int)) { Editor = Editor };
            Cells = new[]
            {
                Cell,
                new SourceGrid.Cells.Cell(7, typeof(int)) { Editor = Editor },
            };
            _events = new SourceGrid.Cells.Controllers.CustomEvents();
            _events.ValueChanged += OnValueChanged;
            Cells[0].AddController(_events);
            Cells[1].AddController(_events);
            Grid.Redim(2, 1);
            Grid.Rows[0].Height = 32;
            Grid.Rows[1].Height = 32;
            Grid[0, 0] = Cell;
            Grid[1, 0] = Cells[1];
            FocusTarget = new Button
            {
                Dock = DockStyle.Bottom,
                Text = "Outside",
            };
            Form.Controls.Add(Grid);
            Form.Controls.Add(FocusTarget);
            Form.Show();
            Grid.Focus();
            Application.DoEvents();
        }

        internal Form Form { get; }

        internal LookupInteractionGrid Grid { get; }

        internal Button FocusTarget { get; }

        internal BootstrapLookupBoxEditor Editor { get; }

        internal SourceGrid.Cells.Cell Cell { get; }

        internal SourceGrid.Cells.Cell[] Cells { get; }

        internal int ValueChangedCount { get; private set; }

        internal SourceGrid.CellContext StartEdit(int row = 0)
        {
            var position = new SourceGrid.Position(row, 0);
            Assert.That(Grid.Selection.Focus(position, true), Is.True);
            var context = new SourceGrid.CellContext(Grid, position, Cells[row]);
            Grid.GetCell(position).View.Measure(context, Size.Empty);
            context.StartEdit();
            Application.DoEvents();
            Assert.That(Editor.IsEditing, Is.True);
            return context;
        }

        internal bool DispatchEditorCommandKey(Keys keyData)
        {
            var message = new Message();
            var arguments = new object[] { message, keyData };
            var processCmdKey = typeof(MyDmsVn.Bootstrap5WinFormUI.Controls.BootstrapLookupBox).GetMethod(
                "ProcessCmdKey",
                BindingFlags.Instance | BindingFlags.NonPublic)!;
            var handledByLookup = (bool)processCmdKey.Invoke(Editor.BootstrapControl, arguments)!;
            return handledByLookup || Grid.DispatchCommandKey(keyData);
        }

        public void Dispose()
        {
            if (Editor.IsEditing)
            {
                Editor.EditCellContext.EndEdit(true);
            }

            Cells[0].RemoveController(_events);
            Cells[1].RemoveController(_events);
            Form.Close();
            Form.Dispose();
            Grid.Dispose();
        }

        private void OnValueChanged(object? sender, EventArgs e)
        {
            ValueChangedCount++;
        }
    }

    private sealed class LookupInteractionGrid : BootstrapSourceGridControl
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

    private sealed class LookupItem
    {
        internal LookupItem(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }
    }
}
