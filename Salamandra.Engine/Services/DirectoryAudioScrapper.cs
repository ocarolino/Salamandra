using Salamandra.Engine.Domain;
using Salamandra.Engine.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Services
{
    public class DirectoryAudioScrapper
    {
        private Dictionary<string, DirectoryAudioInfo> directoriesLibrary;

        private BackgroundWorker backgroundWorker;
        private Queue<string> scrapQueue;

        public DirectoryAudioScrapper()
        {
            this.directoriesLibrary = new Dictionary<string, DirectoryAudioInfo>();

            this.backgroundWorker = new BackgroundWorker();
            this.backgroundWorker.WorkerSupportsCancellation = true;
            this.backgroundWorker.WorkerReportsProgress = true;
            this.backgroundWorker.DoWork += BackgroundWorker_DoWork;
            this.backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            this.backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

            this.scrapQueue = new Queue<string>();
        }

        #region Background Worker Events
        private void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null && !e.Cancelled)
            {
                Debug.WriteLine("Directory scanning ended sucefully.");

                if (this.scrapQueue.Count > 0)
                {
                    Debug.WriteLine("Still some directories on folder, restarting scanning.");
                    StartScanning();
                }
            }
        }

        private void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            if (this.backgroundWorker.CancellationPending == false)
            {
                DirectoryAudioInfo directoryAudioInfo = (DirectoryAudioInfo)e.UserState!;
                directoryAudioInfo.LastScanDate = DateTime.Now;

                Debug.WriteLine(String.Format("Directory {0} finished scan.", directoryAudioInfo.DirectoryPath));

                if (this.directoriesLibrary.ContainsKey(directoryAudioInfo.DirectoryPath))
                {
                    if (directoryAudioInfo.Files.Count > 0)
                    {
                        DirectoryAudioInfo existingDirectoryAudioInfo = this.directoriesLibrary[directoryAudioInfo.DirectoryPath];
                        existingDirectoryAudioInfo.LastScanDate = directoryAudioInfo.LastScanDate;
                        existingDirectoryAudioInfo.Files = new List<string>(directoryAudioInfo.Files);

                        Debug.WriteLine(String.Format("Directory {0} updated.", directoryAudioInfo.DirectoryPath));
                    }
                    else
                    {
                        this.directoriesLibrary.Remove(directoryAudioInfo.DirectoryPath);
                        Debug.WriteLine(String.Format("Directory {0} removed from library.", directoryAudioInfo.DirectoryPath));
                    }
                }
                else
                {
                    if (directoryAudioInfo.Files.Count > 0)
                    {
                        this.directoriesLibrary.Add(directoryAudioInfo.DirectoryPath, directoryAudioInfo);
                        Debug.WriteLine(String.Format("Directory {0} added to library.", directoryAudioInfo.DirectoryPath));
                    }
                }
            }
        }

        private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            Queue<string> directories = (Queue<string>)e.Argument!;

            while (directories.Count > 0)
            {
                if (this.backgroundWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                string directory = directories.Dequeue().EnsureHasDirectorySeparatorChar();
                DirectoryAudioInfo directoryAudioInfo = new DirectoryAudioInfo(directory);

                if (Directory.Exists(directory))
                {
                    string[] supportedFiles = { ".wav", ".mp3", ".wma", ".ogg", ".flac" };

                    Debug.WriteLine(String.Format("Scanning {0}...", directory));

                    var files = Directory.EnumerateFiles(directory, "*.*", SearchOption.TopDirectoryOnly)
                        .Where(f => supportedFiles.Any(f.ToLower().EndsWith)).ToList();

                    var subDirs = Directory.EnumerateDirectories(directory);

                    foreach (var item in subDirs)
                        directories.Enqueue(item);

                    directoryAudioInfo.Files = files;
                }
                this.backgroundWorker.ReportProgress(0, directoryAudioInfo);

            }
        }
        #endregion

        #region Scan Methods
        public void CheckAndScan(string path)
        {
            path = path.EnsureHasDirectorySeparatorChar();

            if (ShouldScanDirectory(path))
                QueueAndScan(path);
        }

        public bool ShouldScanDirectory(string path)
        {
            if (!this.directoriesLibrary.ContainsKey(path))
                return true;

            DirectoryAudioInfo directoryAudioInfo = this.directoriesLibrary[path];
            double diffInSeconds = (DateTime.Now - directoryAudioInfo.LastScanDate).TotalSeconds;

            if (directoryAudioInfo.Files.Count == 0)
                return true;

            if (diffInSeconds >= 300)
                return true;

            return false;
        }

        public void QueueAndScan(string path)
        {
            this.scrapQueue.Enqueue(path);

            if (!this.backgroundWorker.IsBusy)
                StartScanning();
        }

        private void StartScanning()
        {
            this.backgroundWorker.RunWorkerAsync(new Queue<string>(scrapQueue));
            scrapQueue.Clear();
        }

        public void StopScanning()
        {
            if (this.backgroundWorker.IsBusy)
                this.backgroundWorker.CancelAsync();
        }
        #endregion

        public List<string> GetFilesFromDirectory(string path)
        {
            List<string> files = new List<string>();

            var keys = this.directoriesLibrary.Keys.Where(x => x.StartsWith(path));

            foreach (var item in keys)
                files.AddRange(this.directoriesLibrary[item].Files);

            return files;
        }
    }
}
