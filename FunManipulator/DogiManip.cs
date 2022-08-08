using SFML.Graphics;
using SFML.System;
using SFML.Window;

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

            SnowballsRightOfPuzzle = 0;
            foreach (var snowball in Snowballs)
            {
                if (snowball.X >= 160)
                    SnowballsRightOfPuzzle++;
            }
        }

        public void CalculateScore()
        {
            const float maxDistance = 8f;
            const float numRightBias = 0.25f;

            // Reset snowball distances, count number to right of puzzle
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
        private const int circlePrecision = 8;
        private static CircleShape shape = new(CircleRadius, circlePrecision);
        private static Vector2f origin = new(CircleRadius, CircleRadius);

        public Vector2f Position { get; set; }
        public bool Active { get; set; } = true;

        public float ClosestDistance { get; set; } = -1;

        public void Update(RenderTexture texture)
        {
            Color color = Color.White;

            if (Active)
            {
                // Follow mouse
                Position = MousePosition;

                // Don't process or display "offscreen"
                if (Position.X < 240)
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
                    // Check for deletion
                    if (MouseRJustReleased)
                    {
                        // todo
                    }
                }

                if (!CreatingSnowballs)
                {
                    if (hovering)
                    {
                        // Hover color
                        color = Color.Yellow;
                        if (MouseLJustPressed)
                        {
                            // Select
                            Active = true;
                        }
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

        public Button(float x, float y, float width, float height)
        {
            shape = new RectangleShape(new Vector2f(width, height));
            shape.Position = new Vector2f(x, y);
        }

        public virtual void Update(RenderTexture texture)
        {
            // Determine color
            bool hovered = (MousePosition.X >= shape.Position.X &&
                            MousePosition.Y >= shape.Position.Y &&
                            MousePosition.X <= shape.Position.X + shape.Size.X &&
                            MousePosition.Y <= shape.Position.Y + shape.Size.Y);
            Color color;
            if (hovered)
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
        }

        public override void OnClick()
        {
            ChosenPreview = Index;
        }
    }

    private const float ViewX = 0, ViewY = 580 - 240;

    private static readonly List<Seed> Seeds = new();
    private static Vector2f MousePosition = new Vector2f(0, 0);
    private static bool MouseLJustPressed = false;
    private static bool MouseLJustReleased = false;
    private static bool MouseRJustPressed = false;
    private static bool MouseRJustReleased = false;
    private static readonly List<EditorSnowball> EditorSnowballs = new();
    private static bool CreatingSnowballs = true;
    private static List<PreviewButton> Previews = new();
    private static Sprite? BackgroundSprite;
    private static Sprite? BackgroundLayer1;
    private static Sprite? BackgroundLayer2;
    private static int ChosenPreview = -1;

    private static Font? Font;

    public static void Run()
    {
        Console.WriteLine("Loading data...");
        LoadData();

        Console.WriteLine("Initializing window...");

        const int windowScale = 2;

        var bgImage = new Texture("dogimanip_bg.png");
        var bgImageLayer1 = new Texture("dogimanip_bg_layer1.png");
        var bgImageLayer2 = new Texture("dogimanip_bg_layer2.png");
        var overlayImage = new Texture("dogimanip_overlay.png");
        BackgroundSprite = new Sprite(bgImage);
        BackgroundLayer1 = new Sprite(bgImageLayer1);
        BackgroundLayer2 = new Sprite(bgImageLayer2);
        var overlaySprite = new Sprite(overlayImage);
        overlaySprite.Color = new Color(0xff, 0xff, 0xff, 80);
        var overlayRectangle = new RectangleShape(new Vector2f(240, 480));
        overlayRectangle.FillColor = new Color(0, 0, 0, 120);

        Font = new Font("8bitoperator_jve.ttf");

        var labelText = new Text("Dogi Manip", Font, 32);
        labelText.FillColor = Color.White;
        labelText.Position = new Vector2f(16, 0);

        var restartButton = new TextButton(10, 50, 60, 20, "Restart", () =>
        {
            EditorSnowballs.Clear();
            EditorSnowballs.Add(new());
            CreatingSnowballs = true;
            Previews.Clear();
            ChosenPreview = -1;
        });

        var view = new View(new FloatRect(0, 0, 640, 480));
        var renderTex = new RenderTexture(640, 480);

        EditorSnowballs.Add(new());

        var window = new RenderWindow(new VideoMode(640 * windowScale, 480 * windowScale), "Dogi Manip Tool", Styles.Default, new ContextSettings { });
        window.SetVerticalSyncEnabled(true);
        window.SetView(view);
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
        window.KeyPressed += (ev, args) =>
        {
            if (args.Code == Keyboard.Key.Enter)
            {
                if (CreatingSnowballs)
                {
                    if (EditorSnowballs.Last().Active)
                        EditorSnowballs.RemoveAt(EditorSnowballs.Count - 1);
                    CreatingSnowballs = false;
                }

                PerformSearch();
            }
        };
        while (window.IsOpen)
        {
            MousePosition = window.MapPixelToCoords(Mouse.GetPosition(window));

            window.DispatchEvents();
            renderTex.Clear(Color.Black);
            renderTex.Draw(BackgroundSprite);
            renderTex.Draw(overlaySprite);
            int snowballCount = EditorSnowballs.Count;
            for (int i = 0; i < snowballCount; i++)
                EditorSnowballs[i].Update(renderTex);
            renderTex.Draw(overlayRectangle);
            renderTex.Draw(labelText);

            restartButton.Update(renderTex);

            if (ChosenPreview != -1 && /* just in case there's an error */ ChosenPreview < Previews.Count)
            {
                var preview = Previews[ChosenPreview];
                int stepCount = preview.Seed.StepCount;
                var text = new Text($"WIP setup instructions\nMore will be here later\n\nstep count={stepCount}\n{(stepCount % 2 == 0 ? "second/bottom pixel" : "first/top pixel")}\ngo up/down {12 + ((stepCount - 220) / 2)} times at bridge", Font, 16);
                text.Position = new Vector2f(10, 100);
                renderTex.Draw(text);
            }
            else
            {
                foreach (var preview in Previews)
                    preview.Update(renderTex);
            }

            renderTex.Display();

            window.Clear();
            window.Draw(new Sprite(renderTex.Texture));
            window.Display();

            MouseLJustPressed = false;
            MouseLJustReleased = false;
            MouseRJustPressed = false;
            MouseRJustReleased = false;
        }
    }

    private static void LoadData()
    {
        using (FileStream fs = new FileStream("dogimanip_snowdata_15bit.bin", FileMode.Open))
        {
            using (BinaryReader br = new BinaryReader(fs))
            {
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
        }
    }

    private static void PerformSearch()
    {
        // Do calculations
        const int maxSeedsToList = 6;
        List<Seed> highestScoring = new(maxSeedsToList);
        foreach (var seed in Seeds)
        {
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

        MakePreviews(highestScoring);
    }

    private static void MakePreviews(List<Seed> seeds)
    {
        Previews.Clear();

        RenderStates backgroundStates = RenderStates.Default;
        backgroundStates.Transform.Translate(new Vector2f(-152, -8));

        const int circlePrecision = 8;
        CircleShape shape = new(EditorSnowball.CircleRadius * 0.5f, circlePrecision);
        Vector2f origin = new(shape.Radius, shape.Radius);

        int index = 1;
        int row = 0, col = 0;
        foreach (var seed in seeds)
        {
            RenderTexture tex = new RenderTexture(58, 180);
            tex.Clear(Color.Black);
            tex.Draw(BackgroundLayer1, backgroundStates);

            var labelText = new Text($"#{index++}", Font, 16);
            labelText.FillColor = index == 2 ? Color.Yellow : Color.Red;
            labelText.OutlineColor = Color.Black;
            labelText.OutlineThickness = 1;
            labelText.Position = new Vector2f(4, -4);
            tex.Draw(labelText);

            foreach (var snowball in seed.Snowballs)
            {
                shape.Position = new Vector2f((snowball.X - ViewX) - 152, (snowball.Y - ViewY) - 8) - (origin * 0.5f);
                tex.Draw(shape);
            }

            tex.Draw(BackgroundLayer2, backgroundStates);

            tex.Display();

            PreviewButton button = new(20 + (col * 72), 100 + (row * 188), tex, index - 2, seed);

            col++;
            if (col >= 3)
            {
                col = 0;
                row++;
            }

            Previews.Add(button);
        }
    }
}