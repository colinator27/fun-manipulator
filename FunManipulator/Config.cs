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
        public static Config Instance { get; private set; }

        public bool RNG15Bit { get; set; } = true;
        public bool RNGOldPoly { get; set; } = false;

        public int ForceFunMinimum { get; set; } = -1;
        public int ForceFunMaximum { get; set; } = -1;

        public int FunSearchRange { get; set; } = 5400;
        public int FunSearchOffset { get; set; } = 0;

        public string BeepFilename { get; set; } = "beep.wav";
        public double BeepFrameOffset { get; set; } = -4;
        public int BeepCount { get; set; } = 5;
        public double BeepInterval { get; set; } = 0.3;

        public static void Load(string filename)
        {
            Instance = JsonSerializer.Deserialize<Config>(File.ReadAllBytes(filename));
        }
    }
}
