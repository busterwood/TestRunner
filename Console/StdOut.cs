using System;
using System.IO;
using System.Linq;

namespace Tests
{
    static class StdOut
    {
        static readonly string ExeName;

        static StdOut()
        {
            ExeName = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs().First());
        }

        internal static void Start(string line)
        {
            Console.Out.WriteLine($"START: {line}");
        }

        public static void Passed(string line)
        {
            var before = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Out.WriteLine($"PASS: {line}");
            Console.ForegroundColor = before;
        }

        public static void Fail(string line)
        {
            var before = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Out.WriteLine($"FAIL: {line}");
            Console.ForegroundColor = before;
        }

        public static void Ignore(string line)
        {
            var before = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Out.WriteLine($"IGNORED: {line}");
            Console.ForegroundColor = before;
        }

    }
}
