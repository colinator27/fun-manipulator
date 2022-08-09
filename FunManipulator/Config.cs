using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FunManipulator
{
    public class Config
    {
        private static Config? _instance = null;
        public static Config Instance
        {
            get
            {
                if (_instance == null)
                    throw new Exception("Config not loaded");
                return _instance;
            }
        }

        public bool RNG15Bit { get; set; } = true;
        public bool RNGOldPoly { get; set; } = false;

        public FunManipConfig FunManip { get; set; } = new();
        public SeedFinderConfig SeedFinder { get; set; } = new();
        public DogiManipConfig DogiManip { get; set; } = new();

        public string BeepFilename { get; set; } = "beep.wav";
        public double BeepFrameOffset { get; set; } = -4;
        public int BeepMsOffset { get; set; } = 0;
        public int BeepCount { get; set; } = 5;
        public double BeepInterval { get; set; } = 0.3;
        public int BeepEarlyMs { get; set; } = -16;
        public int BeepLateMs { get; set; } = -2;

        public float WindowScale { get; set; } = 2f;
        public bool WindowTransparent { get; set; } = false;

        public string Program { get; set; } = "";

        public class FunManipConfig
        {
            public int ForceFunMinimum { get; set; } = -1;
            public int ForceFunMaximum { get; set; } = -1;

            public int FunSearchRange { get; set; } = 5400;
            public int FunSearchOffset { get; set; } = 0;
        }

        public class SeedFinderConfig
        {
            public long Seed { get; set; } = -1;
            public int SearchRange { get; set; } = 5000000;
            public int SearchRangeFind { get; set; } = 500000;
        }

        public class DogiManipConfig
        {
            public Keyboard.Key ChooseHoveredKey { get; set; } = Keyboard.Key.A;
            public Keyboard.Key ScreenshotKey { get; set; } = Keyboard.Key.PageUp;
            public Keyboard.Key[] PreviewSelectKeys { get; set; } = { Keyboard.Key.Num1, Keyboard.Key.Num2, Keyboard.Key.Num3, Keyboard.Key.Num4, Keyboard.Key.Num5, Keyboard.Key.Num6 };
            public Keyboard.Key MoveToGameKey { get; set; } = Keyboard.Key.F1;
            public Keyboard.Key ToggleMinimizedKey { get; set; } = Keyboard.Key.F2;
            public string? InstructionFilename { get; set; } = "instructions.txt";
            public float ScoreMaxDistance { get; set; } = 4f;
            public float ScoreNumRightBias { get; set; } = 0.25f;
        }

        public static JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters = { new JsonStringEnumConverter() },
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static void Load(string filename)
        {
            _instance = JsonSerializer.Deserialize<Config>(File.ReadAllBytes(filename), JsonOptions);
        }
    }
}
