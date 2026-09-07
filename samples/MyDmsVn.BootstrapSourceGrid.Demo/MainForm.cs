using System.Drawing;
using System.Windows.Forms;

namespace MyDmsVn.BootstrapSourceGrid.Demo;

public sealed class MainForm : Form
{
    public MainForm()
    {
        Text = "MyDmsVn BootstrapSourceGrid Demo";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(960, 640);

        Controls.Add(new Label
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            Text = "BootstrapSourceGrid demo scenarios will be added in later stages.",
            TextAlign = ContentAlignment.MiddleCenter,
        });
    }
}
