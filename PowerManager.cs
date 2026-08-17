using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PaperEX.Caffeine;

/// <summary>
/// Thin wrapper around the Win32 SetThreadExecutionState API.
/// </summary>
internal static class PowerManager
{
    private const uint EsContinuous = 0x80000000;
    private const uint EsSystemRequired = 0x00000001;
    private const uint EsDisplayRequired = 0x00000002;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint SetThreadExecutionState(uint esFlags);

    [DllImport("kernel32.dll")]
    private static extern void SetLastError(uint dwErrCode);

    /// <summary>Block idle sleep and keep the display on until the mode changes.</summary>
    public static bool KeepAwakeAndDisplayOn()
        => Apply(EsContinuous | EsSystemRequired | EsDisplayRequired);

    /// <summary>Block idle sleep but let the display turn off per the user's settings.</summary>
    public static bool KeepAwakeAllowDisplayOff()
        => Apply(EsContinuous | EsSystemRequired);

    /// <summary>Clear our execution state so Windows resumes normal power management.</summary>
    public static bool RestoreDefault()
        => Apply(EsContinuous);

    private static bool Apply(uint flags)
    {
        SetLastError(0);
        uint previousState = SetThreadExecutionState(flags);

        // A successful first call legitimately returns the previous state, which is 0.
        // Only treat 0 as a failure when the Win32 error code was also set.
        int error = Marshal.GetLastWin32Error();
        if (previousState == 0 && error != 0)
        {
            Debug.WriteLine($"SetThreadExecutionState(0x{flags:X8}) failed with Win32 error {error}.");
            return false;
        }

        return true;
    }
}
