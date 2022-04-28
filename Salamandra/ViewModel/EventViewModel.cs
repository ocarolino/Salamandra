using Microsoft.Win32;
using Newtonsoft.Json;
using Salamandra.Commands;
using Salamandra.Engine.Domain.Enums;
using Salamandra.Engine.Domain.Events;
using Salamandra.Engine.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        public ICommand ComboTrackTypeChangedCommand { get; set; }
        public ICommand ValidateAndCloseCommand { get; set; }

        public Action<bool>? CloseWindow { get; set; }

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
            this.ComboTrackTypeChangedCommand = new RelayCommand(p => ComboTrackTypeChanged());
            this.ValidateAndCloseCommand = new RelayCommand(p => ValidateAndClose());
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
                case TrackScheduleType.FileTrack:
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "Arquivos de áudio (*.wav, *.mp3, *.wma, *.ogg, *.flac) | *.wav; *.mp3; *.wma; *.ogg; *.flac";
                    openFileDialog.Multiselect = true;

                    if (openFileDialog.ShowDialog() == true)
                        this.ScheduledEvent.Filename = openFileDialog.FileName;
                    break;
                case TrackScheduleType.RandomFileTrack:
                    Ookii.Dialogs.Wpf.VistaFolderBrowserDialog vistaFolderBrowserDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

                    if (vistaFolderBrowserDialog.ShowDialog() == true)
                        this.ScheduledEvent.Filename = vistaFolderBrowserDialog.SelectedPath;
                    break;
                case TrackScheduleType.TimeAnnouncementTrack:
                    throw new NotImplementedException();
            }
        }

        private void ComboTrackTypeChanged()
        {
            this.ScheduledEvent.Filename = String.Empty;

            switch (this.ScheduledEvent.TrackScheduleType)
            {
                case TrackScheduleType.TimeAnnouncementTrack:
                    this.EventRequiresPath = false;
                    break;
                default:
                    this.EventRequiresPath = true;
                    break;
            }
        }

        private void ValidateAndClose()
        {
            if (this.ScheduledEvent.UseExpirationDateTime &&
                this.ScheduledEvent.StartingDateTime >= this.ScheduledEvent.ExpirationDateTime)
            {
                MessageBox.Show("A data de expiração do evento deve ser posterior a data de início.", "Eventos",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (this.ScheduledEvent.UsePlayingHours && this.ScheduledEvent.PlayingHours.Count == 0)
            {
                MessageBox.Show("Os horários que o evento tocará devem ser selecionados.", "Eventos",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (this.ScheduledEvent.UseDaysOfWeek && this.ScheduledEvent.DaysOfWeek.Count == 0)
            {
                MessageBox.Show("Os dias que o evento tocará devem ser selecionados.", "Eventos",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (this.EventRequiresPath && String.IsNullOrWhiteSpace(this.ScheduledEvent.Filename))
            {
                MessageBox.Show("O arquivo do evento deve ser selecionado.", "Eventos",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            this.PrepareFriendlyName();

            this.CloseWindow?.Invoke(true);
        }

        private void PrepareFriendlyName()
        {
            switch (this.ScheduledEvent.TrackScheduleType)
            {
                case TrackScheduleType.FileTrack:
                    this.ScheduledEvent.FriendlyName = Path.GetFileNameWithoutExtension(this.ScheduledEvent.Filename);
                    break;
                case TrackScheduleType.RandomFileTrack:
                    this.ScheduledEvent.Filename = this.ScheduledEvent.Filename.EnsureHasDirectorySeparatorChar();
                    this.ScheduledEvent.FriendlyName = Path.GetFileName(this.ScheduledEvent.Filename.TrimEnd(Path.DirectorySeparatorChar));
                    break;
                case TrackScheduleType.TimeAnnouncementTrack:
                    this.ScheduledEvent.FriendlyName = "Locução de Hora";
                    break;
                default:
                    break;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
