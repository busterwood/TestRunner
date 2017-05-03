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
        public event EventHandler RunStarted;
        public event EventHandler<TestEventArgs> Tested;
        public event EventHandler<RunFinishedEventArgs> RunFinished;
        public event EventHandler<string> Info;
        public event EventHandler<string> Error;

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
                        case LineClass.Output:
                            lines.Add(line);
                            break;
                        case LineClass.Pass:
                        case LineClass.Ignored:
                        case LineClass.Fail:
                            var args = Parse(line, @class);
                            args.Output = lines;
                            Tested?.Invoke(this, args);
                            lines = new List<string>();
                            break;
                        case LineClass.Info:
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
                        case LineClass.Error:
                            Error?.Invoke(this, line);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        static LineClass Classify(string line)
        {
            var bits = line.Split(':');
            if (bits.Length < 2)
                return LineClass.Output;
            if (bits[0] == "PASS")
                return LineClass.Pass;
            if (bits[0] == "FAIL")
                return LineClass.Fail;
            if (bits[0] == "IGNORED")
                return LineClass.Ignored;
            if (bits[1] == " INFO")
                return LineClass.Info;
            if (bits[1] == " ERROR")
                return LineClass.Error;
            return LineClass.Output;
        }        

        enum LineClass
        {
            Pass = 1,
            Fail = 2,
            Ignored = 3,
            Output,
            Info,
            Error,
        }

        TestEventArgs Parse(string line, LineClass @class)
        {
            var result = new TestEventArgs();
            var bits = line.Split(new string[] { ": " }, StringSplitOptions.None);
            result.Result = (TestResult)@class;
            if (@class == LineClass.Pass || @class ==  LineClass.Fail)
            {
                bits = bits[1].Split(new string[] { " in " }, StringSplitOptions.None); // TestFixture.TestName in 10 MS
                result.TestName = bits[0];
                result.Elapsed = TimeSpan.FromMilliseconds(int.Parse(bits[1].Split(' ')[0]));
            }
            else
            {
                result.TestName = bits[1];
            }

            return result;
        }


        private RunFinishedEventArgs ParseTotals(string line)
        {
            var infototals = line.Split(':');
            var totals = infototals.Last();
            var bits = totals.Split(new string[] { ", " }, StringSplitOptions.None);
            int total = int.Parse(bits[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).First());
            int passed = int.Parse(bits[1].Split(' ').First());
            int failed = int.Parse(bits[2].Split(' ').First());
            int ignored = int.Parse(bits[3].Split(' ').First());
            return new RunFinishedEventArgs { Total = total, Passed = passed, Failed = failed, Ignored = ignored };
        }
    }

    public class TestEventArgs : EventArgs
    {
        public string TestName { get; set; }
        public TestResult Result { get; set; }
        public List<string> Output { get; set; }
        public TimeSpan? Elapsed { get; set; }
    }

    public enum TestResult
    {
        Pass = 1,
        Fail = 2,
        Ignored = 3,
    }

    public class RunFinishedEventArgs : EventArgs
    {
        public int Total { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int Ignored { get; set; }
    }
}
