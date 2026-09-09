using System;
using System.Windows.Forms;
using MyDmsVn.Bootstrap5WinFormUI.Controls;
using MyDmsVn.BootstrapSourceGrid.Editors.Internal;

namespace MyDmsVn.BootstrapSourceGrid.Editors;

/// <summary>
/// Adapts a <see cref="BootstrapFormattedTextBox"/> to the SourceGrid editor lifecycle.
/// </summary>
public sealed class BootstrapFormattedTextBoxEditor : SourceGrid.Cells.Editors.EditorControlBase
{
    internal BootstrapFormattedTextBoxEditor(Type valueType)
        : base(valueType)
    {
        UseCellViewProperties = false;
    }

    /// <summary>
    /// Gets the Bootstrap formatted text box used while a cell is being edited.
    /// </summary>
    public BootstrapFormattedTextBox BootstrapControl =>
        (BootstrapSourceGridFormattedTextBoxControl)Control;

    /// <inheritdoc />
    protected override Control CreateControl()
    {
        return new BootstrapSourceGridFormattedTextBoxControl();
    }

    /// <inheritdoc />
    public override void SetEditValue(object editValue)
    {
        BootstrapControl.RawValue = IsStringConversionSupported()
            ? ValueToString(editValue)
            : ValueToDisplayString(editValue);
        ((BootstrapSourceGridFormattedTextBoxControl)Control).SelectAllForGridEdit();
    }

    /// <inheritdoc />
    public override object GetEditedValue()
    {
        return BootstrapControl.RawValue;
    }

    /// <inheritdoc />
    protected override void OnSendCharToEditor(char key)
    {
        ((BootstrapSourceGridFormattedTextBoxControl)Control).ReplaceWithFirstEditCharacter(key);
    }
}
