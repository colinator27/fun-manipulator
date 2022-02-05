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
                if (!NamingLetters.GetSeed(out seed, out int pos))
                    return;
                Console.WriteLine($"Found seed: {seed} (at position {pos})");
            }
        }


        while (true)
        {
            if (!ConsoleHelpers.ReadYesNo("Verify a pattern?"))
                break;

            double range = ConsoleHelpers.ReadLineDoubleMin("Enter random() range (>= 0): ", 0);
            double minInclusive = ConsoleHelpers.ReadLineDoubleMin("Enter minimum inclusive value (>= 0): ", 0);
            double maxExclusive = ConsoleHelpers.ReadLineDoubleRange($"Enter maximum exclusive value (in range): ", minInclusive, range);

            Console.WriteLine("Enter estimated random() ranges.");
            Console.WriteLine("To enter boolean states, use T/F.");
            Console.WriteLine("To enter unknowns, use U.");
            Console.WriteLine("To end, enter blank line.");
            Search.Pattern pattern = new();
            while (true)
            {
                string line = Console.ReadLine()?.Trim() ?? "";
                if (string.IsNullOrEmpty(line))
                    break;
                if (line.ToLowerInvariant() == "t")
                {
                    pattern.Elements.Add(new Search.ElementRandomInRange(range, minInclusive, maxExclusive));
                    continue;
                }
                if (line.ToLowerInvariant() == "f")
                {
                    pattern.Elements.Add(new Search.ElementRandomInRange(range, minInclusive, maxExclusive) { Inverted = true });
                    continue;
                }
                if (line.ToLowerInvariant() == "u")
                {
                    pattern.Elements.Add(new Search.ElementUnknown(false));
                    continue;
                }
                if (double.TryParse(line, out double val))
                {
                    if (val < 0 || val > range)
                        Console.WriteLine("Warning: Value outside of range, ignoring.");
                    else
                    {
                        if (val >= minInclusive && val < maxExclusive)
                            pattern.Elements.Add(new Search.ElementRandomInRange(range, minInclusive, maxExclusive));
                        else
                            pattern.Elements.Add(new Search.ElementRandomInRange(range, minInclusive, maxExclusive) { Inverted = true });
                    }
                    continue;
                }
                Console.WriteLine("Warning: Invalid format, ignoring.");
            }

            Console.WriteLine("Searching...");
            int pos = Search.TryFindPattern(seed, pattern, 0, Config.Instance.SeedFinder.SearchRange);
            if (pos != -1)
                Console.WriteLine($"Verified pattern at position {pos}");
            else
                Console.WriteLine("Failed to verify pattern within search range");
        }
    }
}