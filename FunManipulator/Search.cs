using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace FunManipulator;

public static class Search
{
    public sealed class Pattern
    {
        public Pattern? Parent { get; set; } = null; 
        public List<IElement> Elements { get; init; }
        public bool AnyOrder { get; set; } = false;
        public int AllowedErrors { get; set; } = 0;

        private Stack<List<IElement>>? _stack = null;

        public Pattern()
        {
            Elements = new List<IElement>();
        }

        public int GetSize()
        {
            int size = 0;
            foreach (var elem in Elements)
                size += elem.GetSize();
            return size;
        }

        public int CheckFirst(uint[] rng, int startIndex, int endIndexExclusive)
        {
            if (AnyOrder)
            {
                // TODO: Might want to reimplement Check() to avoid the List allocation, but fine for now
                List<int> list = Check(rng, startIndex, endIndexExclusive);
                if (list.Count == 0)
                    return -1;
                return list[0];
            }
            else
            {
                // Search through all of the RNG array, looking for the pattern in the correct order
                int size = GetSize();
                for (int i = startIndex; i <= endIndexExclusive - size; i++)
                {
                    int errors = 0;
                    int currIndex = i;
                    foreach (var elem in Elements)
                    {
                        if (!elem.Check(rng, ref currIndex))
                        {
                            if (++errors > AllowedErrors)
                                break;
                        }
                    }
                    if (errors <= AllowedErrors)
                        return i;
                }
                return -1;
            }
        }

        public List<int> Check(uint[] rng, int startIndex, int endIndexExclusive)
        {
            List<int> results = new();
            if (AnyOrder)
            {
                _stack ??= new();
                throw new NotImplementedException();
            }
            else
            {
                // Search through all of the RNG array, looking for the pattern in the correct order
                int size = GetSize();
                for (int i = startIndex; i <= endIndexExclusive - size; i++)
                {
                    int errors = 0;
                    int currIndex = i;
                    foreach (var elem in Elements)
                    {
                        if (!elem.Check(rng, ref currIndex))
                        {
                            if (++errors > AllowedErrors)
                                break;
                        }
                    }
                    if (errors <= AllowedErrors)
                        results.Add(i);
                }
            }
            return results;
        }
    }

    public enum ElementKind
    {
        Unknown,
        RandomInRange,
        RandomRangeInRange,
        IRandomInRange,
        IRandomRangeInRange,
        ChooseIndex,
        GreaterThanPrevious,
        LesserThanPrevious,
        LaterPattern,
        Skip
    }

    public interface IElement
    {
        public ElementKind Kind { get; init; }
        public int GetSize();

        public bool Check(uint[] rng, ref int index);
    }

    public sealed class ElementUnknown : IElement
    {
        public ElementKind Kind { get; init; } = ElementKind.Unknown;
        public int GetSize() => (IsIRandom ? 2 : 1);

        public bool IsIRandom { get; init; }

        public ElementUnknown(bool isIRandom)
        {
            IsIRandom = isIRandom;
        }

        public bool Check(uint[] rng, ref int index)
        {
            index += (IsIRandom ? 2 : 1);
            return true;
        }
    }

    public sealed class ElementSkip : IElement
    {
        public ElementKind Kind { get; init; } = ElementKind.Skip;
        public int GetSize() => Amount;

        public int Amount { get; init; }

        public ElementSkip(int amount)
        {
            Amount = amount;
        }

        public bool Check(uint[] rng, ref int index)
        {
            if (index + Amount > rng.Length)
                return false;

            index += Amount;
            return true;
        }
    }

    public sealed class ElementRandomInRange : IElement
    {
        public ElementKind Kind { get; init; } = ElementKind.RandomInRange;
        public int GetSize() => 1;
        public long MinInclusive { get; init; }
        public long MaxExclusive { get; init; }
        public bool Inverted { get; set; } = false;

        public ElementRandomInRange(long minInclusive, long maxExclusive)
        {
            MinInclusive = minInclusive;
            MaxExclusive = maxExclusive;
        }

        public ElementRandomInRange(double range, double minInclusive, double maxExclusive)
        {
            (MinInclusive, MaxExclusive) = RNG.GetRange(range, minInclusive, maxExclusive);
        }

        public bool Check(uint[] rng, ref int index)
        {
            uint curr = rng[index++];
            if (Inverted)
                return curr < MinInclusive || curr >= MaxExclusive;
            return curr >= MinInclusive && curr < MaxExclusive;
        }
    }

