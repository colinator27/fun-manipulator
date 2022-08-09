using System.Runtime.InteropServices;
using static FunManipulator.PlatformSpecific;
using static FunManipulator.X11.X11API;

namespace FunManipulator.X11;

public static class X11Helpers
{
#pragma warning disable CA1416 // Validate platform compatibility
    private static IntPtr GetRootWindow(IntPtr display)
    {
        IntPtr rootWindow = IntPtr.Zero;
        int defaultScreen = XDefaultScreen(display);
        rootWindow = XRootWindow(display, defaultScreen);
        return rootWindow;
    }

    /*
    This doesn't work yet
    
    private static IntPtr GetGameWindow()
    {
        IntPtr display = XOpenDisplay(null);

        IntPtr window = IntPtr.Zero;
        IntPtr rootWindow = GetRootWindow(display);
        int clientList = XInternAtom(display, "_NET_CLIENT_LIST", true);

        int actualType;
        int format;
        int numItems;
        int bytesAfter;
        IntPtr data;

        int status = XGetWindowProperty(display, rootWindow, clientList, 0, ~0, false, AnyPropertyType, 
                                        out actualType, out format, out numItems, out bytesAfter, out data);
        if (status == 0)
        {
            for (int i = 0; i < numItems; i++)
            {
                IntPtr windowName = IntPtr.Zero;
                IntPtr currWindow = Marshal.ReadIntPtr(data + (Marshal.SizeOf<IntPtr>() * i));
                status = XFetchName(display, currWindow, ref windowName);
                if (status == 0)
                {
                    string? name = Marshal.PtrToStringAuto(windowName);
                    if (name != null)
                    {
                        if (name == "UNDERTALE")
                        {
                            window = currWindow;
                            XFree(windowName);
                            break;
                        }
                    }
                }
                XFree(windowName);
            }
        }

        XFree(data);

        XCloseDisplay(display);
        return window;
    }
    */

    public static void InitializeWindowing()
    {
        // Fixes a crash
        XInitThreads();
    }

    public static void ConfigureAppWindow(IntPtr wnd)
    {
        IntPtr display = XOpenDisplay(null);

        if (Config.Instance.WindowTransparent)
        {
            int property = XInternAtom(display, "_NET_WM_WINDOW_OPACITY", false);
            if (property != None)
            {
                ulong opacity = (0xffffffff / 0xff) * 120;
                XChangeProperty(display, wnd, property, XA_CARDINAL, 32, PropModeReplace, ref opacity, 1);
            }
        }

        /*
        This currently doesn't work for some reason

        int wmStateAbove = XInternAtom(display, "_NET_WM_STATE_ABOVE", true);
        int wmNetWmState = XInternAtom(display, "_NET_WM_STATE", true);
        if (wmStateAbove != None && wmNetWmState != None)
        {
            XClientMessageEvent message = new()
            {
                type = EventType.ClientMessage,
                window = wnd,
                message_type = wmNetWmState,
                format = 32
            };
            message.l0 = _NET_WM_STATE_ADD;
            message.l1 = wmStateAbove;
            message.l2 = 0;
            message.l3 = 0;
            message.l4 = 0;
            XSendEvent(display, GetRootWindow(display), false, 
                        EventMask.SubstructureRedirectMask | EventMask.SubstructureNotifyMask, ref message);
        }
        */

        XFlush(display);
        XCloseDisplay(display);
    }
#pragma warning restore CA1416 // Validate platform compatibility
}