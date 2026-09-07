using System.ComponentModel;

namespace MyDmsVn.Bootstrap5WinFormUI.Controls;

/// <summary>
/// Provides a Bootstrap-themed SourceGrid while preserving SourceGrid grid behavior.
/// </summary>
[ToolboxItem(true)]
public class BootstrapSourceGrid : SourceGrid.Grid
{
    /// <summary>
    /// Initializes a new Bootstrap-themed SourceGrid.
    /// </summary>
    public BootstrapSourceGrid()
    {
        Name = nameof(BootstrapSourceGrid);
    }
}
