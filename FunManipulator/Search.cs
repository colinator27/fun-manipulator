using System.Collections.Concurrent;

namespace FunManipulator;

public static class Search
{
    public static int Parallelism { get; set; } = Math.Min(16, Environment.ProcessorCount);

    /// <summary>
    /// Attempts to find a seed where a specific boolean/ranged pattern of RNG exists, within a search range.
    /// Will fail if more than one possible seed is found.
    /// </summary>
    /// <param name="pattern">Array representing known RNG calls that are within the desired range (true for within)</param>
    /// <param name="offset">Location in RNG sequence to begin the search</param>
    /// <param name="searchSize">Number of RNG values to advance in the sequence before aborting</param>
    /// <param name="minInclusive">Lowest value that is within desired range</param>
    /// <param name="maxExclusive">Above highest value that is within desired range</param>
    /// <param name="resultSeed">The seed that was found, if successful</param>
    /// <param name="resultIndex">The index within the RNG sequence where the pattern starts</param>
    /// <returns>True if successful, false otherwise.</returns>
    public static bool TryFindSeedWithinRange(bool[] pattern, int offset, int searchSize, long minInclusive, long maxExclusive, out uint resultSeed, out int resultIndex)
    {
        uint localResultSeed = 0;
        int localResultIndex = 0;
        bool foundSeed = false;
        bool foundExtraSeeds = false;

        // Split the work of all the seeds into separate threads
        // Do it in chunks to reduce large memory allocations
        var rangePartitioner = Partitioner.Create(0, RNG.UniqueSeeds.Length);
        object _lock = new();
        Parallel.ForEach(rangePartitioner, new ParallelOptions { MaxDegreeOfParallelism = Parallelism }, range =>
        {
            bool[] simulated = new bool[searchSize];
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
                {
                    uint val = rng.Next();
                    simulated[i] = (val >= minInclusive && val < maxExclusive);
                }

                // Search the simulated results for our pattern
                int patternPos = 0;
                for (int i = 0; i < searchSize; i++)
                {
                    if (pattern[patternPos] == simulated[i])
                    {
                        patternPos++;
                        if (patternPos >= pattern.Length)
                        {
                            lock (_lock)
                            {
                                if (!foundSeed)
                                {
                                    // This is the first seed found
                                    foundSeed = true;
                                    localResultSeed = seed;
                                    localResultIndex = (i + 1) - patternPos;
                                }
                                else
                                {
                                    // Found multiple seeds with the pattern
                                    foundExtraSeeds = true;
                                }
                            }
                            break;
                        }
                    }
                    else
                    {
                        // Pattern order not matched, reset to after the first element to continue scanning
                        i -= patternPos;
                        patternPos = 0;
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
    /// Finds values within a range consecutively a given number of times, within a search range.
    /// </summary>
    /// <param name="seed">Seed to use</param>
    /// <param name="offset">Location in RNG sequence to begin the search</param>
    /// <param name="searchSize">Number of RNG values to advance in the sequence before aborting</param>
    /// <param name="minInclusive">Lowest value that is within desired range</param>
    /// <param name="maxExclusive">Above highest value that is within desired range</param>
    /// <param name="amount">Number of consecutive values to find</param>
    /// <param name="stride">Number of RNG values between each relevant value (1 means none)</param>
    /// <param name="surroundingList">List filled with surrounding RNG values when successful, otherwise cleared</param>
    /// <returns>Index, or -1 if unsuccessful.</returns>
    public static int FindConsecutive(uint seed, int offset, int searchSize, long minInclusive, long maxExclusive, int amount, int stride, List<uint[]> surroundingList)
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

            if (val >= minInclusive && val < maxExclusive)
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
    /// Attempts to find a boolean/ranged pattern of RNG within a seed.
    /// Bytes 0 are not within range, 1 are within range, 2 are unknown.
    /// </summary>
    /// <param name="seed">RNG seed to use.</param>
    /// <param name="pattern">Array representing known RNG calls that are within the desired range (true for within)</param>
    /// <param name="offset">Location in RNG sequence to begin the search</param>
    /// <param name="searchSize">Number of RNG values to advance in the sequence before aborting</param>
    /// <param name="minInclusive">Lowest value that is within desired range</param>
    /// <param name="maxExclusive">Above highest value that is within desired range</param>
    /// <param name="resultIndex">The index within the RNG sequence where the pattern starts</param>
    /// <returns>True if successful, false otherwise.</returns>
    public static bool TryFindPattern(uint seed, byte[] pattern, int offset, int searchSize, long minInclusive, long maxExclusive, out int resultIndex)
    {
        bool[] simulated = new bool[searchSize];
        RNG rng = new(seed);

        // Advance to the starting location
        for (int i = 0; i < offset; i++)
            rng.Next();

        // Now simulate the results we want
        for (int i = 0; i < searchSize; i++)
        {
            uint val = rng.Next();
            simulated[i] = (val >= minInclusive && val < maxExclusive);
        }

        // Search the simulated results for our pattern
        int patternPos = 0;
        for (int i = 0; i < searchSize; i++)
        {
            if (pattern[patternPos] == 2 || pattern[patternPos] == (simulated[i] ? 1 : 0))
            {
                patternPos++;
                if (patternPos >= pattern.Length)
                {
                    resultIndex = (i + 1) - patternPos;
                    return true;
                }
            }
            else
            {
                // Pattern order not matched, reset to after the first element to continue scanning
                i -= patternPos;
                patternPos = 0;
            }
        }

        resultIndex = -1;
        return false;
    }
}
