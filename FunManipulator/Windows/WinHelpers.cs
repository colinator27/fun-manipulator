using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using static FunManipulator.PlatformSpecific;
using static FunManipulator.Windows.WinAPI;

namespace FunManipulator.Windows;

public static class WinHelpers
{
#pragma warning disable CA1416 // Validate platform compatibility
    public static byte[] GetPngFromBitmap(Bitmap bitmap)
    {
        using (var stream = new MemoryStream())
        {
            bitmap.Save(stream, ImageFormat.Png);
            return stream.ToArray();
        }
    }

    private static IntPtr GetGameHWnd()
    {
        return FindWindow("YYGameMakerYY", "UNDERTALE");
    }

    public static ScreenshotResult TakeScreenshotOfGame()
    {
        IntPtr hwnd = GetGameHWnd();
        if (hwnd == IntPtr.Zero)
            return new();

        Rect rect;
        GetClientRect(hwnd, out rect);
        int clientWidth = rect.Right - rect.Left;
        int clientHeight = rect.Bottom - rect.Top;

        Point topLeft = new Point(rect.Left, rect.Top);
        ClientToScreen(hwnd, ref topLeft);
        rect.Left = topLeft.X;
        rect.Top = topLeft.Y;

        Bitmap bmp = new Bitmap(clientWidth, clientHeight, PixelFormat.Format32bppArgb);
        using (Graphics graphics = Graphics.FromImage(bmp))
        {
            graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, new Size(clientWidth, clientHeight), CopyPixelOperation.SourceCopy);
        }

        return new()
        {
            Width = clientWidth,
            Height = clientHeight,
            Data = GetPngFromBitmap(bmp)
        };
    }

    public static void ConfigureAppWindow(IntPtr hWnd)
    {
        if (Config.Instance.WindowTransparent)
        {
            Rect rect;
            GetClientRect(hWnd, out rect);
            int clientWidth = rect.Right - rect.Left;
            int clientHeight = rect.Bottom - rect.Top;

            DWM_BLURBEHIND bb = new();
            bb.dwFlags = DWM_BB.Enable;
            bb.fEnable = true;
            DwmEnableBlurBehindWindow(hWnd, ref bb);

            SetWindowPos(hWnd, (IntPtr)HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }

        SetWindowLongPtr(hWnd, GWL_EXSTYLE, (IntPtr)((GetWindowLongPtr(hWnd, GWL_EXSTYLE).ToInt64() | WS_EX_NOACTIVATE | WS_EX_APPWINDOW)));
    }

    public static void MoveWindowToGameWindow(IntPtr hWnd, bool onlyScale)
    {
        IntPtr gameHWnd = GetGameHWnd();
        if (gameHWnd == IntPtr.Zero)
            return;
        if (GetForegroundWindow() != gameHWnd)
            onlyScale = true;

        // Restore window first if needed
        if (!onlyScale)
            ToggleWindowMinimized(hWnd, true);

        Rect rect;
        GetClientRect(gameHWnd, out rect);
        int clientWidth = rect.Right - rect.Left;
        int clientHeight = rect.Bottom - rect.Top;

        Point topLeft = new Point(rect.Left, rect.Top);
        ClientToScreen(gameHWnd, ref topLeft);
        rect.Left = topLeft.X;
        rect.Top = topLeft.Y;
        rect.Right = rect.Left + clientWidth;
        rect.Bottom = rect.Top + clientHeight;

        AdjustWindowRectEx(ref rect, (uint)GetWindowLongPtr(hWnd, GWL_STYLE), false, (uint)GetWindowLongPtr(hWnd, GWL_EXSTYLE));
        SetWindowPos(hWnd, (IntPtr)HWND_TOPMOST, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, SWP_NOACTIVATE | (onlyScale ? SWP_NOMOVE : 0));
    }

    public static void ToggleWindowMinimized(IntPtr hWnd, bool onlyRestore = false)
    {
        if ((GetWindowLongPtr(hWnd, GWL_STYLE).ToInt64() & WS_MINIMIZE) == WS_MINIMIZE)
            ShowWindow(hWnd, SW_SHOWNOACTIVATE);
        else if (!onlyRestore)
            ShowWindow(hWnd, SW_MINIMIZE);
    }

    public static void HideConsole()
    {
        ShowWindow(GetConsoleWindow(), SW_HIDE);
    }
#pragma warning restore CA1416 // Validate platform compatibility
}
