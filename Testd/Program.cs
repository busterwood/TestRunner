using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Test.Daemon
{
    class Program
    {
        static string asmName;
        static bool x64;

        static int Main(string[] argv)
        {
            var args = argv.ToList();

            if (string.Equals(args[0], "--x64", StringComparison.OrdinalIgnoreCase))
            {
                x64 = true;
                args.RemoveAt(0);
            }

            if (args.Count == 0)
            {
                StdErr.Error("First argument must be the assembly to test");
                return 1;
            }
            asmName = args.First();

            var monitor = new FolderMonitor(Directory.GetCurrentDirectory());
            monitor.Changed += Monitor_Changed;
            monitor.Start();
            Console.ReadLine();
            return 0;
        }

        private static void Monitor_Changed(object sender, EventArgs e)
        {
            StdErr.Info($"Starting new test run of '{asmName}'....");
            var asm = Assembly.GetExecutingAssembly();
            var location = Path.GetDirectoryName(asm.Location);
            var si = new ProcessStartInfo
            {
                FileName = Path.Combine(location, $"Test{(x64 ? "X64" : "")}.exe"),
                Arguments = asmName,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = Directory.GetCurrentDirectory(),
            };
            Process p = Process.Start(si);
            var tasks = new Task[] {
                Task.Run(() => EchoAsync(p.StandardError, Console.Error)),
                Task.Run(() => EchoAsync(p.StandardOutput, Console.Out)),
            };
            Task.WaitAll(tasks);
            p.WaitForExit(); //TODO: timeout and terminate
            StdErr.Info($"Finished test run of '{asmName}'");
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
            return Console.ForegroundColor;
        }
    }
}
