using Microsoft.Win32;
using Newtonsoft.Json;
using Salamandra.Commands;
using Salamandra.Engine.Domain.Enums;
using Salamandra.Engine.Domain.Events;
using Salamandra.Engine.Domain.Settings;
using Salamandra.Engine.Extensions;
using Salamandra.Views;
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
        public ICommand OpenPreListenCommand { get; set; }
        public ICommand ImmediateChangedCommand { get; set; }

        public Action<bool>? CloseWindow { get; set; }

        public ApplicationSettings ApplicationSettings { get; set; }

        public EventViewModel(ApplicationSettings applicationSettings)
        {
            this.ApplicationSettings = applicationSettings;

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

            this.ImmediateChangedCommand = new RelayCommand(p => ImmediateChanged());

            this.OpenPreListenCommand = new RelayCommand(p => OpenPreListen(),
                p => this.ScheduledEvent.TrackScheduleType == TrackScheduleType.FileTrack && !String.IsNullOrEmpty(this.ScheduledEvent.Filename));
        }

        public EventViewModel(ScheduledEvent scheduledEvent, ApplicationSettings applicationSettings) : this(applicationSettings)
        {
            this.originalScheduledEvent = scheduledEvent;
        }

        public void Loading()
        {
            if (this.originalScheduledEvent != null)
            {
                var serialized = JsonConvert.SerializeObject(this.originalScheduledEvent);
                var scheduledEvent = JsonConvert.DeserializeObject<ScheduledEvent>(serialized);

                if (scheduledEvent != null)
                {
                    this.ScheduledEvent = scheduledEvent;
                    UpdateEventRequiresPath();
                }
            }
        }

        private void OpenPathDialog()
        {
            switch (this.ScheduledEvent.TrackScheduleType)
            {
                case TrackScheduleType.FileTrack:
                    OpenFileDialog audioOpenFileDialog = new OpenFileDialog();
                    audioOpenFileDialog.Filter = "Arquivos de áudio (*.wav, *.mp3, *.wma, *.ogg, *.flac) | *.wav; *.mp3; *.wma; *.ogg; *.flac";

                    if (audioOpenFileDialog.ShowDialog() == true)
                        this.ScheduledEvent.Filename = audioOpenFileDialog.FileName;
                    break;
                case TrackScheduleType.RandomFileTrack:
                    Ookii.Dialogs.Wpf.VistaFolderBrowserDialog vistaFolderBrowserDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

                    if (vistaFolderBrowserDialog.ShowDialog() == true)
                        this.ScheduledEvent.Filename = vistaFolderBrowserDialog.SelectedPath;
                    break;
                case TrackScheduleType.OpenPlaylistTrack:
                    OpenFileDialog playlistOpenFileDialog = new OpenFileDialog();
                    playlistOpenFileDialog.Filter = "Playlist M3U (*.m3u) | *.m3u";

                    if (playlistOpenFileDialog.ShowDialog() == true)
                        this.ScheduledEvent.Filename = playlistOpenFileDialog.FileName;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void ComboTrackTypeChanged()
        {
            this.ScheduledEvent.Filename = String.Empty;

            UpdateEventRequiresPath();
        }

        private void ImmediateChanged()
        {
            if (this.ScheduledEvent.Immediate)
                this.ScheduledEvent.UseMaximumWait = false;
        }

        private void UpdateEventRequiresPath()
        {
            switch (this.ScheduledEvent.TrackScheduleType)
            {
                case TrackScheduleType.FileTrack:
                case TrackScheduleType.RandomFileTrack:
                case TrackScheduleType.OpenPlaylistTrack:
                    this.EventRequiresPath = true;
                    break;
                default:
                    this.EventRequiresPath = false;
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

            if (this.ScheduledEvent.UseMaximumWait && this.ScheduledEvent.MaximumWaitTime <= TimeSpan.Zero)
            {
                MessageBox.Show("Um evento com espera máxima deve ter o tempo de espera maior que zero.", "Eventos",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (this.ScheduledEvent.TrackScheduleType == TrackScheduleType.StartPlaylistTrack &&
    !this.ScheduledEvent.Immediate)
            {
                MessageBox.Show("Um evento de iniciar a playlist que não seja imediato pode não ter o efeito desejado.", "Eventos",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }


            PrepareFriendlyName();
            UpdateTimestamps();

            this.CloseWindow?.Invoke(true);
        }

        private void UpdateTimestamps()
        {
            if (this.originalScheduledEvent == null)
            {
                this.ScheduledEvent.CreatedAt = DateTime.Now;
                this.ScheduledEvent.UpdatedAt = this.ScheduledEvent.CreatedAt;
            }
            else
            {
                this.ScheduledEvent.UpdatedAt = DateTime.Now;
            }
        }

        private void PrepareFriendlyName()
        {
            switch (this.ScheduledEvent.TrackScheduleType)
            {
                case TrackScheduleType.FileTrack:
                case TrackScheduleType.OpenPlaylistTrack:
                    this.ScheduledEvent.FriendlyName = Path.GetFileNameWithoutExtension(this.ScheduledEvent.Filename);
                    break;
                case TrackScheduleType.RandomFileTrack:
                    this.ScheduledEvent.Filename = this.ScheduledEvent.Filename.EnsureHasDirectorySeparatorChar();
                    this.ScheduledEvent.FriendlyName = Path.GetFileName(this.ScheduledEvent.Filename.TrimEnd(Path.DirectorySeparatorChar));
                    break;
                case TrackScheduleType.TimeAnnouncementTrack:
                    this.ScheduledEvent.FriendlyName = "Locução de Hora";
                    break;
                case TrackScheduleType.StartPlaylistTrack:
                    this.ScheduledEvent.FriendlyName = "Iniciar Playlist";
                    break;
                case TrackScheduleType.StopPlaylistTrack:
                    this.ScheduledEvent.FriendlyName = "Parar Playlist";
                    break;
                default:
                    break;
            }
        }

        private void OpenPreListen()
        {
            if (this.ScheduledEvent.TrackScheduleType != TrackScheduleType.FileTrack ||
                String.IsNullOrEmpty(this.ScheduledEvent.Filename))
                return;

            PreListenViewModel preListenViewModel = new PreListenViewModel(this.ApplicationSettings, this.ScheduledEvent.Filename);

            PreListenWindow preListenWindow = new PreListenWindow(preListenViewModel);
            preListenWindow.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            preListenWindow.ShowDialog();
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }
}
