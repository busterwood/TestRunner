using System;
using System.IO;
using System.Linq;

namespace Test
{
    static class StdOut
    {
        static readonly string ExeName;

        static StdOut()
        {
            ExeName = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs().First());
        }

        public static void Passed(string line)
        {
            var before = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Error.WriteLine($"PASS: {line}");
            Console.ForegroundColor = before;
        }

        public static void Fail(string line)
        {
            var before = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"FAIL: {line}");
            Console.ForegroundColor = before;
        }
    }
}
