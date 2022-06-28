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
                p => this.ScheduledEvent.TrackScheduleType == TrackScheduleType.AudioFileTrack && !String.IsNullOrEmpty(this.ScheduledEvent.Filename));
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
                case TrackScheduleType.AudioFileTrack:
                case TrackScheduleType.SystemProcessTrack:
                case TrackScheduleType.OpenPlaylistTrack:
                case TrackScheduleType.OpenScheduleTrack:
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = GetFileDialogFilter(this.ScheduledEvent.TrackScheduleType);

                    if (openFileDialog.ShowDialog() == true)
                        this.ScheduledEvent.Filename = openFileDialog.FileName;
                    break;
                case TrackScheduleType.RandomFileTrack:

                    Ookii.Dialogs.Wpf.VistaFolderBrowserDialog vistaFolderBrowserDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

                    if (vistaFolderBrowserDialog.ShowDialog() == true)
                        this.ScheduledEvent.Filename = vistaFolderBrowserDialog.SelectedPath;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private string GetFileDialogFilter(TrackScheduleType trackScheduleType)
        {
            // ToDo: Traduções desses filtros
            switch (trackScheduleType)
            {
                case TrackScheduleType.AudioFileTrack:
                    return "Arquivos de áudio (*.wav, *.mp3, *.wma, *.ogg, *.flac) | *.wav; *.mp3; *.wma; *.ogg; *.flac";
                case TrackScheduleType.OpenPlaylistTrack:
                    return "Playlist M3U (*.m3u) | *.m3u";
                case TrackScheduleType.SystemProcessTrack:
                    return "Todos os arquivos (*.*) | *.*";
                case TrackScheduleType.OpenScheduleTrack:
                    return "Lista de Eventos (*.sche) | *.sche";
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
                case TrackScheduleType.AudioFileTrack:
                case TrackScheduleType.RandomFileTrack:
                case TrackScheduleType.OpenPlaylistTrack:
                case TrackScheduleType.SystemProcessTrack:
                case TrackScheduleType.OpenScheduleTrack:
                    this.EventRequiresPath = true;
                    break;
                default:
                    this.EventRequiresPath = false;
                    break;
            }
        }

        private void ValidateAndClose()
        {
            // ToDo: Considerar somente o dia em casos de eventos que usam mais de um horário
            if (this.ScheduledEvent.UseExpirationDateTime &&
                this.ScheduledEvent.StartingDateTime >= this.ScheduledEvent.ExpirationDateTime)
            {
                MessageBox.Show(Salamandra.Strings.ViewsTexts.EventWindow_Validation_ExpirationAfterStarting,
                    Salamandra.Strings.ViewsTexts.EventWindow_WindowTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (this.ScheduledEvent.UsePlayingHours && this.ScheduledEvent.PlayingHours.Count == 0)
            {
                MessageBox.Show(Salamandra.Strings.ViewsTexts.EventWindow_Validation_MustSelectHours,
                    Salamandra.Strings.ViewsTexts.EventWindow_WindowTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (this.ScheduledEvent.UseDaysOfWeek && this.ScheduledEvent.DaysOfWeek.Count == 0)
            {
                MessageBox.Show(Salamandra.Strings.ViewsTexts.EventWindow_Validation_MustSelectDays,
                    Salamandra.Strings.ViewsTexts.EventWindow_WindowTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (this.EventRequiresPath && String.IsNullOrWhiteSpace(this.ScheduledEvent.Filename))
            {
                MessageBox.Show(Salamandra.Strings.ViewsTexts.EventWindow_Validation_MustSelectFile,
                    Salamandra.Strings.ViewsTexts.EventWindow_WindowTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (this.ScheduledEvent.UseMaximumWait && this.ScheduledEvent.MaximumWaitTime <= TimeSpan.Zero)
            {
                MessageBox.Show(Salamandra.Strings.ViewsTexts.EventWindow_Validation_WaitingTimeGreaterThanZero,
                    Salamandra.Strings.ViewsTexts.EventWindow_WindowTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (this.ScheduledEvent.TrackScheduleType == TrackScheduleType.StartPlaylistTrack &&
                !this.ScheduledEvent.Immediate)
            {
                MessageBox.Show(Salamandra.Strings.ViewsTexts.EventWindow_Validation_DelayedStartPlayback,
                    Salamandra.Strings.ViewsTexts.EventWindow_WindowTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }

            this.ScheduledEvent.UpdateFriendlyName();
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

        private void OpenPreListen()
        {
            if (this.ScheduledEvent.TrackScheduleType != TrackScheduleType.AudioFileTrack ||
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
