using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Test;

namespace TestGui
{
    class TestdRunner
    {
        List<string> lines = new List<string>();

        public string AsmName { get; }
        public EventHandler RunStarted;
        public EventHandler<TestEventArgs> Tested;
        public EventHandler<RunFinishedEventArgs> RunFinished;
        public EventHandler<string> Info;
        public EventHandler<string> Error;

        string previousInfo;
        readonly string[] args;

        public TestdRunner(string[] args)
        {
            this.args = args;
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            this.AsmName = args.First(a => !a.StartsWith("--", StringComparison.Ordinal));
        }

        public void Start()
        {
            var asm = Assembly.GetExecutingAssembly();
            var location = Path.GetDirectoryName(asm.Location);
            var si = new ProcessStartInfo
            {
                FileName = Path.Combine(location, "Testd.exe"),
                Arguments = string.Join(" ", args),
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = Directory.GetCurrentDirectory(),                                
            };
            Process p = Process.Start(si);
            p.Exited += Testd_Exited;
            p.EnableRaisingEvents = true;
            Task.Run(() => ParseAutoputAsync(p.StandardError));
            Task.Run(() => ParseAutoputAsync(p.StandardOutput));
        }

        private void Testd_Exited(object sender, EventArgs e)
        {
            Process p = (Process)sender;
            var code = p.ExitCode;
        }

        void ParseAutoputAsync(TextReader input)
        {
            for (;;)
            {
                var line = input.ReadLine();
                if (line == null)
                    return;
                lock (AsmName)
                {
                    var @class = Classify(line);
                    switch (@class)
                    {
                        case Class.Output:
                            lines.Add(line);
                            break;
                        case Class.Pass:
                        case Class.Fail:
                            var args = Parse(line);
                            args.Output = lines;
                            Tested?.Invoke(this, args);
                            lines = new List<string>();
                            break;
                        case Class.Info:
                            if (line.Contains("Starting new test run of '"))
                            {
                                RunStarted?.Invoke(this, EventArgs.Empty);
                            }
                            if (line.Contains("Finished test run of '"))
                            {
                                var stats = ParseTotals(previousInfo);
                                RunFinished?.Invoke(this, stats);
                            }
                            previousInfo = line;
                            break;
                        case Class.Error:
                            Error?.Invoke(this, line);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private RunFinishedEventArgs ParseTotals(string line)
        {
            var infototals = line.Split(':');
            var totals = infototals.Last();
            var bits = totals.Split(new string[] { ", " }, StringSplitOptions.None);
            int total = int.Parse(bits[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).First());
            int passed = int.Parse(bits[1].Split(' ').First());
            int failed= int.Parse(bits[2].Split(' ').First());
            return new RunFinishedEventArgs { Total = total, Passed = passed, Failed = failed };
        }

        static Class Classify(string line)
        {
            var bits = line.Split(':');
            if (bits.Length < 2)
                return Class.Output;
            if (bits[0] == "PASS")
                return Class.Pass;
            if (bits[0] == "FAIL")
                return Class.Fail;
            if (bits[1] == " INFO")
                return Class.Info;
            if (bits[1] == " ERROR")
                return Class.Error;
            return Class.Output;
        }        

        enum Class
        {
            Output,
            Pass,
            Fail,
            Info,
            Error,
        }

        TestEventArgs Parse(string line)
        {
            var result = new TestEventArgs();
            var bits = line.Split(new string[] { ": " }, StringSplitOptions.None);
            result.TestName = bits[1];
            result.Pass = (bits[0].Equals("PASS", StringComparison.Ordinal));
            return result;
        }

    }

    public class TestEventArgs : EventArgs
    {
        public string TestName { get; set; }
        public bool Pass { get; set; }
        public List<string> Output { get; set; }
        public TimeSpan Elapsed { get; set; }
    }

    public class RunFinishedEventArgs : EventArgs
    {
        public int Total { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
    }
}
