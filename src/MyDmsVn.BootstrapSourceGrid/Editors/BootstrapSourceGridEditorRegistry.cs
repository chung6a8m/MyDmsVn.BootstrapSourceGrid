using System;
using System.Collections.Generic;

namespace MyDmsVn.BootstrapSourceGrid.Editors;

internal sealed class BootstrapSourceGridEditorRegistry : IDisposable
{
    private readonly List<SourceGrid.Cells.Editors.EditorControlBase> _ownedEditors = new();

    internal T Register<T>(T editor)
        where T : SourceGrid.Cells.Editors.EditorControlBase
    {
        if (editor is null)
        {
            throw new ArgumentNullException(nameof(editor));
        }

        _ownedEditors.Add(editor);
        return editor;
    }

    public void Dispose()
    {
        foreach (var editor in _ownedEditors)
        {
            editor.Control.Dispose();
            editor.Dispose();
        }

        _ownedEditors.Clear();
    }
}
