using SFML.Graphics;
using SFML.Window;

namespace FunManipulator;

public class ScreenshotTool : GUITool
{
    private static Sprite? Screenshot = null;

    public static void Run()
    {
        Console.WriteLine("Initializing window...");

        PlatformSpecific.InitializeWindowing();

        // Load font
        Font = new Font("8bitoperator_jve.ttf");

        // We want to draw to a 640x480 canvas, not whatever gets scaled to
        var renderTex = new RenderTexture(640, 480);

        // Initialize window and its settings
        float windowScale = Config.Instance.WindowScale;
        var window = new RenderWindow(new VideoMode((uint)(640f * windowScale), (uint)(480f * windowScale)), "Screenshot Tool", Styles.Default, new ContextSettings { });
        PlatformSpecific.ConfigureAppWindow(window.SystemHandle);
        PlatformSpecific.MoveWindowToGameWindow(window.SystemHandle, true);
        ConfigureWindow(window);
        ConfigureView(window, 640, 480);

        /*
        // Create Dogi Manip Tool button
        var dogiManipToolButton = new TextButton(10, 10, 20, 20, "D", () =>
        {
            window.Close();
            Program.NextProgramToRun = "DogiManip";
        });
        */

        // Hide console
        PlatformSpecific.HideConsole();
        Program.AutoProgressEnding = true;

        while (window.IsOpen)
        {
            // Update window I/O
            EarlyUpdateWindow(window);

            // Draw background
            if (Config.Instance.WindowTransparent)
            {
                renderTex.Clear(new Color(0, 0, 0, 0));
                if (Screenshot != null)
                {
                    // Screenshot being used
                    Screenshot.Color = new Color(255, 255, 255, (byte)Config.Instance.ScreenshotTool.TransparentScreenshotAlpha);
                    renderTex.Draw(Screenshot);
                }
            }
            else
            {
                renderTex.Clear(Color.Black);
                if (Screenshot != null)
                {
                    // Screenshot being used
                    renderTex.Draw(Screenshot);
                }
            }

            // Draw UI
            /*
            dogiManipToolButton.Update(renderTex);
            */

            // Final draw to the screen
            renderTex.Display();
            if (Config.Instance.WindowTransparent)
                window.Clear(new Color(0, 0, 0, 0));
            else
                window.Clear();
            window.Draw(new Sprite(renderTex.Texture));
            window.Display();

            // Update window I/O
            LateUpdateWindow();
        }
    }
}
