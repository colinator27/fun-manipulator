﻿using System.Runtime.CompilerServices;

namespace FunManipulator;

public sealed class RNG
{
    public static bool Seeds15Bit { get; private set; }
    public static bool SeedsSigned { get; private set; }
    public static uint[]? UniqueSeeds { get; private set; }
    public static uint RandomPoly { get; private set; }

    public static void Initialize(bool is15bit = true, bool useOldPoly = false, bool signed = false)
    {
        Seeds15Bit = is15bit;
        SeedsSigned = signed;

        // Build unique seed array
        HashSet<uint> uniqueStates = new();
        if (Seeds15Bit)
        {
            UniqueSeeds = new uint[32768];
            uint i = 0;
            int j = 0;
            while (uniqueStates.Count < 32768)
            {
                ushort state = (ushort)((((i * 0x343fd) + 0x269ec3) >> 16) & 0x7fff);
                if (uniqueStates.Add(state))
                    UniqueSeeds[j++] = i;
                i++;
            }
        }
        else if (SeedsSigned)
        {
            UniqueSeeds = new uint[65536];
            uint i = 0;
            int j = 0;
            while (uniqueStates.Count < 65536)
            {
                uint state = (uint)(((((int)i * 0x343fd) + 0x269ec3) >> 16) & 0x7fffffff);
                if (uniqueStates.Add(state))
                    UniqueSeeds[j++] = i;
                i++;
            }
        }
        else
        {
            UniqueSeeds = new uint[65536];
            uint i = 0;
            int j = 0;
            while (uniqueStates.Count < 65536)
            {
                ushort state = (ushort)(((i * 0x343fd) + 0x269ec3) >> 16);
                if (uniqueStates.Add(state))
                    UniqueSeeds[j++] = i;
                i++;
            }
        }

        if (useOldPoly)
            RandomPoly = 0xda442d20u;
        else
            RandomPoly = 0xda442d24u;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (long, long) GetRange(double range, double minInclusive, double maxExclusive)
    {
        if (range != 1)
        {
            minInclusive /= range;
            maxExclusive /= range;
        }
        return ((long)(minInclusive * ((long)uint.MaxValue + 1)), (long)(maxExclusive * ((long)uint.MaxValue + 1)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ValueToDouble(uint val, double range)
    {
        return val * 2.328306436538696e-10 * range;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ValueToSingle(uint val, float range)
    {
        return val * 2.328306436538696e-10f * range;
    }

    public uint[] State { get; set; }
    private int Index { get; set; }

    public RNG()
    {
        State = new uint[16];
        Index = 0;
    }

    public RNG(uint seed)
    {
        State = new uint[16];
        SetSeed(seed);
    }

    public void SetSeed(uint seed)
    {
        // Initialize random state
        Index = 0;
        if (Seeds15Bit)
        {
            for (int i = 0; i < 16; i++)
            {
                seed = (((seed * 0x343fd) + 0x269ec3) >> 16) & 0x7fff;
                State[i] = seed;
            }
        }
        else if (SeedsSigned)
        {
            int signedSeed = (int)seed;
            for (int i = 0; i < 16; i++)
            {
                signedSeed = (((signedSeed * 0x343fd) + 0x269ec3) >> 16) & 0x7fffffff;
                State[i] = (uint)signedSeed;
            }
        }
        else
        {
            seed = (uint)(((((int)seed * 0x343fd) + 0x269ec3) >> 16) & 0x7fffffff);
            State[0] = seed;
            for (int i = 1; i < 16; i++)
            {
                seed = ((seed * 0x343fd) + 0x269ec3) >> 16;
                State[i] = seed;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Next()
    {
        // Advance random state as per WELL512a
        uint a = State[Index];
        uint b = State[(Index + 13) & 15];
        uint c = a ^ b ^ (a << 16) ^ (b << 15);
        b = State[(Index + 9) & 15];
        b ^= (b >> 11);
        a = State[Index] = c ^ b;
        uint d = a ^ ((a << 5) & RandomPoly);
        Index = (Index + 15) & 15;
        a = State[Index];
        State[Index] = a ^ c ^ d ^ (a << 2) ^ (c << 18) ^ (b << 28);
        return State[Index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double NextDouble(double range)
    {
        return Next() * 2.3283064365386963e-10 * range;
    }
}
