using System;
using System.Windows.Forms;
using MyDmsVn.Bootstrap5WinFormUI.Controls;
using MyDmsVn.BootstrapSourceGrid.Editors.Internal;

namespace MyDmsVn.BootstrapSourceGrid.Editors;

/// <summary>
/// Adapts a <see cref="BootstrapLookupBox"/> to the SourceGrid editor lifecycle.
/// </summary>
public sealed class BootstrapLookupBoxEditor : SourceGrid.Cells.Editors.EditorControlBase
{
    internal BootstrapLookupBoxEditor(Type valueType)
        : base(valueType)
    {
        UseCellViewProperties = false;
    }

    /// <summary>
    /// Gets the Bootstrap lookup box used while a cell is being edited.
    /// </summary>
    public BootstrapLookupBox BootstrapControl =>
        (BootstrapSourceGridLookupBoxControl)Control;

    /// <inheritdoc />
    protected override Control CreateControl()
    {
        return new BootstrapSourceGridLookupBoxControl();
    }

    /// <inheritdoc />
    public override void SetEditValue(object editValue)
    {
        BootstrapControl.SelectedValue = editValue;
    }

    /// <inheritdoc />
    public override object GetEditedValue()
    {
        return BootstrapControl.SelectedValue!;
    }

    /// <inheritdoc />
    protected override void OnSendCharToEditor(char key)
    {
        ((BootstrapSourceGridLookupBoxControl)Control).ReplaceWithFirstEditCharacter(key);
    }
}
