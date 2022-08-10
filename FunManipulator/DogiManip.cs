using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Text;
using System.Text.RegularExpressions;

namespace FunManipulator;

public class DogiManip
{
    private class Seed
    {
        public int SeedIndex { get; init; }
        public int StepCount { get; init; }
        public List<Snowball> Snowballs { get; init; }
        public int SnowballsRightOfPuzzle { get; init; }
        public float Score { get; set; } = 0;

        public Seed(int seedIndex, int stepCount, List<Snowball> snowballs)
        {
            SeedIndex = seedIndex;
            StepCount = stepCount;
            Snowballs = snowballs;

            // Additionally calculate number of snowballs to the right of the puzzle
            SnowballsRightOfPuzzle = 0;
            foreach (var snowball in Snowballs)
            {
                if (snowball.X >= 160)
                    SnowballsRightOfPuzzle++;
            }
        }

        public void CalculateScore()
        {
            float maxDistance = Config.Instance.DogiManip.ScoreMaxDistance;
            float numRightBias = Config.Instance.DogiManip.ScoreNumRightBias;

            // Reset snowball distances, and count number to right of puzzle
            foreach (var snowball in Snowballs)
                snowball.ClosestDistance = -1f;
            int numToRightOfPuzzle = 0;
            foreach (var editorSnowball in EditorSnowballs)
            {
                if (editorSnowball.Active)
                    continue;
                editorSnowball.ClosestDistance = -1f;
                float x = ((editorSnowball.Position.X * 0.5f) + ViewX);
                if (x >= 160)
                    numToRightOfPuzzle++;
            }

            foreach (var editorSnowball in EditorSnowballs)
            {
                if (editorSnowball.Active)
                    continue;
                foreach (var snowball in Snowballs)
                {
                    // Check distance between the snowballs
                    float dx = snowball.X - ((editorSnowball.Position.X * 0.5f) + ViewX);
                    float dy = snowball.Y - ((editorSnowball.Position.Y * 0.5f) + ViewY);
                    float distance = MathF.Sqrt((dx * dx) + (dy * dy));
                    if (distance < maxDistance)
                    {
                        // We're within the threshold for this counting as the correct snowball
                        // Now just check to see if it's the *closest* one we've seen
                        if (snowball.ClosestDistance == -1f || distance < snowball.ClosestDistance)
                        {
                            // So we're the closest one THIS snowball has seen... but are we the closest that the OTHERS have seen?
                            if (editorSnowball.ClosestDistance == -1f || distance < editorSnowball.ClosestDistance)
                            {
                                snowball.ClosestDistance = distance;
                                editorSnowball.ClosestDistance = distance;
                            }
                        }
                    }
                }
            }

            // Compute final score
            Score = (maxDistance * numToRightOfPuzzle * numRightBias) / MathF.Sqrt(1.0f + Math.Abs(numToRightOfPuzzle - SnowballsRightOfPuzzle));
            foreach (var editorSnowball in EditorSnowballs)
            {
                if (editorSnowball.Active)
                    continue;
                if (editorSnowball.ClosestDistance == -1f)
                    continue;
                Score += (maxDistance - editorSnowball.ClosestDistance);
            }
        }

        public class Snowball
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float ClosestDistance { get; set; } = -1;

