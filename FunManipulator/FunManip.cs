namespace FunManipulator;

public static class FunManip
{
    public static void Run()
    {
        Console.WriteLine("Initializing audio...");
        Beeps.BuildData();

        Console.Write("Enter minimum fun value (inclusive): ");
        int minFun;
        if (Config.Instance.FunManip.ForceFunMinimum != -1)
        {
            minFun = Config.Instance.FunManip.ForceFunMinimum;
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
        if (Config.Instance.FunManip.ForceFunMaximum != -1)
        {
            maxFun = Config.Instance.FunManip.ForceFunMaximum;
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

        if (!NamingLetters.GetSeed(out uint seed, out int pos))
            return;
        Console.WriteLine($"Found seed ({seed}). Searching for fun values...");
        var elem = new Search.ElementRandomInRange(100, minFun - 1, maxFun - 1);
        int resultFrames = -1;
        int consecutive;
        List<uint[]> surrounding = new();
        for (consecutive = 4; consecutive > 0; consecutive--)
        {
            const int minimumFrames = 30 * 10;
            int offset = pos + Config.Instance.FunManip.FunSearchOffset + 104 + (minimumFrames * 3); // Delay by at least 5 seconds

            // Now find two consecutive values we want
            int distance = Search.FindConsecutiveSingle(seed, offset, Config.Instance.FunManip.FunSearchRange, elem, consecutive, 3, surrounding);
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
        const double timeForFrameMs = ((1.0 / 30.0) * 1000);
        int targetMilliseconds = (int)(((resultFrames + Config.Instance.BeepFrameOffset) * timeForFrameMs) + 
                                        Config.Instance.BeepMsOffset - (Beeps.Start * 1000));
        int actualMs = (int)(resultFrames * timeForFrameMs);
        int earlyMs = actualMs - (int)((consecutive / 2.0) * timeForFrameMs) + Config.Instance.BeepEarlyMs;
        int lateMs = actualMs + (int)((consecutive / 2.0) * timeForFrameMs) + Config.Instance.BeepLateMs;
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
        Beeps.Sound?.Play();

        Console.WriteLine("Played sound.");

        while (clock.ElapsedTime.AsMilliseconds() < endMs)
        {
            Thread.Sleep(1);
            if (SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.Z) || SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.Enter))
            {
                int time = clock.ElapsedTime.AsMilliseconds();
                if (time <= earlyMs)
                    Console.WriteLine($"Warning: The press was possibly early (delta={time - actualMs} ms)");
                else if (time >= lateMs)
                    Console.WriteLine($"Warning: The press was possibly late (delta={time - actualMs} ms)");
                else
                    Console.WriteLine($"The press was likely accurate! (delta={time - actualMs} ms)");
                break;
            }
        }

        while (clock.ElapsedTime.AsMilliseconds() < endMs)
            Thread.Sleep(5);
        Beeps.Cleanup();
    }
}