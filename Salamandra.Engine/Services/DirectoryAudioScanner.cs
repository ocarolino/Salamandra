using Newtonsoft.Json;
using Salamandra.Engine.Domain;
using Salamandra.Engine.Extensions;
using Salamandra.Engine.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Services
{
    public class DirectoryAudioScanner
    {
        private Dictionary<string, DirectoryAudioInfo> directoriesLibrary;

        private BackgroundWorker backgroundWorker;
        private Queue<string> scrapQueue;

        private List<string> filesBlackList;

        public DateTime LastDateTimeSaved { get; set; }
        public bool IsSaving { get; set; }

        public DirectoryAudioScanner()
        {
            this.directoriesLibrary = new Dictionary<string, DirectoryAudioInfo>();

            this.backgroundWorker = new BackgroundWorker();
            this.backgroundWorker.WorkerSupportsCancellation = true;
            this.backgroundWorker.WorkerReportsProgress = true;
            this.backgroundWorker.DoWork += BackgroundWorker_DoWork;
            this.backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            this.backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

            this.scrapQueue = new Queue<string>();
            this.filesBlackList = new List<string>();

            this.LastDateTimeSaved = DateTime.Now;
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
                    Debug.WriteLine(String.Format("Scanning {0}...", directory));

                    var files = Directory.EnumerateFiles(directory, "*.*", SearchOption.TopDirectoryOnly)
                        .Where(f => SoundEngine.SupportedAudioFormats.Any(e => f.EndsWith(e, StringComparison.OrdinalIgnoreCase))).ToList();

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

        public bool Enqueue(string path)
        {
            ArgumentNullException.ThrowIfNull(nameof(path));

            path = path.EnsureHasDirectorySeparatorChar();

            if (ShouldScanDirectory(path))
            {
                this.scrapQueue.Enqueue(path);
                return true;
            }

            return false;
        }

        public void EnqueueAndScan(string path)
        {
            ArgumentNullException.ThrowIfNull(nameof(path));

            if (Enqueue(path))
                StartScanning();
        }

        public void StartScanning()
        {
            if (this.backgroundWorker.IsBusy || this.scrapQueue.Count == 0)
                return;

            this.backgroundWorker.RunWorkerAsync(new Queue<string>(scrapQueue));
            scrapQueue.Clear();
        }

        public void StopScanning()
        {
            if (this.backgroundWorker.IsBusy)
                this.backgroundWorker.CancelAsync();
        }

        public void ScanLibrary()
        {
            foreach (var directory in this.directoriesLibrary)
                Enqueue(directory.Key);

            StartScanning();
        }
        #endregion

        public List<string> GetFilesFromDirectory(string path)
        {
            ArgumentNullException.ThrowIfNull(nameof(path));

            List<string> files = new List<string>();

            path = path.EnsureHasDirectorySeparatorChar();

            var keys = this.directoriesLibrary.Keys.Where(x => x.StartsWith(path));

            foreach (var item in keys)
                files.AddRange(this.directoriesLibrary[item].Files);

            return files;
        }

        public string? GetRandomFileFromDirectory(string path, bool blacklist = true)
        {
            ArgumentNullException.ThrowIfNull(nameof(path));

            var files = GetFilesFromDirectory(path);

            if (files.Count == 0)
                return null;

            var availableFiles = files.Except(this.filesBlackList).ToList();

            if (availableFiles.Count == 0)
            {
                availableFiles = files;
                filesBlackList = filesBlackList.Except(filesBlackList.Where(x => x.StartsWith(path))).ToList();
            }

            Random random = new Random();

            string file = availableFiles[random.Next(availableFiles.Count)];

            if (blacklist)
                this.filesBlackList.Add(file);

            return file;
        }

        private void SaveToFile(string filename)
        {
            ArgumentNullException.ThrowIfNull(nameof(filename));

            string json = JsonConvert.SerializeObject(this.directoriesLibrary);
            File.WriteAllText(filename, json);
        }

        private async Task SaveToFileAsync(string filename)
        {
            ArgumentNullException.ThrowIfNull(nameof(filename));

            string json = JsonConvert.SerializeObject(this.directoriesLibrary);
            await File.WriteAllTextAsync(filename, json);
        }

        private void LoadFromFile(string filename)
        {
            ArgumentNullException.ThrowIfNull(nameof(filename));

            try
            {
                var library = JsonConvert.DeserializeObject<Dictionary<string, DirectoryAudioInfo>>(File.ReadAllText(filename));

                if (library != null)
                    this.directoriesLibrary = library;
                else
                    this.directoriesLibrary = new Dictionary<string, DirectoryAudioInfo>();
            }
            catch (Exception)
            {
                this.directoriesLibrary = new Dictionary<string, DirectoryAudioInfo>();

                throw;
            }
        }

        public void Save(LogManager? logManager = null)
        {
            this.IsSaving = true;

            string filename = Path.Combine(FileSystemUtils.GetApplicationCurrentDirectory(), "directory_library.json");

            try
            {
                SaveToFile(filename);

                this.LastDateTimeSaved = DateTime.Now;
            }
            catch (Exception ex)
            {
                logManager?.Error(String.Format("There was an error when saving the directory library at {0}. ({1})",
                    filename, ex.Message), "Library");
            }

            this.IsSaving = false;
        }

        public void SavePeriodically(LogManager? logManager = null)
        {
            if (this.IsSaving)
                return;

            if (DateTime.Now.Subtract(this.LastDateTimeSaved).TotalMinutes >= 15) // ToDo: Const files.
            {
                this.IsSaving = true;

                string filename = Path.Combine(FileSystemUtils.GetApplicationCurrentDirectory(), "directory_library.json");

                var task = Task.Run(async () =>
                {
                    await SaveToFileAsync(filename);

                    this.LastDateTimeSaved = DateTime.Now;
                });

                task.ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        logManager?.Error(String.Format("There was an error when saving the directory library at {0}. ({1})",
                            filename, t.Exception!.Message), "Library");

                    this.IsSaving = false;
                });
            }
        }

        public void Load(LogManager? logManager = null)
        {
            string filename = Path.Combine(FileSystemUtils.GetApplicationCurrentDirectory(), "directory_library.json");

            if (File.Exists(filename))
            {
                try
                {
                    LoadFromFile(filename);
                    this.LastDateTimeSaved = DateTime.Now;
                }
                catch (Exception ex)
                {
                    logManager?.Error(String.Format("There was an error when loading the directory library at {0}. ({1})",
                        filename, ex.Message), "Library");
                }
            }

            ScanLibrary();
        }
    }
}