    public sealed class ElementIRandomInRange : IElement
    {
        public ElementKind Kind { get; init; } = ElementKind.IRandomInRange;
        public int GetSize() => 2;
        public long Range { get; init; }
        public long MinInclusive { get; init; }
        public long MaxExclusive { get; init; }
        public bool Inverted { get; set; } = false;

        public ElementIRandomInRange(long range, long minInclusive, long maxExclusive)
        {
            Range = range;
            MinInclusive = minInclusive;
            MaxExclusive = maxExclusive;
        }

        public ElementIRandomInRange(long range, long exact)
        {
            Range = range;
            MinInclusive = exact;
            MaxExclusive = exact + 1;
        }

        public bool Check(uint[] rng, ref int index)
        {
            long a = rng[index++];
            if (index >= rng.Length)
                return false;
            long b = rng[index++];
            long curr = ((b << 32) | a) % (Range + 1);
            if (Inverted)
                return curr < MinInclusive || curr >= MaxExclusive;
            return curr >= MinInclusive && curr < MaxExclusive;
        }
    }

    public sealed class ElementRandomRangeInRange : IElement
    {
        public ElementKind Kind { get; init; } = ElementKind.RandomRangeInRange;
        public int GetSize() => 1;
        public long MinInclusive { get; init; }
        public long MaxExclusive { get; init; }
        public bool Inverted { get; set; } = false;

        public ElementRandomRangeInRange(double rangeStart, double rangeEnd, double minInclusive, double maxExclusive)
        {
            if (rangeEnd < rangeStart)
            {
                double temp = rangeEnd;
                rangeEnd = rangeStart;
                rangeStart = temp;
            }
            (MinInclusive, MaxExclusive) = RNG.GetRange(rangeEnd - rangeStart, minInclusive - rangeStart, maxExclusive - rangeStart);
        }

        public bool Check(uint[] rng, ref int index)
        {
            uint curr = rng[index++];
            if (Inverted)
                return curr < MinInclusive || curr >= MaxExclusive;
            return curr >= MinInclusive && curr < MaxExclusive;
        }
    }

    public sealed class ElementIRandomRangeInRange : IElement
    {
        public ElementKind Kind { get; init; } = ElementKind.IRandomInRange;
        public int GetSize() => 2;
        public long Range { get; init; }
        public long MinInclusive { get; init; }
        public long MaxExclusive { get; init; }
        public bool Inverted { get; set; } = false;

        public ElementIRandomRangeInRange(long rangeStart, long rangeEnd, long minInclusive, long maxExclusive)
        {
            if (rangeEnd < rangeStart)
            {
                long temp = rangeEnd;
                rangeEnd = rangeStart;
                rangeStart = temp;
            }
            Range = rangeEnd - rangeStart;
            MinInclusive = minInclusive - rangeStart;
            MaxExclusive = maxExclusive - rangeStart;
        }

        public bool Check(uint[] rng, ref int index)
        {
            long a = rng[index++];
            long b = rng[index++];
            long curr = ((b << 32) | a) % (Range + 1);
            if (Inverted)
                return curr < MinInclusive || curr >= MaxExclusive;
            return curr >= MinInclusive && curr < MaxExclusive;
        }
    }

    public sealed class ElementChooseIndex : IElement
    {
        public ElementKind Kind { get; init; } = ElementKind.ChooseIndex;
        public int GetSize() => 1;
        public int ArgCount { get; init; }
        public int MinInclusive { get; init; }
        public int MaxExclusive { get; init; }
        public bool Inverted { get; set; } = false;

        public ElementChooseIndex(int argCount, int minInclusive, int maxExclusive)
        {
            ArgCount = argCount;
            MinInclusive = minInclusive;
            MaxExclusive = maxExclusive;
        }

        public ElementChooseIndex(int argCount, int exact)
        {
            ArgCount = argCount;
            MinInclusive = exact;
            MaxExclusive = exact + 1;
        }

        public bool Check(uint[] rng, ref int index)
        {
            long curr = rng[index++] % (ArgCount + 1);
            if (Inverted)
                return curr < MinInclusive || curr >= MaxExclusive;
            return curr >= MinInclusive && curr < MaxExclusive;
        }
    }

    // Note: only works for random()
    public sealed class ElementGreaterThanPrevious : IElement
    {
        public ElementKind Kind { get; init; } = ElementKind.GreaterThanPrevious;
        public int GetSize() => 1;

