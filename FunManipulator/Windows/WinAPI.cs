using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace FunManipulator.Windows;

public static class WinAPI
{
#pragma warning disable CA1416 // Validate platform compatibility
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);
    [DllImport("user32.dll")]
    public static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);
    [DllImport("user32.dll")]
    public static extern bool GetClientRect(IntPtr hWnd, out Rect lpRect);
    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName); 
    [DllImport("user32.dll")]
    public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    public const int PW_CLIENTONLY = 1;

    public const int GWL_EXSTYLE = -20;
    public const uint WS_EX_NOACTIVATE = 0x08000000;
    public const uint WS_EX_APPWINDOW = 0x00040000;
#pragma warning restore CA1416 // Validate platform compatibility
}

