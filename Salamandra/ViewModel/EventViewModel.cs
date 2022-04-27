using Microsoft.Win32;
using Newtonsoft.Json;
using Salamandra.Commands;
using Salamandra.Engine.Domain.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Salamandra.ViewModel
{
    public class EventViewModel : INotifyPropertyChanged
    {
        private ScheduledEvent? originalScheduledEvent;

        public ScheduledEvent ScheduledEvent { get; set; }

        public bool EventRequiresPath { get; set; }
        public ObservableCollection<int> Hours { get; set; }
        public ObservableCollection<DayOfWeek> Days { get; set; }

        public ICommand OpenPathDialogCommand { get; set; }

        public EventViewModel()
        {
            this.Hours = new ObservableCollection<int>();
            this.Days = new ObservableCollection<DayOfWeek>();

            for (int i = 0; i < 24; i++)
                this.Hours.Add(i);

            foreach (DayOfWeek item in Enum.GetValues(typeof(DayOfWeek)))
                this.Days.Add(item);

            this.ScheduledEvent = new ScheduledEvent();
            this.EventRequiresPath = true;

            this.OpenPathDialogCommand = new RelayCommand(p => OpenPathDialog(), p => this.EventRequiresPath);
        }

        public EventViewModel(ScheduledEvent scheduledEvent) : this()
        {
            this.originalScheduledEvent = scheduledEvent;
        }

        public void Loading()
        {
            if (this.originalScheduledEvent != null)
            {
                var serialized = JsonConvert.SerializeObject(this.originalScheduledEvent);
                var scheduledEvent = JsonConvert.DeserializeObject<ScheduledEvent>(serialized);

                this.ScheduledEvent = scheduledEvent;
            }
        }

        private void OpenPathDialog()
        {
            switch (this.ScheduledEvent.TrackScheduleType)
            {
                case Engine.Domain.Enums.TrackScheduleType.FileTrack:
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "Arquivos de áudio (*.wav, *.mp3, *.wma, *.ogg, *.flac) | *.wav; *.mp3; *.wma; *.ogg; *.flac";
                    openFileDialog.Multiselect = true;

                    if (openFileDialog.ShowDialog() == true)
                        this.ScheduledEvent.Filename = openFileDialog.FileName;
                    break;
                case Engine.Domain.Enums.TrackScheduleType.RandomFileTrack:
                    Ookii.Dialogs.Wpf.VistaFolderBrowserDialog vistaFolderBrowserDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

                    if (vistaFolderBrowserDialog.ShowDialog() == true)
                        this.ScheduledEvent.Filename = vistaFolderBrowserDialog.SelectedPath;
                    break;
                case Engine.Domain.Enums.TrackScheduleType.TimeAnnouncementTrack:
                    throw new NotImplementedException();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