        public bool Check(uint[] rng, ref int index)
        {
            if (index == 0)
                return true; // Can't be greater than nothing, so just assume it's true
            bool isGreater = rng[index] > rng[index - 1];
            index++;
            return isGreater;
        }
    }

    // Note: only works for random()
    public sealed class ElementLesserThanPrevious : IElement
    {
        public ElementKind Kind { get; init; } = ElementKind.LesserThanPrevious;
        public int GetSize() => 1;

        public bool Check(uint[] rng, ref int index)
        {
            if (index == 0)
                return true; // Can't be lesser than nothing, so just assume it's true
            bool isLesser = rng[index] < rng[index - 1];
            index++;
            return isLesser;
        }
    }

    public sealed class ElementLaterPattern : IElement
    {
        public ElementKind Kind { get; init; } = ElementKind.LaterPattern;
        public int GetSize() => _size;

        public Pattern Pattern { get; init; }
        public int SearchRange { get; init; }
        private int _size { get; init; }

        public ElementLaterPattern(Pattern pattern, int searchRange = 5000)
        {
            Pattern = pattern;
            SearchRange = searchRange;
            _size = pattern.GetSize();
        }

        public bool Check(uint[] rng, ref int index)
        {
            return Pattern.CheckFirst(rng, index, Math.Min(index + SearchRange, rng.Length - _size)) != -1;
        }
    }

    public static int Parallelism { get; set; } = Math.Min(16, Environment.ProcessorCount);

    /// <summary>
    /// Attempts to find a seed where a specific boolean/ranged pattern of RNG exists, within a search range.
    /// Will fail if more than one possible seed is found.
    /// </summary>
    /// <param name="pattern">Known RNG pattern that is within the desired range</param>
    /// <param name="offset">Location in RNG sequence to begin the search</param>
    /// <param name="searchSize">Number of RNG values to advance in the sequence before aborting</param>
    /// <param name="resultSeed">The seed that was found, if successful</param>
    /// <param name="resultIndex">The index within the RNG sequence where the pattern starts</param>
    /// <returns>True if successful, false otherwise.</returns>
    public static bool TryFindSeedWithinRange(Pattern pattern, int offset, int searchSize, out uint resultSeed, out int resultIndex, List<(uint, int)>? seedList = null)
    {
        uint localResultSeed = 0;
        int localResultIndex = 0;
        bool foundSeed = false;
        bool foundExtraSeeds = false;

        // Split the work of all the seeds into separate threads
        // Do it in chunks to reduce large memory allocations
        var rangePartitioner = Partitioner.Create(0, RNG.UniqueSeeds?.Length ?? throw new Exception("RNG not initialized"));
        object _lock = new();
        Parallel.ForEach(rangePartitioner, new ParallelOptions { MaxDegreeOfParallelism = Parallelism }, range =>
        {
            uint[] simulated = new uint[searchSize];
            RNG rng = new();
            for (int ind = range.Item1; ind < range.Item2; ind++)
            {
                // Initialize new generator using one of the unique seeds
                uint seed = RNG.UniqueSeeds[ind];
                rng.SetSeed(seed);

                // Advance to the starting location
                for (int i = 0; i < offset; i++)
                    rng.Next();

                // Now simulate the results we want
                for (int i = 0; i < searchSize; i++)
                    simulated[i] = rng.Next();

                // Search the simulated results for our pattern
                int patternIndex = pattern.CheckFirst(simulated, 0, simulated.Length);
                if (patternIndex != -1)
                {
                    lock (_lock)
                    {
                        seedList?.Add((seed, patternIndex));
                        if (!foundSeed)
                        {
                            // This is the first seed found
                            foundSeed = true;
                            localResultSeed = seed;
                            localResultIndex = patternIndex;
                        }
                        else
                        {
                            // Found multiple seeds with the pattern
                            foundExtraSeeds = true;
                        }
                    }
                }
            }
        });

        if (foundSeed && !foundExtraSeeds)
        {
            // Success!
            resultSeed = localResultSeed;
            resultIndex = localResultIndex;
            return true;
        }

        // Either didn't find any seeds, or more than one seed matched
        resultSeed = 0;
        resultIndex = -1;
        return false;
    }

