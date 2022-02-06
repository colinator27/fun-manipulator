namespace FunManipulator;

public static class Testing
{
    public static void Run()
    {
        Search.Pattern pattern = new();
        pattern.AllowedErrors = 0;
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious()); // 2
        pattern.Elements.Add(new Search.ElementLesserThanPrevious()); // 3
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious()); // 4
        pattern.Elements.Add(new Search.ElementLesserThanPrevious()); // 5
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious()); // 6
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious()); // 7
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious()); // 8
        pattern.Elements.Add(new Search.ElementLesserThanPrevious()); // 9
        pattern.Elements.Add(new Search.ElementLesserThanPrevious()); // 10
        pattern.Elements.Add(new Search.ElementLesserThanPrevious()); // 11
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious()); // 12
        pattern.Elements.Add(new Search.ElementLesserThanPrevious()); // 13
        pattern.Elements.Add(new Search.ElementUnknown(false)); // 14
        pattern.Elements.Add(new Search.ElementUnknown(false));
        pattern.Elements.Add(new Search.ElementUnknown(false));
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious());
        pattern.Elements.Add(new Search.ElementLesserThanPrevious());
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious());
        pattern.Elements.Add(new Search.ElementLesserThanPrevious());
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious());
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious());
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious());
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious()); // 24
        pattern.Elements.Add(new Search.ElementLesserThanPrevious());
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious());
        pattern.Elements.Add(new Search.ElementLesserThanPrevious()); // 27
        pattern.Elements.Add(new Search.ElementLesserThanPrevious());
        pattern.Elements.Add(new Search.ElementLesserThanPrevious());
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious());
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious());
        pattern.Elements.Add(new Search.ElementLesserThanPrevious());
        pattern.Elements.Add(new Search.ElementLesserThanPrevious()); // 33
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious());
        pattern.Elements.Add(new Search.ElementLesserThanPrevious()); // 35
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious());
        pattern.Elements.Add(new Search.ElementLesserThanPrevious()); // 37
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious());
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious());
        /*pattern.Elements.Add(new Search.ElementLesserThanPrevious()); // 40
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious());
        pattern.Elements.Add(new Search.ElementLesserThanPrevious());
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious()); // 43
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious());
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious());
        pattern.Elements.Add(new Search.ElementLesserThanPrevious());
        pattern.Elements.Add(new Search.ElementLesserThanPrevious());
        pattern.Elements.Add(new Search.ElementGreaterThanPrevious());*/
        uint seed;
        int ind;
        if (Search.TryFindSeedWithinRange(pattern, 0, 100000, out seed, out ind))
        {
            Console.WriteLine($"Found seed {seed} at {ind}");
        }
        else
            Console.WriteLine("Didn't find seed");
    }
}