using System;
using System.ComponentModel;
using System.Drawing;
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
            _events = new SourceGrid.Cells.Controllers.CustomEvents();
            _events.ValueChanged += OnValueChanged;
            Cell.AddController(_events);
            Grid.Redim(2, 1);
            Grid.Rows[0].Height = 32;
            Grid.Rows[1].Height = 32;
            Grid[0, 0] = Cell;
            Grid[1, 0] = new SourceGrid.Cells.Cell(7, typeof(int)) { Editor = Editor };
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

        internal int ValueChangedCount { get; private set; }

        internal SourceGrid.CellContext StartEdit()
        {
            var position = new SourceGrid.Position(0, 0);
            Assert.That(Grid.Selection.Focus(position, true), Is.True);
            var context = new SourceGrid.CellContext(Grid, position, Cell);
            Grid.GetCell(position).View.Measure(context, Size.Empty);
            context.StartEdit();
            Application.DoEvents();
            Assert.That(Editor.IsEditing, Is.True);
            return context;
        }

        public void Dispose()
        {
            if (Editor.IsEditing)
            {
                Editor.EditCellContext.EndEdit(true);
            }

            Cell.RemoveController(_events);
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
