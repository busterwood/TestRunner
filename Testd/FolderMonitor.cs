using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Test.Daemon
{
    class FolderMonitor
    {
        readonly BlockingCollection<DateTime> changes = new BlockingCollection<DateTime>();
        readonly string folder;
        FileSystemWatcher exes;
        FileSystemWatcher dlls;

        public event EventHandler Changed;

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
            changes.Add(DateTime.UtcNow);

            Task.Run(() => StartWorker());
        }

        private void FileChanged(object sender, FileSystemEventArgs e)
        {
            changes.Add(DateTime.UtcNow);
        }

        void StartWorker()
        {
            for(;;)
            {
                changes.Take(); // wait for one change
                ReadAllChangesAsync(); // then consume all following ones until it stops changing
                Changed?.Invoke(this, EventArgs.Empty); // then run the tests
            }
        }

        private void ReadAllChangesAsync()
        {
            for(;;)
            {
                DateTime value;
                bool got = changes.TryTake(out value, TimeSpan.FromSeconds(2));
                if (!got)
                    return;
            }
        }
    }
}
