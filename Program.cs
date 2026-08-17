using System.Diagnostics;
using System.Windows.Forms;

namespace PaperEX.Caffeine;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Log unexpected UI-thread exceptions instead of showing a crash dialog.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += static (_, e) =>
            Debug.WriteLine($"Unhandled UI exception: {e.Exception}");

        using var singleInstance = new SingleInstanceManager(@"Local\PaperEX.Caffeine.SingleInstance");
        if (!singleInstance.IsFirstInstance)
        {
            // Another instance is already running; exit quietly.
            return;
        }

        using var context = new TrayApplicationContext();
        Application.Run(context);
    }
}
