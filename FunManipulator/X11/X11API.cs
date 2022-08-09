using System.Runtime.InteropServices;

namespace FunManipulator.X11;

public static class X11API
{
    [DllImport("X11")]
    public extern static int XInitThreads();
    [DllImport("X11")]
    public static extern IntPtr XOpenDisplay(string? display_name);
    [DllImport("X11")]
    public static extern void XFlush(IntPtr display);
    [DllImport("X11")]
    public static extern void XFree(IntPtr ptr);
    [DllImport("X11")]
    public static extern void XCloseDisplay(IntPtr display);
    [DllImport("X11")]
    public static extern int XInternAtom (IntPtr display, string atom_name, bool only_if_exists);
    [DllImport("X11")]
    public static extern void XChangeProperty(IntPtr display, IntPtr w, int property, int type, int format, int mode, ref ulong data, int nelements);
    [DllImport("X11")]
    public static extern int XSendEvent(IntPtr display, IntPtr window, bool propagate, EventMask event_mask, ref XClientMessageEvent event_send);
    [DllImport("X11")]
    public static extern int XDefaultScreen(IntPtr display);
    [DllImport("X11")]
    public static extern IntPtr XRootWindow(IntPtr display, int screen);
    [DllImport("X11")]
    public static extern int XGetWindowProperty(IntPtr display, IntPtr w, int property, int long_offset, int long_length, bool delete, int req_type, out int actual_type_return, out int actual_format_return, out int nitems_return, out int bytes_after_return, out IntPtr prop_return);
    [DllImport("X11")]
    public static extern int XFetchName(IntPtr display, IntPtr window, ref IntPtr name);

    [Flags]
    public enum EventMask : int
    {
        KeyPressMask,
        KeyReleaseMask,  
        ButtonPressMask,
        ButtonReleaseMask,
        EnterWindowMask,
        LeaveWindowMask,
        PointerMotionMask,
        PointerMotionHintMask,
        Button1MotionMask,
        Button2MotionMask,
        Button3MotionMask,
        Button4MotionMask,
        Button5MotionMask,
        ButtonMotionMask,
        KeymapStateMask,
        ExposureMask,
        VisibilityChangeMask,
        StructureNotifyMask,
        ResizeRedirectMask,
        SubstructureNotifyMask,
        SubstructureRedirectMask,
        FocusChangeMask,
        PropertyChangeMask,
        ColormapChangeMask,
        OwnerGrabButtonMask
    }

    public enum EventType
    {
        KeyPress = 2,
        KeyRelease = 3,
        ButtonPress = 4,
        ButtonRelease = 5,
        MotionNotify = 6,
        EnterNotify = 7,
        LeaveNotify = 8,
        FocusIn = 9,
        FocusOut = 10,
        KeymapNotify = 11,
        Expose = 12,
        GraphicsExpose = 13,
        NoExpose = 14,
        VisibilityNotify = 15,
        CreateNotify = 16,
        DestroyNotify = 17,
        UnmapNotify = 18,
        MapNotify = 19,
        MapRequest = 20,
        ReparentNotify = 21,
        ConfigureNotify = 22,
        ConfigureRequest = 23,
        GravityNotify = 24,
        ResizeRequest = 25,
        CirculateNotify = 26,
        CirculateRequest = 27,
        PropertyNotify = 28,
        SelectionClear = 29,
        SelectionRequest = 30,
        SelectionNotify = 31,
        ColormapNotify = 32,
        ClientMessage = 33,
        MappingNotify = 34,
        GenericEvent = 35
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XClientMessageEvent
    {
        public EventType type;
        public uint serial;
        public bool send_event;
        public IntPtr display;
        public IntPtr window;
        public int message_type;
        public int format;
        public int l0;
        public int l1;
        public int l2;
        public int l3;
        public int l4;
    }

    public const int _NET_WM_STATE_REMOVE = 0;
    public const int _NET_WM_STATE_ADD = 1;
    public const int _NET_WM_STATE_TOGGLE = 2;

    public const int None = 0;
    public const int XA_CARDINAL = 6;
    public const int PropModeReplace = 0;
    public const int AnyPropertyType = 0;
}