    /// <summary>
    /// Finds values matching a pattern element consecutively a given number of times, within a search range.
    /// Only supports pattern elements with single RNG call requirements (to use less memory).
    /// </summary>
    /// <param name="seed">Seed to use</param>
    /// <param name="offset">Location in RNG sequence to begin the search</param>
    /// <param name="searchSize">Number of RNG values to advance in the sequence before aborting</param>
    /// <param name="patternElement">Pattern element to match for</param>
    /// <param name="amount">Number of consecutive values to find</param>
    /// <param name="stride">Number of RNG values between each relevant value (1 means none)</param>
    /// <param name="surroundingList">List filled with surrounding RNG values when successful, otherwise cleared</param>
    /// <returns>Index, or -1 if unsuccessful.</returns>
    public static int FindConsecutiveSingle(uint seed, int offset, int searchSize, IElement patternElement, int amount, int stride, List<uint[]> surroundingList)
    {
        RNG rng = new(seed);

        // Create structures for tracking surrounding values
        surroundingList.Clear();
        int surroundingPad = Math.Max(amount, 4);
        uint[][] surrounding = new uint[stride][];
        int surroundingCount = amount + (surroundingPad * 2);
        for (int i = 0; i < stride; i++)
            surrounding[i] = new uint[surroundingCount];

        // Advance to the starting location, and track values
        int currStrideInd = 0;
        for (int i = 0; i < offset; i++)
        {
            for (int j = surroundingCount - 1; j > 0; j--)
                surrounding[currStrideInd][j] = surrounding[currStrideInd][j - 1];
            surrounding[currStrideInd][0] = rng.Next();
            currStrideInd = (currStrideInd + 1) % stride;
        }

        int counter = 0;
        uint[] singleRng = new uint[1];
        for (int pos = 0; pos < searchSize; pos += stride)
        {
            // Get next value we're interested in
            uint val = rng.Next();
            for (int j = surroundingCount - 1; j > 0; j--)
                surrounding[0][j] = surrounding[0][j - 1];
            surrounding[0][0] = val;

            // Advance RNG past values we're not interested in
            for (int i = 1; i < stride; i++)
            {
                for (int j = surroundingCount - 1; j > 0; j--)
                    surrounding[i][j] = surrounding[i][j - 1];
                surrounding[i][0] = rng.Next();
            }

            singleRng[0] = val;
            int tempIndex = 0;
            if (patternElement.Check(singleRng, ref tempIndex))
            {
                // Found a new consecutive value. Increment counter, and if high enough,
                // return the position where the values began.
                if (++counter >= amount)
                {
                    for (int i = 0; i < surroundingPad; i++)
                    {
                        // Find the pertinent surrounding values
                        for (int j = surroundingCount - 1; j > 0; j--)
                            surrounding[0][j] = surrounding[0][j - 1];
                        surrounding[0][0] = rng.Next();

                        // Find surrounding values in the other stride offsets
                        for (int k = 1; k < stride; k++)
                        {
                            for (int j = surroundingCount - 1; j > 0; j--)
                                surrounding[k][j] = surrounding[k][j - 1];
                            surrounding[k][0] = rng.Next();
                        }
                    }

                    // Actually add the collected data to the supplied list
                    // (and reverse it!)
                    foreach (uint[] arr in surrounding)
                        Array.Reverse(arr);
                    surroundingList.AddRange(surrounding);

                    return pos - ((amount - 1) * stride);
                }
            }
            else
            {
                // This value isn't within the range, so reset the counter.
                counter = 0;
            }
        }

        // Unsuccessful
        return -1;
    }

    /// <summary>
    /// Attempts to find a pattern of RNG within a seed.
    /// </summary>
    /// <param name="seed">RNG seed to use.</param>
    /// <param name="pattern">Known RNG pattern that is within the desired range</param>
    /// <param name="offset">Location in RNG sequence to begin the search</param>
    /// <param name="searchSize">Number of RNG values to advance in the sequence before aborting</param>
    /// <returns>The first index of the pattern discovered, or -1 if none found.</returns>
    public static List<int> TryFindPattern(uint seed, Pattern pattern, int offset, int searchSize)
    {
        uint[] simulated = new uint[searchSize];
        RNG rng = new(seed);

        // Advance to the starting location
        for (int i = 0; i < offset; i++)
            rng.Next();

        // Now simulate the results we want
        for (int i = 0; i < searchSize; i++)
            simulated[i] = rng.Next();

        // Search the simulated results for our pattern
        return pattern.Check(simulated, offset, searchSize);
    }
}
