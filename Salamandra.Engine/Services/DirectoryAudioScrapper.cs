using Salamandra.Engine.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Services
{
    public class DirectoryAudioScrapper
    {
        private Dictionary<string, DirectoryAudioInfo> directories;

        private BackgroundWorker backgroundWorker;
        private Queue<string> scrapQueue;

        public DirectoryAudioScrapper()
        {
            this.directories = new Dictionary<string, DirectoryAudioInfo>();

            this.backgroundWorker = new BackgroundWorker();
            this.backgroundWorker.DoWork += BackgroundWorker_DoWork;
            this.backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            this.backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;

            this.scrapQueue = new Queue<string>();
        }

        private void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void CheckAndScan(string path)
        {

        }

        public void QueueAndScan(string path)
        {
            this.scrapQueue.Enqueue(path);

            if (!this.backgroundWorker.IsBusy)
            {
                this.backgroundWorker.RunWorkerAsync(new Queue<string>(scrapQueue));
                scrapQueue.Clear();
            }
        }
    }
}
