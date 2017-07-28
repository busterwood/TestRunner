using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TestGui
{
    class ProjectFinder
    {
        public async Task Find(string folder)
        {
            await Task.Yield();
            if (Path.GetDirectoryName(folder).Equals(".git", StringComparison.Ordinal))
                return;

            var projectFiles = Directory.GetFiles(folder, "*test*.csproj");
            if (projectFiles.Any())
            {
                await Task.WhenAll(projectFiles.Select(pf => FindBuilds(pf)).ToArray());
            }
            else
            {
                var subDirs = Directory.EnumerateDirectories(folder);
                await Task.WhenAll(subDirs.Select(Find).ToArray());
            }
        }

        private async Task FindBuilds(object testProjectFile)
        {
            await Task.Yield();
            var pf = LoadProjectFile((string)testProjectFile);
            var args = new FoundProjectEventArgs
            {
                Folder = Path.GetDirectoryName((string)testProjectFile),
                Project = pf.AssemblyName,
            };
            args.Builds = pf.OutputPaths
                .Select(outputPath => Path.Combine(args.Folder, outputPath, pf.AssemblyName + ".dll"))
                .Where(dll => File.Exists(dll))
                .Select(dll => new Build(Path.GetDirectoryName(dll), Path.GetFileName(dll)))
                .ToList();
            if (args.Builds.Count > 0)
                ProjectFound?.Invoke(this, args);
        }

        private ProjectFile LoadProjectFile(string testProjectFile)
        {
            var lines = File.ReadAllLines(testProjectFile);
            if (lines.Length == 0)
                return new ProjectFile(Path.GetFileNameWithoutExtension(testProjectFile), new List<string>());

            if (lines[0].StartsWith(@"<Project Sdk=""Microsoft.NET.Sdk""", StringComparison.Ordinal))
                return Read2017Projectfile(testProjectFile, lines);
            else
                return ReadOlderProjectfile(lines);

        }

        private ProjectFile ReadOlderProjectfile(string[] lines)
        {
            string asmName = null;
            var outputs = new List<string>();

            foreach (var line in lines)
            {
                // stop at EOF or first item group
                if (IsItemGroup(line))
                    break;

                if (asmName == null)
                    asmName = ParseAssemblyName(line);

                var temp = ParseOutputPath(line);
                if (temp != null)
                    outputs.Add(temp);
            }
            return new ProjectFile(asmName, outputs);
        }

        private ProjectFile Read2017Projectfile(string projectFile, string[] lines)
        {
            string asmName = Path.GetFileNameWithoutExtension(projectFile) + ".dll";
            var outputs = new List<string>();

            foreach (var line in lines)
            {
                outputs.AddRange(ParseTargetFramework(line, asmName));
                outputs.AddRange(ParseTargetFrameworks(line, asmName));
            }
            return new ProjectFile(asmName, outputs);
        }

        private IEnumerable<string> ParseTargetFramework(string line, string asmName)
        {
            const string START = "<TargetFramework>";
            const string END = "</TargetFramework>";
            int start = line.IndexOf(START, StringComparison.Ordinal);
            int end = line.IndexOf(END, StringComparison.Ordinal);
            if (start >= 0 && end > start)
            {
                start += START.Length;
                var framework = line.Substring(start, end - start);
                yield return string.Join(Path.PathSeparator.ToString(), "bin", "Debug", framework, asmName);
                yield return string.Join(Path.PathSeparator.ToString(), "bin", "Release", framework, asmName);
            }
        }

        private IEnumerable<string> ParseTargetFrameworks(string line, string asmName)
        {
            const string START = "<TargetFramework>";
            const string END = "</TargetFramework>";
            int start = line.IndexOf(START, StringComparison.Ordinal);
            int end = line.IndexOf(END, StringComparison.Ordinal);
            if (start >= 0 && end > start)
            {
                start += START.Length;
                var frameworks = line.Substring(start, end - start).Split(';');
                foreach (var framework in frameworks)
                {
                    yield return string.Join(Path.PathSeparator.ToString(), "bin", "Debug", framework, asmName);
                    yield return string.Join(Path.PathSeparator.ToString(), "bin", "Release", framework, asmName);
                }
            }
        }

        private static bool IsItemGroup(string line)
        {
            return line.IndexOf("<ItemGroup>", StringComparison.Ordinal) >= 0;
        }

        static string ParseAssemblyName(string line)
        {
            const string startTag = "<AssemblyName>";
            const string endTag = "</AssemblyName>";

            // try to find start of assembly name
            int startIdx = line.IndexOf(startTag, StringComparison.Ordinal);
            if (startIdx < 0)
                return null;
            startIdx += startTag.Length;

            // try to find end of assembly name
            int endIdx = line.IndexOf(endTag, startIdx, StringComparison.Ordinal);
            if (endIdx < 0)
                return null;

            return line.Substring(startIdx, endIdx - startIdx);
        }

        static string ParseOutputPath(string line)
        {
            const string startTag = "<OutputPath>";
            const string endTag = "</OutputPath>";
            
            // try to find start of assembly name
            int startIdx = line.IndexOf(startTag, StringComparison.Ordinal);
            if (startIdx < 0)
                return null;
            startIdx += startTag.Length;

            // try to find end of assembly name
            int endIdx = line.IndexOf(endTag, startIdx, StringComparison.Ordinal);
            if (endIdx < 0)
                return null;

            return line.Substring(startIdx, endIdx - startIdx);
        }

        struct ProjectFile
        {
            public string AssemblyName { get; }
            public List<string> OutputPaths { get; }

            public ProjectFile(string assemblyName, List<string> outputPaths)
            {
                this.AssemblyName = assemblyName;
                this.OutputPaths = outputPaths;
            }
        }

        public event EventHandler<FoundProjectEventArgs> ProjectFound;
    }

    public class FoundProjectEventArgs : EventArgs
    {
        public string Folder { get; set; }
        public string Project { get; set; }
        public List<Build> Builds { get; set; } = new List<Build>();
    }

    public class Build : IDisposable
    {
        readonly FileSystemWatcher watcher;
        DateTime lastChanged;

        public string Folder { get; }

        public DateTime LastChangedUtc
        {
            get { return lastChanged; }
            set
            {
                lastChanged = value;
                Built?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler Built;

        public Build(string folder, string asmName)
        {
            Folder = folder;
            var path = Path.Combine(folder, asmName);
            LastChangedUtc = File.GetLastWriteTimeUtc(path);
            watcher = new FileSystemWatcher(folder, asmName);
            watcher.Changed += (sender, args) => LastChangedUtc = File.GetLastWriteTimeUtc(path);
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            watcher.Dispose();
        }

        public bool X64 => Folder.IndexOf("x64", StringComparison.OrdinalIgnoreCase) > 0;
        public bool X86 => Folder.IndexOf("x86", StringComparison.OrdinalIgnoreCase) > 0;
    }
}
