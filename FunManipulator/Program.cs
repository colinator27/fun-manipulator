using FunManipulator;

try
{
    Console.WriteLine("Loading config...");
    Config.Load("config.json");

    Console.WriteLine("Initializing RNG...");
    RNG.Initialize(Config.Instance.RNG15Bit, Config.Instance.RNGOldPoly);

    Dictionary<string, ProgramExecutor> programs = new()
    {
        { "funmanip", FunManip.Run },
        { "seedfinder", SeedFinder.Run },
        { "dogimanip", DogiManip.Run },
        { "testing", Testing.Run }
    };
    string? program = Config.Instance.Program?.ToLowerInvariant();
    while (string.IsNullOrEmpty(program))
    {
        Console.WriteLine("Valid programs: ");
        foreach (string name in programs.Keys)
            Console.WriteLine($" - {name}");
        Console.Write("Enter program name: ");
        program = Console.ReadLine()?.ToLowerInvariant();
        if (program != null && !programs.ContainsKey(program))
            program = null;
    }
    programs[program]();

    Console.WriteLine("Completed. Press any key to exit.");
    Console.ReadKey();
}
catch (Exception e)
{
    Console.WriteLine($"Exception: {e}");
}

public delegate void ProgramExecutor();