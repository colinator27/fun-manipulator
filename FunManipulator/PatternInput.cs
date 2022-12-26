using System.Text.RegularExpressions;

namespace FunManipulator;

public static class PatternInput
{
    public static Search.Pattern ConfigInputMode()
    {
        var mode = Config.Instance.SeedFinder.InputMode;

        if (mode == Config.SeedFinderConfig.PatternInputMode.None)
        {
            // Allow user to select a mode
            Console.WriteLine("Available input modes:");
            Console.WriteLine(" - basic");
            Console.WriteLine(" - dust");
            Console.Write("Please input mode: ");
            while (mode == Config.SeedFinderConfig.PatternInputMode.None)
            {
                string? str = Console.ReadLine();
                if (str != null)
                {
                    switch (str.ToLowerInvariant())
                    {
                        case "basic":
                            mode = Config.SeedFinderConfig.PatternInputMode.Basic;
                            break;
                        case "dust":
                            mode = Config.SeedFinderConfig.PatternInputMode.Dust;
                            break;
                        default:
                            Console.WriteLine("Invalid option. Defaulting to basic.");
                            mode = Config.SeedFinderConfig.PatternInputMode.Basic;
                            break;
                    }
                }
            }
        }

        // Return proper pattern
        return mode switch
        {
            Config.SeedFinderConfig.PatternInputMode.Dust => DustPattern(),
            _ => Basic(),
        };
    }

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
        Console.WriteLine("To end, enter \"end\", \"quit\", or \"exit\".");

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

    private static Regex _dustPatternRegex = new(@"(\d+) (\d+)");
    public static Search.Pattern DustPattern()
    {
        Search.Pattern res = new();

        Console.Write($"Enter dust ID (default {Config.Instance.SeedFinder.DefaultDustID}): ");
        if (!int.TryParse(Console.ReadLine(), out int dustId))
            dustId = 31;

        Console.Write($"Enter dust frame (default {Config.Instance.SeedFinder.DefaultFrame}): ");
        if (!int.TryParse(Console.ReadLine(), out int dustFrame))
            dustFrame = -1;

        Console.Write($"Enter dust X offset (default {Config.Instance.SeedFinder.DefaultDustX}): ");
        if (!int.TryParse(Console.ReadLine(), out int dustXOffset))
            dustXOffset = Config.Instance.SeedFinder.DefaultDustX;
        Console.Write($"Enter dust Y offset (default {Config.Instance.SeedFinder.DefaultDustY}): ");
        if (!int.TryParse(Console.ReadLine(), out int dustYOffset))
            dustYOffset = Config.Instance.SeedFinder.DefaultDustY;

        if (dustId < 0 || dustId > DustParticles.Images.Count)
        {
            Console.WriteLine("Invalid dust ID. Using default...");
            dustId = Config.Instance.SeedFinder.DefaultDustID;
        }

        DustImage image = DustParticles.Images[dustId];

        if (dustFrame < 0 || dustFrame > image.Frames.Count)
        {
            Console.WriteLine("Invalid dust frame. Using default...");
            dustFrame = Config.Instance.SeedFinder.DefaultFrame;
        }

        DustFrame frame;
        if (dustFrame <= -1)
        {
            // Find last non-empty frame by default
            int i = image.Frames.Count - 1;
            do
            {
                frame = image.Frames[i];
                i--;
            }
            while (i > 0 && frame.Particles.Count == 0);

            // For every integer less than -1, go back an extra frame
            i -= Math.Abs(dustFrame) - 1;
            if (i < 0)
                frame = image.Frames[0];
            else
                frame = image.Frames[i];
        }
        else
        {
            frame = image.Frames[dustFrame];
        }

        Console.WriteLine("Enter X/Y positions of each dust particle, one line at a time, separated with spaces.");
        Console.WriteLine("To end, enter \"end\", \"quit\", or \"exit\".");

        List<DustParticle> recordedParticles = new();
        while (true)
        {
            string line = Console.ReadLine()?.Trim() ?? "";
            if (line.Contains(';'))
                line = line[..line.IndexOf(';')];
            if (string.IsNullOrEmpty(line))
                continue;
            line = line.ToLowerInvariant();
            if (line == "end" || line == "quit" || line == "exit")
                break;
            Match match = _dustPatternRegex.Match(line);
            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int x) &&
                    int.TryParse(match.Groups[2].Value, out int y))
                {
                    recordedParticles.Add(new(x - dustXOffset, y - dustYOffset, 1));
                    continue;
                }
            }
            Console.WriteLine("Invalid input; ignoring.");
        }

        res.Elements.Add(new Search.ElementExtLastDustFrame(frame.Particles, recordedParticles));

        return res;
    }
}