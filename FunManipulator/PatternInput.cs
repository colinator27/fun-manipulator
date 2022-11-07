namespace FunManipulator;

public static class PatternInput
{
    public static Search.Pattern Basic()
    {
        double range = ConsoleHelpers.ReadLineDoubleMin("Enter random() range (>= 0): ", 0);
        double minInclusive = ConsoleHelpers.ReadLineDoubleMin("Enter minimum inclusive value (>= 0): ", 0);
        double maxExclusive = ConsoleHelpers.ReadLineDoubleRange($"Enter maximum exclusive value (in range): ", minInclusive, range);

        Console.WriteLine("Enter estimated random() ranges, or other RNG patterns.");
        Console.WriteLine("To enter boolean states, use T/F.");
        Console.WriteLine("To enter greater/lesser than previous, use G/L.");
        Console.WriteLine("To enter unknowns, use U. Use UI for unknown irandom() calls.");
        Console.WriteLine("To enter new range, enter \"range\".");
        Console.WriteLine("To enter later pattern, enter \"later\", with opt. search range.");
        Console.WriteLine("To enter chain of unknowns, enter \"skip\" with number of calls.");
        Console.WriteLine("To end, enter \"end\" or \"quit\".");

        Search.Pattern rootPattern = new();
        Search.Pattern pattern = rootPattern;
        while (true)
        {
            string line = Console.ReadLine()?.Trim() ?? "";
            if (line.Contains(';'))
                line = line[..line.IndexOf(';')];
            if (string.IsNullOrEmpty(line))
                continue;
            line = line.ToLowerInvariant();
            if (line == "end" || line == "quit" || line == "exit")
            {
                if (pattern.Parent != null)
                {
                    pattern = pattern.Parent;
                    continue;
                }
                else
                    break;
            }
            if (line == "range")
            {
                range = ConsoleHelpers.ReadLineDoubleMin("Enter random() range (>= 0): ", 0);
                minInclusive = ConsoleHelpers.ReadLineDoubleMin("Enter minimum inclusive value (>= 0): ", 0);
                maxExclusive = ConsoleHelpers.ReadLineDoubleRange($"Enter maximum exclusive value (in range): ", minInclusive, range);
                continue;
            }
            if (line == "t")
            {
                pattern.Elements.Add(new Search.ElementRandomInRange(range, minInclusive, maxExclusive));
                continue;
            }
            if (line == "f")
            {
                pattern.Elements.Add(new Search.ElementRandomInRange(range, minInclusive, maxExclusive) { Inverted = true });
                continue;
            }
            if (line == "u")
            {
                pattern.Elements.Add(new Search.ElementUnknown(false));
                continue;
            }
            if (line == "ui")
            {
                pattern.Elements.Add(new Search.ElementUnknown(true));
                continue;
            }
            if (line == "g")
            {
                pattern.Elements.Add(new Search.ElementGreaterThanPrevious());
                continue;
            }
            if (line == "l")
            {
                pattern.Elements.Add(new Search.ElementLesserThanPrevious());
                continue;
            }
            if (line == "later")
            {
                pattern = new Search.Pattern() { Parent = pattern };
                if (int.TryParse(line[5..], out int searchRange))
                    pattern.Elements.Add(new Search.ElementLaterPattern(pattern, searchRange));
                else
                    pattern.Elements.Add(new Search.ElementLaterPattern(pattern));
                continue;
            }
            if (line == "skip")
            {
                if (int.TryParse(line[4..], out int amount))
                {
                    pattern.Elements.Add(new Search.ElementSkip(amount));
                    continue;
                }
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

        return rootPattern;
    }
}