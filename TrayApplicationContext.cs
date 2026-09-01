using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace PaperEX.Caffeine;

internal enum AwakeMode
{
    KeepAwakeAndDisplayOn,
    KeepAwakeAllowDisplayOff,
}

/// <summary>
/// Tray-only application context: no main window, just a NotifyIcon and its menu.
/// </summary>
internal sealed class TrayApplicationContext : ApplicationContext
{
    private const string AppTitle = "PaperEX的咖啡因";

    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _menu;
    private readonly Font _titleFont;
    private readonly ToolStripMenuItem _displayOnItem;
    private readonly ToolStripMenuItem _displayOffItem;

    private AwakeMode _mode;
    private bool _cleanedUp;

    public TrayApplicationContext()
    {
        _menu = new ContextMenuStrip();
        _titleFont = new Font(_menu.Font, FontStyle.Bold);

        _displayOnItem = new ToolStripMenuItem("不休眠 + 屏幕常亮");
        _displayOffItem = new ToolStripMenuItem("不休眠 + 允许屏幕关闭");
        var exitItem = new ToolStripMenuItem("退出");

        _displayOnItem.Click += (_, _) => SwitchMode(AwakeMode.KeepAwakeAndDisplayOn);
        _displayOffItem.Click += (_, _) => SwitchMode(AwakeMode.KeepAwakeAllowDisplayOff);
        exitItem.Click += (_, _) => ExitThread();

        _menu.Items.Add(new ToolStripMenuItem(AppTitle) { Enabled = false, Font = _titleFont });
        _menu.Items.Add(_displayOnItem);
        _menu.Items.Add(_displayOffItem);
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add(exitItem);

        _notifyIcon = new NotifyIcon
        {
            Icon = LoadAppIcon(),
            ContextMenuStrip = _menu,
            Visible = true,
        };

        // Start blocking idle sleep with the display kept on, immediately.
        SwitchMode(AwakeMode.KeepAwakeAndDisplayOn);
    }

    private void SwitchMode(AwakeMode mode)
    {
        bool applied = mode switch
        {
            AwakeMode.KeepAwakeAndDisplayOn => PowerManager.KeepAwakeAndDisplayOn(),
            AwakeMode.KeepAwakeAllowDisplayOff => PowerManager.KeepAwakeAllowDisplayOff(),
            _ => false,
        };

        if (!applied)
        {
            Debug.WriteLine($"Failed to apply mode {mode}; keeping current mode {_mode}.");
            return;
        }

        _mode = mode;
        _displayOnItem.Checked = mode == AwakeMode.KeepAwakeAndDisplayOn;
        _displayOffItem.Checked = mode == AwakeMode.KeepAwakeAllowDisplayOff;
        _notifyIcon.Text = mode == AwakeMode.KeepAwakeAndDisplayOn
            ? $"{AppTitle}\n不休眠 · 屏幕常亮"
            : $"{AppTitle}\n不休眠 · 允许屏幕关闭";
    }

    protected override void ExitThreadCore()
    {
        Cleanup();
        base.ExitThreadCore();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Cleanup();
        }

        base.Dispose(disposing);
    }

    private void Cleanup()
    {
        if (_cleanedUp)
        {
            return;
        }

        _cleanedUp = true;

        if (!PowerManager.RestoreDefault())
        {
            Debug.WriteLine("Failed to restore the default power state.");
        }

        _notifyIcon.Visible = false; // hide before disposal so no ghost icon is left behind
        _notifyIcon.Dispose();
        _menu.Dispose();
        _titleFont.Dispose();
    }

    private static Icon LoadAppIcon()
    {
        try
        {
            using Stream? stream = typeof(TrayApplicationContext).Assembly
                .GetManifestResourceStream("PaperEX.Caffeine.Resources.app.ico");
            if (stream is not null)
            {
                return new Icon(stream);
            }

            Debug.WriteLine("Embedded app icon not found; using the default application icon.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load the embedded icon ({ex.Message}); using the default application icon.");
        }

        return SystemIcons.Application;
    }
}
