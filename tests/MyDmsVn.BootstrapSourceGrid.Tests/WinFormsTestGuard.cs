using System.Threading;
using System.Windows.Forms;

namespace MyDmsVn.BootstrapSourceGrid.Tests;

internal static class WinFormsTestGuard
{
    private static int _configured;

    public static void Configure()
    {
        if (Interlocked.Exchange(ref _configured, 1) != 0)
        {
            return;
        }

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
    }
}
