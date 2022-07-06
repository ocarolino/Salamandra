using Salamandra.Commands;
using Salamandra.Engine.Domain.Settings;
using Salamandra.Engine.Exceptions;
using Salamandra.Engine.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Salamandra.ViewModel
{
    public class PreListenViewModel : INotifyPropertyChanged
    {
        private bool isDraggingTrackPosition;

        public ApplicationSettings ApplicationSettings { get; set; }
        public string Filename { get; set; }

        public SoundEngine SoundEngine { get; set; }
        public bool HasSoundStopped { get; set; }

        public DispatcherTimer MainTimer { get; set; }

        public string FriendlyName { get; set; }

        public bool AllowSeekDrag { get; set; }
        public double TrackLengthInSeconds { get; set; }
        public double TrackPositionInSeconds { get; set; }
        public TimeSpan TrackPositionTime { get => TimeSpan.FromSeconds(this.TrackPositionInSeconds); }
        public TimeSpan TrackLengthTime { get => TimeSpan.FromSeconds(this.TrackLengthInSeconds); }

        public float CurrentVolume { get; set; }

        public ICommand? StopPlaybackCommand { get; set; }
        public ICommand? SeekBarDragStartedCommand { get; set; }
        public ICommand? SeekBarDragCompletedCommand { get; set; }

        public ICommand? VolumeControlValueChangedCommand { get; set; }

        public Action? CloseHandler { get; set; }

        public PreListenViewModel(ApplicationSettings applicationSettings, string filename)
        {
            this.ApplicationSettings = applicationSettings;

            this.CurrentVolume = applicationSettings.PlayerSettings.PreListenVolume;

            this.Filename = filename;

            this.SoundEngine = new SoundEngine() { OutputDevice = this.ApplicationSettings.DeviceSettings.PreListenOutputDevice };
            this.SoundEngine.SoundStopped += SoundEngine_SoundStopped;
            this.SoundEngine.SoundError += SoundEngine_SoundError;

            this.MainTimer = new DispatcherTimer();
            this.MainTimer.Interval = TimeSpan.FromMilliseconds(250);
            this.MainTimer.Tick += MainTimer_Tick;

            this.FriendlyName = String.Empty;

            LoadCommands();
        }

        private void SoundEngine_SoundError(object? sender, Engine.Events.SoundErrorEventArgs e)
        {
            DisplayError(e.SoundErrorException!.Message);
            CloseWindow();
        }

        private void CloseWindow()
        {
            this.ApplicationSettings.PlayerSettings.PreListenVolume = this.CurrentVolume;

            this.HasSoundStopped = true;
            this.CloseHandler?.Invoke();
        }

        private void SoundEngine_SoundStopped(object? sender, Engine.Events.SoundStoppedEventArgs e)
        {
            CloseWindow();
        }

        private void MainTimer_Tick(object? sender, EventArgs e)
        {
            if (!this.isDraggingTrackPosition)
                this.TrackPositionInSeconds = this.SoundEngine.PositionInSeconds;
        }

        private void LoadCommands()
        {
            this.StopPlaybackCommand = new RelayCommand(p => StopPlayback());

            this.SeekBarDragStartedCommand = new RelayCommand(p => SeekBarDragStarted(), p => this.AllowSeekDrag);
            this.SeekBarDragCompletedCommand = new RelayCommand(p => SeekBarDragCompleted(), p => this.AllowSeekDrag);

            this.VolumeControlValueChangedCommand = new RelayCommand(p => VolumeControlValueChanged());
        }

        public void Loading()
        {
            PlayAudioFile();
        }

        private void PlayAudioFile()
        {
            try
            {
                this.SoundEngine.PlayAudioFile(this.Filename, this.CurrentVolume);
                this.FriendlyName = Path.GetFileNameWithoutExtension(this.Filename);

                this.TrackLengthInSeconds = this.SoundEngine.TotalLengthInSeconds;
                this.TrackPositionInSeconds = 0;

                this.AllowSeekDrag = true;

                this.MainTimer.Start();
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);
                CloseWindow();
            }
        }

        public void StopPlayback()
        {
            this.MainTimer.Stop();
            this.SoundEngine.Stop();
        }

        private void SeekBarDragStarted()
        {
            Debug.WriteLine("SeekBarDragStarted - Value: " + this.TrackPositionInSeconds.ToString());

            this.isDraggingTrackPosition = true;
        }

        private void SeekBarDragCompleted()
        {
            Debug.WriteLine("SeekBarDragCompleted - Value: " + this.TrackPositionInSeconds.ToString());

            this.SoundEngine.PositionInSeconds = this.TrackPositionInSeconds;

            this.isDraggingTrackPosition = false;
        }

        private void VolumeControlValueChanged()
        {
            this.SoundEngine.Volume = this.CurrentVolume;
        }

        private void DisplayError(string message)
        {
            MessageBox.Show(String.Format("{0}\n\n{1}", Salamandra.Strings.ViewsTexts.PreListenWindow_Error_PlaybackError, message),
                Salamandra.Strings.ViewsTexts.Misc_ApplicationTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }
}
