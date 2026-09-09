using System;
using System.Windows.Forms;
using MyDmsVn.Bootstrap5WinFormUI.Controls;
using MyDmsVn.BootstrapSourceGrid.Editors.Internal;

namespace MyDmsVn.BootstrapSourceGrid.Editors;

/// <summary>
/// Adapts a <see cref="BootstrapTextBox"/> to the SourceGrid editor lifecycle.
/// </summary>
public sealed class BootstrapTextBoxEditor : SourceGrid.Cells.Editors.EditorControlBase
{
    internal BootstrapTextBoxEditor(Type valueType)
        : base(valueType)
    {
        UseCellViewProperties = false;
    }

    /// <summary>
    /// Gets the Bootstrap text box used while a cell is being edited.
    /// </summary>
    public BootstrapTextBox BootstrapControl => (BootstrapSourceGridTextBoxControl)Control;

    /// <inheritdoc />
    protected override Control CreateControl()
    {
        return new BootstrapSourceGridTextBoxControl();
    }

    /// <inheritdoc />
    public override void SetEditValue(object editValue)
    {
        BootstrapControl.Text = IsStringConversionSupported()
            ? ValueToString(editValue)
            : ValueToDisplayString(editValue);
        ((BootstrapSourceGridTextBoxControl)Control).SelectAllForGridEdit();
    }

    /// <inheritdoc />
    public override object GetEditedValue()
    {
        return BootstrapControl.Text;
    }

    /// <inheritdoc />
    protected override void OnSendCharToEditor(char key)
    {
        ((BootstrapSourceGridTextBoxControl)Control).ReplaceWithFirstEditCharacter(key);
    }
}
