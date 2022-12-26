namespace FunManipulator;

public class NamingLetters
{
    public static bool GetSeed(out uint seed, out int pos)
    {
        Console.WriteLine("Enter positions of letters on first three rows.");
        Console.WriteLine("Enter 1 for no letter visibility.");
        Console.WriteLine("Enter 2 for only vertical lines visible.");
        Console.WriteLine("Enter 3 for only horizontal lines visible.");
        Console.WriteLine("Enter 4 for horizontal and vertical lines visible.");

        while (true)
        {
            char curr = 'A';
            Search.Pattern pattern = new();
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
                        pattern.Elements.Add(new Search.ElementRandomInRange(1, 0, 0.5));
                        pattern.Elements.Add(new Search.ElementRandomInRange(1, 0, 0.5));
                        break;
                    }
                    if (keyStates[1] && !keyStatesLast[1])
                    {
                        Console.Write($"\r '{curr}' => vertical lines");
                        Console.WriteLine();
                        pattern.Elements.Add(new Search.ElementRandomInRange(1, 0, 0.5));
                        pattern.Elements.Add(new Search.ElementRandomInRange(1, 0.5, 1));
                        break;
                    }
                    if (keyStates[2] && !keyStatesLast[2])
                    {
                        Console.Write($"\r '{curr}' => horizontal lines");
                        Console.WriteLine();
                        pattern.Elements.Add(new Search.ElementRandomInRange(1, 0.5, 1));
                        pattern.Elements.Add(new Search.ElementRandomInRange(1, 0, 0.5));
                        break;
                    }
                    if (keyStates[3] && !keyStatesLast[3])
                    {
                        Console.Write($"\r '{curr}' => vertical and horizontal lines");
                        Console.WriteLine();
                        pattern.Elements.Add(new Search.ElementRandomInRange(1, 0.5, 1));
                        pattern.Elements.Add(new Search.ElementRandomInRange(1, 0.5, 1));
                        break;
                    }
                    Thread.Sleep(33);
                }
                Thread.Sleep(33 * 2);
                curr++;
            }

            Console.WriteLine("Searching for seed...");
            if (!Search.TryFindSeedWithinRange(pattern, Config.Instance.NamingSearchStart, 
                                               Config.Instance.NamingSearchRange, out seed, out pos))
            {
                Console.WriteLine("Unable to find individual seed with the pattern.");
                Console.WriteLine("Retry? Press Y or N.");
                Thread.Sleep(500);
                while (true)
                {
                    if (SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.Y))
                        break;
                    if (SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.N))
                        return false;
                    Thread.Sleep(33);
                }
            }
            else
                break;
        }

        return true;
    }
}