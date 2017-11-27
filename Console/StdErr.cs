using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Tests
{
    static class StdErr
    {
        static readonly string ExeName;

        static StdErr()
        {
            ExeName = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs().First());
        }

        public static void Info(string line)
        {
            var before = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Error.WriteLine($"{ExeName}: INFO: {line}");
            Console.ForegroundColor = before;
        }

        public static void Error(string line)
        {
            var before = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"{ExeName}: ERROR: {line}");
            Console.ForegroundColor = before;
            if (Debugger.IsAttached)
                Debugger.Break();
        }
    }
}
