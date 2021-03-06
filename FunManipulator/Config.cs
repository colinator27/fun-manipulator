using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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

        public string BeepFilename { get; set; } = "beep.wav";
        public double BeepFrameOffset { get; set; } = -4;
        public int BeepMsOffset { get; set; } = 0;
        public int BeepCount { get; set; } = 5;
        public double BeepInterval { get; set; } = 0.3;
        public int BeepEarlyMs { get; set; } = -16;
        public int BeepLateMs { get; set; } = -2;

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

        public static void Load(string filename)
        {
            _instance = JsonSerializer.Deserialize<Config>(File.ReadAllBytes(filename));
        }
    }
}