            public Snowball(float x, float y)
            {
                X = x;
                Y = y;
            }
        }
    }

    private class EditorSnowball
    {
        public const float CircleRadius = 2.8f * 2.0f;
        public const int CirclePrecision = 8;
        private static CircleShape shape = new(CircleRadius, CirclePrecision);
        private static Vector2f origin = new(CircleRadius, CircleRadius);

        public Vector2f Position { get; set; }
        public bool Active { get; set; } = true;

        public float ClosestDistance { get; set; } = -1;

        public void Update(RenderTexture texture)
        {
            Color color = Color.White;

            if (Active)
            {
                if (ChosenPreview != -1)
                {
                    // There's no need for this to exist; we already chose
                    EditorSnowballsToDelete.Add(this);
                    return;
                }

                // Follow mouse
                Position = MousePosition;

                // Don't process or display "offscreen"
                if (Position.X < 240 || Position.X > 460)
                    return;

                // Check for being placed
                if (MouseLJustPressed)
                {
                    Active = false;
                    PerformSearch();
                    if (CreatingSnowballs && EditorSnowballs.Count < 16)
                        EditorSnowballs.Add(new());
                }
                else
                    color = Color.Red;
            }
            else
            {
                // Check if mouse is in radius
                float dx = MousePosition.X - Position.X;
                float dy = MousePosition.Y - Position.Y;
                bool hovering = (MathF.Sqrt((dx * dx) + (dy * dy)) < CircleRadius);

                if (hovering)
                {
                    if (CreatingSnowballs)
                    {
                        if (MouseRJustReleased)
                        {
                            // Mark for deletion
                            EditorSnowballsToDelete.Add(this);
                        }
                    }
                    else
                    {
                        // Hover color
                        color = Color.Yellow;
                    }
                }
            }

            shape.Position = Position - origin;
            shape.FillColor = color;
            texture.Draw(shape);
        }
    }

    private abstract class Button
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

    private class TextButton : Button
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

    private class PreviewButton : Button
    {
        public RenderTexture RenderTexture { get; init; }
        public Texture Texture { get; init; }
        public Sprite Sprite { get; init; }
        public int Index { get; init; }
        public Seed Seed { get; init; }

        public PreviewButton(float x, float y, RenderTexture renderTex, int index, Seed seed) : base(x - 2, y - 2, renderTex.Size.X + 4, renderTex.Size.Y + 4)
        {
            RenderTexture = renderTex;
            Texture = renderTex.Texture;
            Sprite = new Sprite(Texture);
            Sprite.Position = new Vector2f(x, y);
            Index = index;
            Seed = seed;
        }

        public override void Update(RenderTexture texture)
        {
            base.Update(texture);

            texture.Draw(Sprite);

            // Change hovered preview index to current one, if hovered
            if (IsHovered)
                HoveredPreview = Index;

            // Selection using keyboard buttons
            if (Config.Instance.DogiManip.PreviewSelectKeys[Index].CheckInput())
            {
                ChosenPreview = Index;
                CreatingSnowballs = false;
                CachedChosenPreview = -1;
            }
        }

        public override void OnClick()
        {
            // Select this preview as the final one!
            ChosenPreview = Index;
            CreatingSnowballs = false;
            CachedChosenPreview = -1;
        }
    }
    
    // The view that the game should be at
    private const float ViewX = 0, ViewY = 580 - 240;

    // Mouse states
    private static Vector2f MousePosition = new Vector2f(0, 0);
    private static bool MouseLJustPressed = false;
    private static bool MouseLJustReleased = false;
    private static bool MouseRJustPressed = false;
    private static bool MouseRJustReleased = false;

    // Seeds and the matching previews discovered
    private static readonly List<Seed> Seeds = new();
    private static List<PreviewButton> Previews = new();

    // Misc. assets
    private static Sprite? BackgroundSprite;
    private static Sprite? PreviewBackgroundLayer;
    private static Sprite? PreviewForegroundLayer;
    private static Font? Font;

    // Editing states
    private static readonly List<EditorSnowball> EditorSnowballs = new();
    private static readonly List<EditorSnowball> EditorSnowballsToDelete = new();
    private static bool CreatingSnowballs = true;
    private static int HoveredPreview = 0;
    private static int ChosenPreview = -1;

    // Cached text for instructions
    private static Text? BeginText = null;
    private static int CachedChosenPreview = -1;
    private static Text? ChosenPreviewInstructionText = null;
    private static Text? ChosenPreviewQuickText = null;
    private static Dictionary<int, (string, string)> CustomInstructions = new();

    // Game screenshot
    private static Texture? GameTexture = null;
    private static Sprite? GameSprite = null;

    public static void Run()
    {
        Console.WriteLine("Loading data...");
        LoadData();

        Console.WriteLine("Initializing window...");

        PlatformSpecific.InitializeWindowing();

        // Load mini-preview images
        var previewBackgroundTex = new Texture("dogimanip_bg_preview.png");
        var previewForegroundTex = new Texture("dogimanip_fg_preview.png");
        PreviewBackgroundLayer = new Sprite(previewBackgroundTex);
        PreviewForegroundLayer = new Sprite(previewForegroundTex);

        // Load old overlay and background images
        var bgImage = new Texture("dogimanip_bg.png");
        BackgroundSprite = new Sprite(bgImage);
        var oldOverlayImage = new Texture("dogimanip_old_overlay.png");
        var oldOverlaySprite = new Sprite(oldOverlayImage);
        oldOverlaySprite.Color = new Color(0xff, 0xff, 0xff, 80);

        // Load overlay image
        var overlayImage = new Texture("dogimanip_overlay.png");
        var overlaySprite = new Sprite(overlayImage);
        overlaySprite.Color = new Color(0xff, 0xff, 0xff, 80);

        // Make dark rectangle on left side of the screen
        var overlayRectangle = new RectangleShape(new Vector2f(240, 480));
        overlayRectangle.FillColor = new Color(0, 0, 0, 150);

        // Load font
        Font = new Font("8bitoperator_jve.ttf");

        // Title in top left
        var labelText = new Text("Dogi Manip", Font, 32);
        labelText.FillColor = Color.White;
        labelText.Position = new Vector2f(16, 0);

        // Load instruction text
        LoadInstructions(Config.Instance.DogiManip.InstructionFilename);

        // Begin/helping text
        BeginText = new Text(@$"Place snowballs in the area on
the right.

Screenshot game - {Config.Instance.DogiManip.ScreenshotKey}
   (updates background to be the
    screenshot, which you can use
    to place the snowballs)

Choose hovered key - {Config.Instance.DogiManip.ChooseHoveredKey}
   (chooses the seed currently 
    shown by green or red overlay)

Preview select keys - 
   {String.Join(", ", Config.Instance.DogiManip.PreviewSelectKeys.Select(p => p.ToString()).ToArray())}
   (chooses #1 to #6 of the previews)

On Windows, the window will not 
focus unless you use the taskbar. 
This allows you to move in the 
game at the same time.
", Font, 16);
        BeginText.FillColor = Color.White;
        BeginText.Position = new Vector2f(12, 100);

        // Create restart button
        var restartButton = new TextButton(10, 50, 60, 20, "Restart", () =>
        {
            Restart();
        });

        // Set up view and render texture for window
        // We want to draw to a 640x480 canvas, not whatever gets scaled to
        var view = new View();
        view.Size = new Vector2f(640, 480);
        view.Center = new Vector2f(320, 240);
        void updateView(uint width, uint height)
        {
            // Create black bars when resized
            const float viewRatio = 640f / 480f;
            float windowRatio = width / (float)height;
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

            view.Viewport = new FloatRect(posX, posY, sizeX, sizeY);
        }
        updateView(640, 480);
        var renderTex = new RenderTexture(640, 480);

        // Always start with one snowball
        EditorSnowballs.Add(new());

        // Initialize window and its settings
        float windowScale = Config.Instance.WindowScale;
        var window = new RenderWindow(new VideoMode((uint)(640f * windowScale), (uint)(480f * windowScale)), "Dogi Manip Tool", Styles.Default, new ContextSettings { });
        PlatformSpecific.ConfigureAppWindow(window.SystemHandle);
        PlatformSpecific.MoveWindowToGameWindow(window.SystemHandle, true);
        window.SetVerticalSyncEnabled(true);
        window.SetView(view);

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
            updateView(args.Width, args.Height);
            window.SetView(view);
        };

        // Hide console
        PlatformSpecific.HideConsole();
        Program.AutoProgressEnding = true;

        // Main loop
        bool minimizeTogglePressed = false;
        while (window.IsOpen)
        {
            if (Config.Instance.DogiManip.ScreenshotKey.CheckInput())
            {
                // While holding screenshot key, take screenshots and update background
                TakeScreenshot();
            }

            // Process moving to game window, when window is transparent
            if (Config.Instance.WindowTransparent)
            {
                if (Config.Instance.DogiManip.MoveToGameKey.CheckInput())
                {
                    if (PlatformSpecific.IsWindowMinimized(window.SystemHandle))
                    {
                        // Take screenshot before covering the game
                        TakeScreenshot();

                        // Reset beforehand as well
                        Restart();
                    }
                    PlatformSpecific.MoveWindowToGameWindow(window.SystemHandle, false);
                }
            }

            // Process minimize toggle
            bool lastMinimizeTogglePressed = minimizeTogglePressed;
            minimizeTogglePressed = Config.Instance.DogiManip.ToggleMinimizedKey.CheckInput();
            if (minimizeTogglePressed && !lastMinimizeTogglePressed)
            {
                PlatformSpecific.ToggleWindowMinimized(window.SystemHandle);
            }

            // Run window events
            window.DispatchEvents();

            // Update mouse position
            MousePosition = window.MapPixelToCoords(Mouse.GetPosition(window));

            // Draw background
            if (Config.Instance.WindowTransparent)
            {
                renderTex.Clear(new Color(0, 0, 0, 0));
                if (GameSprite != null)
                {
                    // Screenshot being used
                    GameSprite.Color = new Color(255, 255, 255, (byte)Config.Instance.DogiManip.TransparentScreenshotAlpha);
                    renderTex.Draw(GameSprite);
                    overlaySprite.Color = new Color(255, 255, 255, (byte)Config.Instance.DogiManip.TransparentScreenshotAlpha);
                    renderTex.Draw(overlaySprite);
                }
            }
            else
            {
                renderTex.Clear(Color.Black);
                if (GameSprite == null)
                {
                    // No screenshot, use old method
                    renderTex.Draw(BackgroundSprite);
                    renderTex.Draw(oldOverlaySprite);
                }
                else
                {
                    // Screenshot being used, draw that instead
                    renderTex.Draw(GameSprite);
                    renderTex.Draw(overlaySprite);
                }
            }

            // Draw snowballs
            // Note that new snowballs can be added during iteration, so this ignores them
            int snowballCount = EditorSnowballs.Count;
            for (int i = 0; i < snowballCount; i++)
                EditorSnowballs[i].Update(renderTex);

            // Draw overlay on left side of screen, as well as title
            renderTex.Draw(overlayRectangle);
            renderTex.Draw(labelText);

            // Draw buttons
            restartButton.Update(renderTex);

            if (ChosenPreview != -1 && /* just in case there's an error */ ChosenPreview < Previews.Count)
            {
                // Update "hovered" preview to be this one, for later on
                HoveredPreview = ChosenPreview;

                // Draw chosen/final preview
                var preview = Previews[ChosenPreview];

                // Calculate number of times to go up/down, and whether to menu buffer
                int stepCount = preview.Seed.StepCount;
                bool menuBuffer = (stepCount % 2 == 1);
                int upDownTimes = 7 + ((stepCount - 220) / 2) + (menuBuffer ? 1 : 0);

                // Generate text only one time to reduce memory allocations
                if (CachedChosenPreview != ChosenPreview)
                {
                    // Check if there's instructions prewritten for this step count
                    string instructions, quickInstructions;
                    if (CustomInstructions.TryGetValue(stepCount, out var instruction))
                    {
                        instructions = instruction.Item1;
                        quickInstructions = instruction.Item2;
                    }
                    else
                    {
                        instructions = $@"Original basic setup
- Hold right into slope
- Press up and keep holding right
- After hitting, release up
- Press down to get on bridge
   (keep holding right)
{(menuBuffer ? "\n- On bridge, menu buffer up once" : "")}
- Go up/down {upDownTimes} times at bridge

(step count={stepCount})";
                        quickInstructions = $@"{(menuBuffer ? "Menu buffer up\n" : "")}Up/down {upDownTimes} times";
                    }
                    ChosenPreviewInstructionText = new Text(instructions, Font, 16);
                    ChosenPreviewInstructionText.Position = new Vector2f(10, 100);

                    ChosenPreviewQuickText = new Text(quickInstructions, Font, 32);
                    ChosenPreviewQuickText.FillColor = Color.Red;
                    ChosenPreviewQuickText.Position = new Vector2f(10, 320);

                    CachedChosenPreview = ChosenPreview;
                }

                // Actually draw the labels
                renderTex.Draw(ChosenPreviewInstructionText);
                renderTex.Draw(ChosenPreviewQuickText);
            }
            else
            {
                if (Previews.Count == 0)
                {
                    // Draw helping text
                    renderTex.Draw(BeginText);
                }
                else
                {
                    // Draw live previews
                    foreach (var preview in Previews)
                        preview.Update(renderTex);
                }
            }

            // Draw "hovered" (defaults to #1) preview overlay; snowballs overlayed on top
            if (EditorSnowballs.Count >= 3 && HoveredPreview < Previews.Count)
            {
                // Create shape
                CircleShape shape = new(EditorSnowball.CircleRadius, EditorSnowball.CirclePrecision);
                if (HoveredPreview == 0)
                {
                    // Draw green to convey that it's #1, so higher confidence
                    shape.FillColor = new Color(0, 206, 20, 120);
                }
                else
                {
                    // Draw red to convey it's not #1, meaning it was an explicit mouse hover
                    shape.FillColor = new Color(200, 0, 0, 120);
                }
                Vector2f origin = new(shape.Radius, shape.Radius);

                // Draw snowballs from its seed
                var seed = Previews[HoveredPreview].Seed;
                foreach (var snowball in seed.Snowballs)
                {
                    shape.Position = new Vector2f(((snowball.X - ViewX) * 2f) - 1f, ((snowball.Y - ViewY) * 2f) - 1f) - (origin * 0.5f);
                    renderTex.Draw(shape);
                }

                if (Config.Instance.DogiManip.ChooseHoveredKey.CheckInput())
                {
                    // When this key is pressed, select this preview as the final choice
                    if (ChosenPreview != HoveredPreview)
                    {
                        ChosenPreview = HoveredPreview;
                        CreatingSnowballs = false;
                        CachedChosenPreview = -1;
                    }
                }
            }
            
            // Reset the hovered preview every frame
            HoveredPreview = 0;

            // Final draw to the screen
            renderTex.Display();
            if (Config.Instance.WindowTransparent)
                window.Clear(new Color(0, 0, 0, 0));
            else
                window.Clear();
            window.Draw(new Sprite(renderTex.Texture));
            window.Display();

            // Clear mouse states
            MouseLJustPressed = false;
            MouseLJustReleased = false;
            MouseRJustPressed = false;
            MouseRJustReleased = false;

            // Remove snowballs that are marked to be deleted
            bool doNewSearch = (EditorSnowballsToDelete.Count != 0);
            foreach (var snowball in EditorSnowballsToDelete)
                EditorSnowballs.Remove(snowball);
            EditorSnowballsToDelete.Clear();
            if (doNewSearch)
                PerformSearch();

            // If there's no snowball left, make sure that one gets created
            if (CreatingSnowballs && EditorSnowballs.Count == 0)
                EditorSnowballs.Add(new());
        }
    }

    /// <summary>
    /// Loads precomputed snowball position information, and their related RNG seed data
    /// </summary>
    private static void LoadData()
    {
        using FileStream fs = new(Config.Instance.RNG15Bit ? "dogimanip_snowdata_15bit.bin" : "dogimanip_snowdata_16bit.bin", FileMode.Open);
        using BinaryReader br = new(fs);
        while (fs.Position < fs.Length)
        {
            int seedIndex = br.ReadUInt16();
            int stepCount = br.ReadByte();
            int snowballCount = br.ReadByte();
            List<Seed.Snowball> snowballs = new(snowballCount);
            for (int i = 0; i < snowballCount; i++)
            {
                float x = br.ReadSingle();
                float y = br.ReadSingle();
                snowballs.Add(new(x, y));
            }
            Seeds.Add(new(seedIndex, stepCount, snowballs));
        }
    }

    /// <summary>
    /// Searches all seeds to find the best matches, using a scoring system.
    /// </summary>
    private static void PerformSearch()
    {
        // Do calculations
        const int maxSeedsToList = 6;
        List<Seed> highestScoring = new(maxSeedsToList);
        foreach (var seed in Seeds)
        {
            // Calculate seed's score and see if it's one of the highest scoring
            seed.CalculateScore();
            if (highestScoring.Count == 0)
                highestScoring.Add(seed);
            else
            {
                if (highestScoring.Count < maxSeedsToList)
                {
                    // Find insertion point in the list
                    bool inserted = false;
                    for (int i = 0; i < highestScoring.Count; i++)
                    {
                        if (seed.Score > highestScoring[i].Score)
                        {
                            highestScoring.Insert(i, seed);
                            inserted = true;
                            break;
                        }
                    }

                    if (!inserted)
                    {
                        // If not greater than any existing elements, just add to the end
                        highestScoring.Add(seed);
                    }
                }
                else if (seed.Score > highestScoring.Last().Score)
                {
                    // We're higher than the lowest scoring seed on the list, so replace it
                    highestScoring[^1] = seed;

                    // Figure out where this falls on the list
                    for (int i = highestScoring.Count - 2; i >= 0; i--)
                    {
                        Seed comparingAgainst = highestScoring[i];
                        if (seed.Score > comparingAgainst.Score)
                        {
                            // We're higher than the next score on the list, so swap places
                            highestScoring[i] = seed;
                            highestScoring[i + 1] = comparingAgainst;
                        }
                        else
                        {
                            // We're not higher than the next score; stop looking
                            break;
                        }
                    }
                }
            }
        }

        // Create visual previews on the left side of the screen
        MakePreviews(highestScoring);
    }

    private static void MakePreviews(List<Seed> seeds)
    {
        // Remove old previews
        Previews.Clear();

        // Translation of background
        RenderStates backgroundStates = RenderStates.Default;
        backgroundStates.Transform.Translate(new Vector2f(-152, -8));

        // Shape and origin of snowballs
        CircleShape shape = new(EditorSnowball.CircleRadius * 0.5f, EditorSnowball.CirclePrecision);
        Vector2f origin = new(shape.Radius, shape.Radius);

        // Go through each preview, in both rows, and draw them
        int index = 1;
        int row = 0, col = 0;
        foreach (var seed in seeds)
        {
            // Create texture with background
            RenderTexture tex = new RenderTexture(58, 180);
            tex.Clear(Color.Black);
            tex.Draw(PreviewBackgroundLayer, backgroundStates);

            // Draw number label
            var labelText = new Text($"#{index++}", Font, 16);
            labelText.FillColor = index == 2 ? Color.Yellow : Color.Red;
            labelText.OutlineColor = Color.Black;
            labelText.OutlineThickness = 1;
            labelText.Position = new Vector2f(4, -4);
            tex.Draw(labelText);

            // Draw snowballs
            foreach (var snowball in seed.Snowballs)
            {
                shape.Position = new Vector2f((snowball.X - ViewX) - 152, (snowball.Y - ViewY) - 8) - (origin * 0.5f);
                tex.Draw(shape);
            }

            // Draw top layer
            tex.Draw(PreviewForegroundLayer, backgroundStates);
            
            // Mark texture for displaying, and create/add UI button
            tex.Display();
            PreviewButton button = new(20 + (col * 72), 100 + (row * 188), tex, index - 2, seed);
            Previews.Add(button);

            // Advance to next column, and row if necessary
            col++;
            if (col >= 3)
            {
                col = 0;
                row++;
            }
        }
    }

    private static void LoadInstructions(string? filename)
    {
        if (string.IsNullOrEmpty(filename))
            return;

        string[] lines = File.ReadAllLines(filename);

        StringBuilder sb = new();
        StringBuilder sbQuick = new();
        Regex stepRegex = new(@"^\@step\: (\d+)$", RegexOptions.Compiled);
        Regex quickRegex = new(@"^\@quick\: (.*)$", RegexOptions.Compiled);
        int currStep = -1;

        for (int i = 0; i < lines.Length; i++)
        {
            string currLine = lines[i];

            Match m = stepRegex.Match(currLine);
            if (m.Success)
            {
                if (currStep != -1)
                    CustomInstructions[currStep] = (sb.ToString().Trim(), sbQuick.ToString().Trim());
                currStep = int.Parse(m.Groups[1].Value);
                sb.Clear();
                sbQuick.Clear();
                continue;
            }
            m = quickRegex.Match(currLine);
            if (m.Success)
            {
                sbQuick.Append(m.Groups[1].Value);
                sbQuick.AppendLine();
                continue;
            }

            sb.Append(currLine);
            sb.AppendLine();
        }

        if (currStep != -1)
            CustomInstructions[currStep] = (sb.ToString().Trim(), sbQuick.ToString().Trim());
    }

    public static void Restart()
    {
        EditorSnowballs.Clear();
        EditorSnowballs.Add(new());
        CreatingSnowballs = true;
        Previews.Clear();
        ChosenPreview = -1;
        CachedChosenPreview = -1;
        HoveredPreview = 0;
    }

    public static void TakeScreenshot()
    {
        var screenshot = PlatformSpecific.TakeScreenshotOfGame();
        if (screenshot.Data != null)
        {
            // Dispose of old screenshots
            GameSprite?.Dispose();
            GameTexture?.Dispose();

            // Load new screenshot into SFML
            GameTexture = new Texture(screenshot.Data);
            GameSprite = new Sprite(GameTexture);

            // Scale screenshot to 640x480
            GameSprite.Scale = new Vector2f(640f / GameTexture.Size.X, 480f / GameTexture.Size.Y);
        }
    }
}