using FunManipulator;

try
{
    Console.WriteLine("Loading config...");
    Config.Load("config.json");

    Console.WriteLine("Initializing RNG...");
    RNG.Initialize(Config.Instance.RNG15Bit, Config.Instance.RNGOldPoly);

    Console.WriteLine("Initializing audio...");
    Beeps.BuildData();

    Console.Write("Enter minimum fun value (inclusive): ");
    int minFun;
    if (Config.Instance.ForceFunMinimum != -1)
    {
        minFun = Config.Instance.ForceFunMinimum;
        if (minFun < 1 || minFun > 100)
        {
            Console.WriteLine("Error: Forced minimum fun value is out of range 1-100!");
            return;
        }
    }
    else
    {
        while (!int.TryParse(Console.ReadLine(), out minFun) || minFun < 1 || minFun > 100)
        {
            Console.WriteLine("Please enter an integer within range 1-100.");
            Console.Write("Enter minimum fun value (inclusive): ");
        }
    }

    Console.Write("Enter maximum fun value (exclusive): ");
    int maxFun;
    if (Config.Instance.ForceFunMaximum != -1)
    {
        maxFun = Config.Instance.ForceFunMaximum;
        if (maxFun <= minFun || maxFun > 101)
        {
            Console.WriteLine($"Error: Forced maximum fun value is out of range {minFun + 1}-101!");
            return;
        }
    }
    else
    {
        while (!int.TryParse(Console.ReadLine(), out maxFun) || maxFun <= minFun || maxFun > 101)
        {
            Console.WriteLine($"Please enter an integer within range {minFun + 1}-101.");
            Console.Write("Enter maximum fun value (exclusive): ");
        }
    }

    Console.WriteLine("Waiting for spacebar input...");
    while (true)
    {
        if (SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.Space))
            break;
        Thread.Sleep(33);
    }
    Thread.Sleep(33 * 2);

    Console.WriteLine("Enter positions of letters on first three rows.");
    Console.WriteLine("Enter 1 for no letter visibility.");
    Console.WriteLine("Enter 2 for only vertical lines visible.");
    Console.WriteLine("Enter 3 for only horizontal lines visible.");
    Console.WriteLine("Enter 4 for horizontal and vertical lines visible.");

    uint seed;
    int pos;
    while (true)
    {
        char curr = 'A';
        bool[] pattern = new bool[42];
        bool[] keyStates = new bool[4];
        bool[] keyStatesLast = new bool[4];
        for (int i = 0; i < 21; i++)
        {
            Console.Write($"Enter value for '{curr}'");
            while (true)
            {
                for (int j = 0; j < 4; j++)
                    keyStatesLast[j] = keyStates[j];
                keyStates[0] = SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.Num1);
                keyStates[1] = SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.Num2);
                keyStates[2] = SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.Num3);
                keyStates[3] = SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.Num4);
                if (keyStates[0] && !keyStatesLast[0])
                {
                    Console.Write($"\r '{curr}' => not visible");
                    Console.WriteLine();
                    break;
                }
                if (keyStates[1] && !keyStatesLast[1])
                {
                    Console.Write($"\r '{curr}' => vertical lines");
                    Console.WriteLine();
                    pattern[(i * 2) + 1] = true;
                    break;
                }
                if (keyStates[2] && !keyStatesLast[2])
                {
                    Console.Write($"\r '{curr}' => horizontal lines");
                    Console.WriteLine();
                    pattern[i * 2] = true;
                    break;
                }
                if (keyStates[3] && !keyStatesLast[3])
                {
                    Console.Write($"\r '{curr}' => vertical and horizontal lines");
                    Console.WriteLine();
                    pattern[i * 2] = true;
                    pattern[(i * 2) + 1] = true;
                    break;
                }
                Thread.Sleep(33);
            }
            Thread.Sleep(33 * 2);
            curr++;
        }

        Console.WriteLine("Searching for seed...");
        (long letterMin, long letterMax) = RNG.GetRange(1, 0.5, 1);
        if (!Search.TryFindSeedWithinRange(pattern, 0, 40000, letterMin, letterMax, out seed, out pos))
        {
            Console.WriteLine("Unable to find individual seed with the pattern.");
            Console.WriteLine("Retry? Press Y or N.");
            Thread.Sleep(500);
            while (true)
            {
                if (SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.Y))
                    break;
                if (SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.N))
                    return;
                Thread.Sleep(33);
            }
        }
        else
            break;
    }

    Console.WriteLine($"Found seed ({seed}). Searching for fun values...");
    (long min, long max) = RNG.GetRange(100, minFun - 1, maxFun - 1);
    int resultFrames = -1;
    int consecutive;
    List<uint[]> surrounding = new();
    for (consecutive = 4; consecutive > 0; consecutive--)
    {
        const int minimumFrames = 30 * 10;
        int offset = pos + Config.Instance.FunSearchOffset + 104 + (minimumFrames * 3); // Delay by at least 5 seconds

        // Now find two consecutive values we want
        int distance = Search.FindConsecutive(seed, offset, Config.Instance.FunSearchRange, min, max, consecutive, 3, surrounding);
        if (distance != -1)
        {
            Console.WriteLine($"Found, distance={distance}, {consecutive} frames in a row. Results in waiting {(minimumFrames + (distance / 3) + (consecutive / 2)) / 30.0:F4} seconds");
            resultFrames = (distance / 3) + minimumFrames + (consecutive / 2) - 180;
            Console.WriteLine($"Frame count is {resultFrames}");
            Console.Write("Surrounding: ");
            foreach (uint val in surrounding[0])
                Console.Write((Math.Floor(RNG.ValueToDouble(val, 100)) + 1).ToString().PadRight(4));
            Console.WriteLine();
            foreach (uint[] arr in surrounding.Skip(1))
            {
                Console.Write("Surrounding (wrong offset): ");
                foreach (uint val in arr)
                    Console.Write((Math.Floor(RNG.ValueToDouble(val, 100)) + 1).ToString().PadRight(4));
                Console.WriteLine();
            }
            break;
        }
    }
    if (resultFrames == -1)
    {
        Console.WriteLine("Very unlucky, couldn't find fun value.");
        return;
    }

    Console.WriteLine("Waiting for alt input...");
    const int frameOffset = -4;
    const double timeForFrameMs = ((1.0 / 30.0) * 1000);
    int targetMilliseconds = (int)(((resultFrames + frameOffset) * timeForFrameMs) - (Beeps.Start * 1000));
    int earlyMs = (int)(((resultFrames - 1) * timeForFrameMs) - (timeForFrameMs * (consecutive * 0.5)));
    int lateMs = (int)(((resultFrames - 1) * timeForFrameMs) + (timeForFrameMs * (consecutive * 0.5)));
    int endMs = (int)(lateMs + (timeForFrameMs * 30 * 10));
    while (true)
    {
        if (SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.LAlt) || SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.RAlt))
            break;
        Thread.Sleep(2);
    }

    SFML.System.Clock clock = new();
    Console.WriteLine("Timer started.");
    while (clock.ElapsedTime.AsMilliseconds() < targetMilliseconds - (int)timeForFrameMs)
        Thread.Sleep(1);
    while (clock.ElapsedTime.AsMilliseconds() < targetMilliseconds) { }
    Beeps.Sound.Play();

    Console.WriteLine("Played sound.");

    while (clock.ElapsedTime.AsMilliseconds() < endMs)
    {
        Thread.Sleep(1);
        if (SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.Z) || SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.Enter))
        {
            int time = clock.ElapsedTime.AsMilliseconds();
            if (time <= earlyMs)
                Console.WriteLine("Warning: The press was likely early");
            else if (time >= lateMs)
                Console.WriteLine("Warning: The press was likely late");
            else
                Console.WriteLine("The press was likely accurate!");
            break;
        }
    }

    while (clock.ElapsedTime.AsMilliseconds() < endMs)
        Thread.Sleep(5);
    Beeps.Sound.Dispose();
    Beeps.Buffer.Dispose();
    Console.WriteLine("Completed.");
}
catch (Exception e)
{
    Console.WriteLine($"Exception: {e}");
}