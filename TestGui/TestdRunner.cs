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
        public event EventHandler<RunStartedEventArgs> RunStarted;
        public event EventHandler<TestEventArgs> TestStarted;
        public event EventHandler<TestEventArgs> Tested;
        public event EventHandler<RunFinishedEventArgs> RunFinished;
        public event EventHandler<string> Info;
        public event EventHandler<string> Error;

        readonly string[] args;
        Process testdProcess;
        string previousInfo;

        public TestdRunner(string folder, string[] args)
        {
            this.Folder = folder;
            this.args = args;
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            this.AsmName = args.First(a => !a.StartsWith("--", StringComparison.Ordinal));
        }

        public string Folder { get; }

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
                WorkingDirectory = Folder,                                
            };
            testdProcess = Process.Start(si);
            testdProcess.Exited += Testd_Exited;
            testdProcess.EnableRaisingEvents = true;
            Task.Run(() => ParseAutoputAsync(testdProcess.StandardError));
            Task.Run(() => ParseAutoputAsync(testdProcess.StandardOutput));
        }

        private void Testd_Exited(object sender, EventArgs e)
        {
            Process p = (Process)sender;
            var code = p.ExitCode;
        }

        void ParseAutoputAsync(TextReader input)
        {
            string testCategory = null;
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
                        case LineClass.Start:
                            {
                                var evtArgs = Parse(line, @class);
                                evtArgs.Output = new List<string>();
                                TestStarted?.Invoke(this, evtArgs);
                                lines = new List<string>();
                                testCategory = evtArgs.Category;
                                break;
                            }
                        case LineClass.Output:
                            lines.Add(line);
                            break;
                        case LineClass.Pass:
                        case LineClass.Ignored:
                        case LineClass.Fail:
                            {
                                var evtArgs = Parse(line, @class);                                
                                evtArgs.Output = lines;
                                evtArgs.Category = testCategory;
                                Tested?.Invoke(this, evtArgs);
                                break;
                            }
                        case LineClass.Info:
                            if (line.Contains("Starting new test run of '"))
                            {
                                line = input.ReadLine();
                                if (line == null)
                                    return;
                                int tests = ParseNumberOfTests(line);
                                bool debug = line.Split(',').Any(x => x.Trim() == "debug");
                                RunStarted?.Invoke(this, new RunStartedEventArgs { Total = tests, Debug=debug });
                            }
                            else if (line.Contains("Finished test run of '"))
                            {
                                var stats = ParseTotals(previousInfo);
                                RunFinished?.Invoke(this, stats);
                            }
                            else if (!line.StartsWith("Testd: INFO: Monitoring '", StringComparison.Ordinal))
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

        private int ParseNumberOfTests(string line)
        {
            var testAndFixtures = line.Split(new string[] { ", " }, StringSplitOptions.None);
            var bits = testAndFixtures[0].Split(' ');
            int count;
            int.TryParse(bits[2], out count);
            return count;
        }

        static LineClass Classify(string line)
        {
            var bits = line.Split(':');
            if (bits.Length < 2)
                return LineClass.Output;
            if (bits[0] == "START")
                return LineClass.Start;
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
            Start = 0,
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
            result.Result = (TestResult)@class;
            var bits = line.Split(new string[] { ": " }, StringSplitOptions.None);
            if (@class == LineClass.Pass || @class == LineClass.Fail)
            {
                bits = bits[1].Split(new string[] { " in " }, StringSplitOptions.None); // TestFixture.TestName in 10 MS
                result.FullTestName = bits[0];
                result.Elapsed = TimeSpan.FromMilliseconds(int.Parse(bits[1].Split(' ')[0]));
            }
            else 
            {
                result.FullTestName = bits[1];
                if (bits.Length > 2)
                    result.Category = bits[2];
            }
            return result;
        }

        private RunFinishedEventArgs ParseTotals(string line)
        {
            if (line == null)
                return new RunFinishedEventArgs();
            var infototals = line.Split(':');
            var totals = infototals.Last();
            var bits = totals.Split(new string[] { ", " }, StringSplitOptions.None);
            int total = int.Parse(bits[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).First());
            int passed = int.Parse(bits[1].Split(' ').First());
            int failed = int.Parse(bits[2].Split(' ').First());
            int ignored = int.Parse(bits[3].Split(' ').First());
            return new RunFinishedEventArgs { Total = total, Passed = passed, Failed = failed, Ignored = ignored };
        }

        public void RunTests()
        {
            testdProcess.StandardInput.WriteLine("run");
        }

        public void DebugTests()
        {
            testdProcess.StandardInput.WriteLine("debug");
        }

        internal void Stop()
        {
            testdProcess.StandardInput.WriteLine();
        }
    }

    public class TestEventArgs : EventArgs
    {
        public string TestFixure
        {
            get
            {
                var firstDot = FullTestName.IndexOf('.');
                return FullTestName.Substring(0, firstDot);
            }
        }

        public string TestName
        {
            get
            {
                var firstDot = FullTestName.IndexOf('.');
                return FullTestName.Substring(firstDot+1);
            }
        }
        public string FullTestName { get; set; }
        public string Category { get; set; }
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

    public class RunStartedEventArgs : EventArgs
    {
        public int Total { get; set; }
        public bool Debug { get; set; }
    }

    public class RunFinishedEventArgs : EventArgs
    {
        public int Total { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int Ignored { get; set; }
    }
}
