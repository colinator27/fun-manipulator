using System.Runtime.InteropServices;

namespace FunManipulator;

public static class PlatformSpecific
{
    public static string GetConfigFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "config_linux.json";

        // Default
        return "config_windows.json";
    }

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

    public static void InitializeWindowing()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            X11.X11Helpers.InitializeWindowing();
    }

    public static void ConfigureAppWindow(IntPtr wnd)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Windows.WinHelpers.ConfigureAppWindow(wnd);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            X11.X11Helpers.ConfigureAppWindow(wnd);
    }

    public static void MoveWindowToGameWindow(IntPtr wnd, bool onlyScale)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Windows.WinHelpers.MoveWindowToGameWindow(wnd, onlyScale);
    }

    public static void ToggleWindowMinimized(IntPtr wnd)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Windows.WinHelpers.ToggleWindowMinimized(wnd);
    }

    public static void HideConsole()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Windows.WinHelpers.HideConsole();
    }
}
