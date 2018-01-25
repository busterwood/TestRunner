using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Tests.Daemon
{
    class FolderMonitor
    {
        readonly BlockingCollection<ChangedEventArgs> changes = new BlockingCollection<ChangedEventArgs>();
        readonly string folder;
        FileSystemWatcher exes;
        FileSystemWatcher dlls;

        public event EventHandler<ChangedEventArgs> Changed;

        public FolderMonitor(string folder)
        {
            this.folder = folder;
            StdErr.Info($"Monitoring '{folder}'");
        }

        public void Start()
        {
            exes = new FileSystemWatcher(folder, "*.exe");
            exes.Changed += FileChanged;
            exes.Created += FileChanged;
            exes.Deleted += FileChanged;
            exes.EnableRaisingEvents = true;
            dlls = new FileSystemWatcher(folder, "*.dll");
            dlls.Changed += FileChanged;
            dlls.Created += FileChanged;
            dlls.Deleted += FileChanged;
            dlls.EnableRaisingEvents = true;

            // start the tests
            changes.Add(new ChangedEventArgs());

            Task.Run(() => StartWorker());
        }

        private void FileChanged(object sender, FileSystemEventArgs e)
        {
            changes.Add(new ChangedEventArgs());
        }

        internal void ListTests()
        {
            changes.Add(new ChangedEventArgs { ListTests = true });
        }

        void StartWorker()
        {
            for(;;)
            {
                var args = changes.Take(); // wait for one change
                ReadAllChangesAsync(); // then consume all following ones until it stops changing
                Changed?.Invoke(this, args); // then run the tests
            }
        }

        private void ReadAllChangesAsync()
        {
            var timeout = TimeSpan.FromSeconds(0.9);
            for(;;)
            {
                ChangedEventArgs value;
                bool got = changes.TryTake(out value, timeout);
                if (!got)
                    return;
            }
        }

        public void RunNow()
        {
            changes.Add(new ChangedEventArgs { Debug = false });
        }

        public void DebugNow()
        {
            changes.Add(new ChangedEventArgs { Debug = true });
        }
    }

    public class ChangedEventArgs : EventArgs
    {
        public bool Debug { get; set; }
        public bool ListTests { get; set; }
    }
}
