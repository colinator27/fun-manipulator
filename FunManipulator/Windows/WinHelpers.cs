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

    public static ScreenshotResult TakeScreenshotOfGame()
    {
        IntPtr hwnd = FindWindow("YYGameMakerYY", "UNDERTALE");
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
        SetWindowLong(hWnd, GWL_EXSTYLE, GetWindowLong(hWnd, GWL_EXSTYLE) | WS_EX_NOACTIVATE | WS_EX_APPWINDOW);
    }
#pragma warning restore CA1416 // Validate platform compatibility
}
