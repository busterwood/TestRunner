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
            var pf = await LoadProjectFile((string)testProjectFile);
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

        private async Task<ProjectFile> LoadProjectFile(string testProjectFile)
        {
            string asmName = null;
            var outputs = new List<string>();
            using (StreamReader r = new StreamReader(testProjectFile))
            {
                for(;;)
                {
                    var line = await r.ReadLineAsync();

                    // stop at EOF or first item group
                    if (line == null || IsItemGroup(line))
                        return new ProjectFile(asmName, outputs);
                    
                    if (asmName == null)
                        asmName = ParseAssemblyName(line);

                    var temp = ParseOutputPath(line);
                    if (temp != null)
                        outputs.Add(temp);
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
    }
}
