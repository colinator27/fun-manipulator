using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunManipulator
{
    public static class ConsoleHelpers
    {
        public static uint ReadLineUInt(string msg)
        {
            uint res;
            string? text;
            do
            {
                Console.Write(msg);
                text = Console.ReadLine();
            }
            while (text == null || !uint.TryParse(text, out res));
            return res;
        }

        public static double ReadLineDouble(string msg)
        {
            double res;
            string? text;
            do
            {
                Console.Write(msg);
                text = Console.ReadLine();
            }
            while (text == null || !double.TryParse(text, out res));
            return res;
        }

        public static double ReadLineDoubleMin(string msg, double min)
        {
            double res;
            string? text;
            do
            {
                Console.Write(msg);
                text = Console.ReadLine();
            }
            while (text == null || !double.TryParse(text, out res) || res < min);
            return res;
        }

        public static double ReadLineDoubleRange(string msg, double min, double max)
        {
            double res;
            string? text;
            do
            {
                Console.Write(msg);
                text = Console.ReadLine();
            }
            while (text == null || !double.TryParse(text, out res) || res < min || res > max);
            return res;
        }

        public static bool ReadYesNo(string msg)
        {
            Console.Write($"{msg} (y/N): ");
            if (Console.ReadLine()?.Trim().ToLowerInvariant() == "y")
                return true;
            return false;
        }
    }
}
