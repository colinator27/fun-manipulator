namespace FunManipulator;

public static class SeedFinder
{
    public static void Run()
    {
        uint seed;
        if (Config.Instance.SeedFinder.Seed >= 0)
        {
            seed = (uint)Config.Instance.SeedFinder.Seed;
            Console.WriteLine($"Using seed {seed}");
        }
        else
        {
            if (ConsoleHelpers.ReadYesNo("Input existing seed?"))
            {
                seed = ConsoleHelpers.ReadLineUInt("Enter seed: ");
            }
            else
            {
                if (ConsoleHelpers.ReadYesNo("Use naming screen?"))
                {
                    if (!NamingLetters.GetSeed(out seed, out int pos))
                        return;
                    Console.WriteLine($"Found seed: {seed} (at position {pos})");
                }
                else
                {
                    Console.WriteLine("Searching for seeds...");
                    Search.Pattern pattern = PatternInput.ConfigInputMode();
                    List<(uint, int)> seedList = new();
                    if (!Search.TryFindSeedWithinRange(pattern, 0, Config.Instance.SeedFinder.SearchRangeFind, out seed, out int pos, seedList))
                    {
                        if (seedList.Count != 0)
                        {
                            Console.WriteLine("Found multiple seeds:");
                            foreach ((uint, uint) s in seedList)
                                Console.WriteLine($" -> {s.Item1} (at {s.Item2})");
                        }
                        else
                        {
                            Console.WriteLine("No seeds matched!");
                        }
                        return;
                    }
                    Console.WriteLine($"Found seed: {seed} (at position {pos})");
                }
            }
        }


        while (true)
        {
            if (!ConsoleHelpers.ReadYesNo("Verify a pattern?"))
                break;

            Search.Pattern pattern = PatternInput.ConfigInputMode();

            Console.WriteLine("Searching...");
            List<int> pos = Search.TryFindPattern(seed, pattern, 0, Config.Instance.SeedFinder.SearchRange);
            if (pos.Count != 0)
            {
                Console.WriteLine($"Verified pattern at positions:");
                foreach (int i in pos)
                    Console.WriteLine($" -> {i}");
            }
            else
                Console.WriteLine("Failed to verify pattern within search range");
        }
    }
}