using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace FunManipulator;

public class GUITool
{
    // Mouse states
    protected static Vector2f MousePosition = new Vector2f(0, 0);
    protected static bool MouseLJustPressed = false;
    protected static bool MouseLJustReleased = false;
    protected static bool MouseRJustPressed = false;
    protected static bool MouseRJustReleased = false;

    // General assets
    protected static Font? Font;

    // Window-related
    protected static View? View;
    protected static uint BaseWidth = 640;
    protected static uint BaseHeight = 480;

    protected static void ConfigureWindow(RenderWindow window)
    {
        window.SetVerticalSyncEnabled(true);

        // Window event handling
        window.Closed += (ev, args) => window.Close();
        window.MouseButtonPressed += (ev, args) =>
        {
            if (args.Button == Mouse.Button.Left)
                MouseLJustPressed = true;
            else if (args.Button == Mouse.Button.Right)
                MouseRJustPressed = true;
        };
        window.MouseButtonReleased += (ev, args) =>
        {
            if (args.Button == Mouse.Button.Left)
                MouseLJustReleased = true;
            else if (args.Button == Mouse.Button.Right)
                MouseRJustReleased = true;
        };
        window.Resized += (ev, args) =>
        {
            UpdateView(args.Width, args.Height);
            if (View != null)
                window.SetView(View);
        };
    }

    protected static void ConfigureView(RenderWindow window, uint width, uint height)
    {
        BaseWidth = width;
        BaseHeight = height;

        View = new View
        {
            Size = new Vector2f(width, height),
            Center = new Vector2f(width / 2, height / 2)
        };
        UpdateView(width, height);
        window.SetView(View);
    }

    protected static void UpdateView(uint newWidth, uint newHeight)
    {
        if (View == null)
            return;

        // Create black bars when resized
        float viewRatio = BaseWidth / (float)BaseHeight;
        float windowRatio = newWidth / (float)newHeight;
        float posX = 0;
        float posY = 0;
        float sizeX = 1;
        float sizeY = 1;

        if (windowRatio >= viewRatio)
        {
            // Horizontal spacing
            sizeX = viewRatio / windowRatio;
            posX = (1 - sizeX) / 2f;
        }
        else
        {
            // Vertical spacing
            sizeY = windowRatio / viewRatio;
            posY = (1 - sizeY) / 2f;
        }

        View.Viewport = new FloatRect(posX, posY, sizeX, sizeY);
    }

    protected static void EarlyUpdateWindow(RenderWindow window)
    {
        // Run window events
        window.DispatchEvents();

        // Update mouse position
        MousePosition = window.MapPixelToCoords(Mouse.GetPosition(window));
    }

    protected static void LateUpdateWindow()
    {
        // Clear mouse states
        MouseLJustPressed = false;
        MouseLJustReleased = false;
        MouseRJustPressed = false;
        MouseRJustReleased = false;
    }

    protected abstract class Button
    {
        private RectangleShape shape { get; init; }
        public Color NormalColor { get; set; } = Color.Black;
        public Color HoverColor { get; set; } = new Color(100, 100, 100);
        public Color PressColor { get; set; } = new Color(200, 200, 200);
        public Color OutlineColor { get; set; } = new Color(80, 80, 80);
        public int OutlineThickness { get; set; } = 1;
        protected bool IsHovered { get; private set; } = false;

        public Button(float x, float y, float width, float height)
        {
            // Make rectangle
            shape = new RectangleShape(new Vector2f(width, height));
            shape.Position = new Vector2f(x, y);
        }

        public virtual void Update(RenderTexture texture)
        {
            // Update color, process clicks
            IsHovered = (MousePosition.X >= shape.Position.X &&
                         MousePosition.Y >= shape.Position.Y &&
                         MousePosition.X <= shape.Position.X + shape.Size.X &&
                         MousePosition.Y <= shape.Position.Y + shape.Size.Y);
            Color color;
            if (IsHovered)
            {
                if (MouseLJustReleased)
                    OnClick();

                if (Mouse.IsButtonPressed(Mouse.Button.Left))
                    color = PressColor;
                else
                    color = HoverColor;
            }
            else
            {
                color = NormalColor;
            }
            shape.FillColor = color;
            shape.OutlineColor = OutlineColor;
            shape.OutlineThickness = OutlineThickness;

            texture.Draw(shape);
        }

        public abstract void OnClick();
    }

    protected class TextButton : Button
    {
        private Text text { get; init; }
        public Action ActionOnClick { get; set; }

        public TextButton(float x, float y, float width, float height, string text, Action onClick) : base(x, y, width, height)
        {
            // Create text
            this.text = new Text(text, Font, 16);
            this.text.FillColor = Color.White;
            this.text.Position = new Vector2f(x + 4, y - 2);

            ActionOnClick = onClick;
        }

        public override void Update(RenderTexture texture)
        {
            base.Update(texture);

            texture.Draw(text);
        }

        public override void OnClick()
        {
            // Perform desired action
            ActionOnClick();
        }
    }
}
