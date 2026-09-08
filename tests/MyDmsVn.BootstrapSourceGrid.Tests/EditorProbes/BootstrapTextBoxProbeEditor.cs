using System;
using System.Windows.Forms;
using MyDmsVn.Bootstrap5WinFormUI.Controls;

namespace MyDmsVn.BootstrapSourceGrid.Tests.EditorProbes;

internal class BootstrapTextBoxProbeEditor : SourceGrid.Cells.Editors.EditorControlBase
{
    internal BootstrapTextBoxProbeEditor()
        : base(typeof(string))
    {
        UseCellViewProperties = false;
    }

    internal BootstrapTextBox BootstrapControl => (BootstrapTextBox)Control;

    protected override Control CreateControl()
    {
        return new BootstrapTextBox();
    }

    public override void SetEditValue(object editValue)
    {
        BootstrapControl.Text = editValue?.ToString() ?? string.Empty;
    }

    public override object GetEditedValue()
    {
        return BootstrapControl.Text;
    }

    protected override void OnSendCharToEditor(char key)
    {
        BootstrapControl.Text = key.ToString();
    }
}
