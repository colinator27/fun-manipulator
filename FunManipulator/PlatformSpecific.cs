using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FunManipulator;

public static class PlatformSpecific
{
    public struct ScreenshotResult
    {
        public int Width;
        public int Height;
        public byte[]? Data;
    }

    public static ScreenshotResult TakeScreenshotOfGame()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Windows.WinHelpers.TakeScreenshotOfGame();
        return new();
    }

    public static void ConfigureAppWindow(IntPtr hWnd)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Windows.WinHelpers.ConfigureAppWindow(hWnd);
    }

    public static void MoveWindowToGameWindow(IntPtr hWnd, bool onlyScale)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Windows.WinHelpers.MoveWindowToGameWindow(hWnd, onlyScale);
    }

    public static void ToggleWindowMinimized(IntPtr hWnd)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Windows.WinHelpers.ToggleWindowMinimized(hWnd);
    }

    public static void HideConsole()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Windows.WinHelpers.HideConsole();
    }
}
