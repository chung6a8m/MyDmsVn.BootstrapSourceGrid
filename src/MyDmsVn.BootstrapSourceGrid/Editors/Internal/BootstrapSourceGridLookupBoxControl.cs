using System;
using System.Windows.Forms;
using MyDmsVn.Bootstrap5WinFormUI.Controls;

namespace MyDmsVn.BootstrapSourceGrid.Editors.Internal;

internal sealed class BootstrapSourceGridLookupBoxControl : BootstrapLookupBox
{
    internal Func<bool, bool>? OwnerNavigationRequested { get; set; }

    internal void ReplaceWithFirstEditCharacter(char value)
    {
        Editor.Text = value.ToString();
        Editor.SelectionStart = Editor.TextLength;
        Editor.SelectionLength = 0;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        var key = keyData & Keys.KeyCode;
        var modifiers = keyData & Keys.Modifiers;
        if (!IsDropDownOpen && modifiers == Keys.None &&
            (key == Keys.Enter || key == Keys.Escape))
        {
            return false;
        }

        if (key == Keys.Tab && (modifiers & (Keys.Alt | Keys.Control)) == Keys.None)
        {
            if (!ResolvePendingLookupForGridNavigation(ref msg))
            {
                return true;
            }

            return OwnerNavigationRequested?.Invoke((modifiers & Keys.Shift) == Keys.Shift) == true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private bool ResolvePendingLookupForGridNavigation(ref Message msg)
    {
        if (!HasPendingText && !IsDropDownOpen)
        {
            return true;
        }

        var closedEnterBehavior = ClosedEnterKeyBehavior;
        var enterBehavior = EnterKeyBehavior;
        try
        {
            ClosedEnterKeyBehavior = BootstrapLookupClosedEnterKeyBehavior.ResolvePendingText;
            EnterKeyBehavior = BootstrapLookupEnterKeyBehavior.CommitSelection;
            base.ProcessCmdKey(ref msg, Keys.Enter);
        }
        finally
        {
            ClosedEnterKeyBehavior = closedEnterBehavior;
            EnterKeyBehavior = enterBehavior;
        }

        return !HasPendingText && !IsDropDownOpen;
    }
}
