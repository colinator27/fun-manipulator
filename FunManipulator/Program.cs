using FunManipulator;

public class Program
{
    public static bool AutoProgressEnding = false;
    public static string? NextProgramToRun = null;

    public static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Loading config...");
            Config.Load(PlatformSpecific.GetConfigFileName());

            Console.WriteLine("Initializing RNG...");
            RNG.Initialize(Config.Instance.RNG15Bit, Config.Instance.RNGOldPoly);

            Dictionary<string, ProgramExecutor> programs = new()
            {
                { "funmanip", FunManip.Run },
                { "seedfinder", SeedFinder.Run },
                { "dogimanip", DogiManip.Run },
                { "screenshot", ScreenshotTool.Run },
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

            do
            {
                programs[program]();

                if (NextProgramToRun != null)
                {
                    Console.WriteLine($"Completed. Now starting program {NextProgramToRun}...");
                    program = NextProgramToRun.ToLowerInvariant();
                    NextProgramToRun = null;

                    if (!programs.ContainsKey(program))
                    {
                        Console.WriteLine("... actually, nevermind! The program doesn't exist!");
                        break;
                    }
                }
                else
                {
                    if (AutoProgressEnding)
                    {
                        Console.WriteLine("Completed. Exiting...");
                    }
                    else
                    {
                        Console.WriteLine("Completed. Press any key to exit.");
                        Console.ReadKey();
                    }
                    break;
                }
            }
            while (program != null);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception: {e}");
        }
    }

    public delegate void ProgramExecutor();
}