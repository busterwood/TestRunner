using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TestGui
{
    class ProjectFinder
    {
        readonly string[] wellKnowBuildsFolders = {
            "Debug", "Release",
            "Debug\\net46", "Release\\net46",
            "Debug\\netstandard1.6", "Release\\netstandard1.6",
            "X64\\Debug", "X64\\Release",
            "X86\\Debug", "X86\\Release"
        };
        readonly string rootFolder;

        public ProjectFinder(string rootFolder)
        {
            if (string.IsNullOrWhiteSpace(rootFolder))
                throw new ArgumentNullException(nameof(rootFolder));
            this.rootFolder = rootFolder;
        }

        public Task Find()
        {
            var projectFiles = Directory.EnumerateFiles(rootFolder, "*test*.csproj", SearchOption.AllDirectories);
            var tasks = projectFiles.Select(pf => Task.Factory.StartNew(FindBuilds, pf));
            return Task.WhenAll(tasks.ToArray());
        }

        private void FindBuilds(object testProjectFile)
        {
            var args = new FoundProjectEventArgs
            {
                Folder = Path.GetDirectoryName((string)testProjectFile),
                Project = FindAssemblyName((string)testProjectFile),
            };
            var bin = Path.Combine(args.Folder, "bin");
            if (Directory.Exists(bin))
            {
                args.Builds = wellKnowBuildsFolders
                    .Select(folder => Path.Combine(Path.Combine(bin, folder), args.Project + ".dll"))
                    .Where(dll => File.Exists(dll))
                    .Select(dll => new Build(Path.GetDirectoryName(dll), Path.GetFileName(dll)))
                    .ToList();
                ProjectFound?.Invoke(this, args);
            }
        }

        private string FindAssemblyName(string testProjectFile)
        {
            const string startTag = "<AssemblyName>";
            const string endTag = "</AssemblyName>";
            using (StreamReader r = new StreamReader(testProjectFile))
            {
                for(;;)
                {
                    var line = r.ReadLine();
                    if (line == null)
                        return Path.GetFileNameWithoutExtension(testProjectFile);
                    
                    // try to find start of assembly name
                    int startIdx = line.IndexOf(startTag, StringComparison.Ordinal); 
                    if (startIdx < 0)
                        continue;
                    startIdx += startTag.Length;

                    // try to find end of assembly name
                    int endIdx = line.IndexOf(endTag, startIdx, StringComparison.Ordinal); 
                    if (endIdx < 0)
                        continue;

                    var asmName = line.Substring(startIdx, endIdx - startIdx);
                    return asmName;
                }
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
