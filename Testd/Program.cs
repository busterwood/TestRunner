using BusterWood.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Test.Daemon
{
    class Program
    {
        static UniqueList<string> testArgs;
        static string asmName;
        static string testExe;
        static Process testProcess;

        static int Main(string[] argv)
        {
            var args = argv.ToUniqueList(StringComparer.OrdinalIgnoreCase);

            testExe = TestExeFromArgs(args);

            if (args.Count == 0)
            {
                StdErr.Error("Required argumenment is missing: assembly to test");
                return 1;
            }
            testArgs = args;
            asmName = args.First(a => !a.StartsWith("-", StringComparison.Ordinal));

            var monitor = new FolderMonitor(Directory.GetCurrentDirectory());
            monitor.Changed += Monitor_Changed;
            monitor.Start();

            for (;;)
            {
                var line = Console.ReadLine()?.ToLower();
                if (string.IsNullOrEmpty(line))
                    break;
                else if (line == "run")
                    monitor.RunNow();
                else if (line == "debug")
                    monitor.DebugNow();
            }
            testProcess?.Kill();
            return 0;
        }

        static string TestExeFromArgs(IList<string> args)
        {
            if (args.Remove("--x64"))
                return "TestX64.exe";
            else if (args.Remove("--x86"))
                return "TestX86.exe";
            else
                return "Test.exe";
        }

        private static void Monitor_Changed(object sender, ChangedEventArgs e)
        {
            StdErr.Info($"Starting new test run of '{asmName}'....");
            var asm = Assembly.GetExecutingAssembly();
            var location = Path.GetDirectoryName(asm.Location);

            // add --debug if needed
            var args = testArgs.ToUniqueList(StringComparer.OrdinalIgnoreCase);
            if (e.Debug)
                args.Add("--debug");

            var si = new ProcessStartInfo
            {
                FileName = Path.Combine(location, testExe),
                Arguments = string.Join(" ", args),
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = Directory.GetCurrentDirectory(),
            };
            testProcess = Process.Start(si);
            var tasks = new Task[] {
                Task.Run(() => EchoAsync(testProcess.StandardError, Console.Error)),
                Task.Run(() => EchoAsync(testProcess.StandardOutput, Console.Out)),
            };
            Task.WaitAll(tasks);
            testProcess.WaitForExit(); //TODO: timeout and terminate
            testProcess = null;
            StdErr.Info($"Finished test run of '{asmName}'" + Environment.NewLine);
        }

        static void EchoAsync(TextReader input, TextWriter output)
        {
            for(;;)
            {
                var line = input.ReadLine();
                if (line == null)
                    return;
                lock (asmName)
                {
                    var old = Console.ForegroundColor;
                    Console.ForegroundColor = GetColour(line);
                    output.WriteLine(line);
                    Console.ForegroundColor = old;
                }
            }
        }

        static ConsoleColor GetColour(string line)
        {
            var bits = line.Split(':');
            if (bits.Length < 2)
                return Console.ForegroundColor;
            if (bits[0] == "PASS")
                return ConsoleColor.Green;
            if (bits[0] == "FAIL")
                return ConsoleColor.Red;
            if (bits[1] == " INFO")
                return ConsoleColor.Cyan;
            if (bits[1] == " ERROR")
                return ConsoleColor.Red;
            if (bits[0] == "START")
                return ConsoleColor.DarkCyan;
            if (bits[0] == "-> done") // specflow pattern
                return ConsoleColor.DarkGreen;
            return Console.ForegroundColor;
        }
    }
}
