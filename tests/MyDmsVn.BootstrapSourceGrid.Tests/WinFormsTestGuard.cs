using System.Windows.Forms;

namespace MyDmsVn.BootstrapSourceGrid.Tests;

internal static class WinFormsTestGuard
{
    private static readonly object SyncRoot = new object();
    private static bool _configured;

    internal static bool IsConfigured
    {
        get
        {
            lock (SyncRoot)
            {
                return _configured;
            }
        }
    }

    public static void Configure()
    {
        lock (SyncRoot)
        {
            if (_configured)
            {
                return;
            }

            Application.SetUnhandledExceptionMode(
                UnhandledExceptionMode.ThrowException,
                threadScope: false);
            _configured = true;
        }
    }
}
