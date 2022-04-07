using Microsoft.Win32;
using Salamandra.Commands;
using Salamandra.Engine.Domain;
using Salamandra.Engine.Domain.Enums;
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
    public class MainViewModel : INotifyPropertyChanged
    {
        public SoundEngine SoundEngine { get; set; }
        public PlaylistManager PlaylistManager { get; set; }

        public PlaylistState PlaybackState { get; set; }
        public bool IsPlaying { get; set; }
        public bool IsPaused { get; set; }
        public float CurrentVolume { get; set; }

        private bool isDraggingTrackPosition;
        public double TrackLengthInSeconds { get; set; }
        public double TrackPositionInSeconds { get; set; }
        public TimeSpan TrackPositionTime { get => TimeSpan.FromSeconds(this.TrackPositionInSeconds); }
        public TimeSpan TrackLengthTime { get => TimeSpan.FromSeconds(this.TrackLengthInSeconds); }

        public SoundFileTrack? SelectedTrack { get; set; }

        public DispatcherTimer MainTimer { get; set; }

        public ICommand? AddFilesToPlaylistCommand { get; set; }
        public ICommand? RemoveTracksFromPlaylistCommand { get; set; }
        public ICommand? StartPlaybackCommand { get; set; }
        public ICommand? StopPlaybackCommand { get; set; }
        public ICommand? PlaySelectedTrackCommand { get; set; }
        public ICommand? SelectedAsNextTrackCommand { get; set; }
        public ICommand? VolumeControlValueChangedCommand { get; set; }
        public ICommand? TogglePlayPauseCommand { get; set; }
        public ICommand? SeekBarDragStartedCommand { get; set; }
        public ICommand? SeekBarDragCompletedCommand { get; set; }
        public ICommand? NextTrackCommand { get; set; }
        public ICommand? StopAfterCurrentCommand { get; set; }

        public MainViewModel()
        {
            this.SoundEngine = new SoundEngine();
            this.SoundEngine.SoundStopped += SoundEngine_SoundStopped;
            this.SoundEngine.SoundError += SoundEngine_SoundError;
            this.PlaylistManager = new PlaylistManager();
            this.PlaylistManager.PlaylistMode = PlaylistMode.Random;

            this.IsPlaying = false;
            this.PlaybackState = PlaylistState.Stopped;

            this.CurrentVolume = 1; // ToDo: Min e Max via SoundEngine

            this.isDraggingTrackPosition = false;
            this.TrackLengthInSeconds = 0;
            this.TrackPositionInSeconds = 0;

            this.MainTimer = new DispatcherTimer();
            this.MainTimer.Interval = TimeSpan.FromMilliseconds(250);
            this.MainTimer.Tick += MainTimer_Tick;
            this.MainTimer.Start();

            LoadCommands();

            this.SoundEngine.EnumerateDevices();
        }

        private void MainTimer_Tick(object? sender, EventArgs e)
        {
            if (!this.IsPlaying)
                return;

            if (this.PlaybackState == PlaylistState.WaitingNextTrack)
                PlayNextTrackOrStop();

            if (this.SoundEngine.State == SoundEngineState.Playing && !this.isDraggingTrackPosition)
                this.TrackPositionInSeconds = this.SoundEngine.PositionInSeconds;
        }

        public void Closing()
        {
            this.StopPlayback();
        }

        private void LoadCommands()
        {
            this.AddFilesToPlaylistCommand = new RelayCommand(p => AddFilesToPlaylist(), p => true);
            this.RemoveTracksFromPlaylistCommand = new RelayCommand(p => RemoveTracksFromPlaylist(p), p => true);

            this.StartPlaybackCommand = new RelayCommand(p => StartPlayback(), p => !this.IsPlaying);
            this.StopPlaybackCommand = new RelayCommand(p => StopPlayback(), p => this.IsPlaying);

            this.PlaySelectedTrackCommand = new RelayCommand(p => PlaySelectedTrack(), p => this.SelectedTrack != null);
            this.SelectedAsNextTrackCommand = new RelayCommand(p => SetSelectedAsNextTrack(), p => this.SelectedTrack != null);

            this.VolumeControlValueChangedCommand = new RelayCommand(p => VolumeControlValueChanged(), p => true);

            this.TogglePlayPauseCommand = new RelayCommand(p => TogglePlayPause(), p => this.IsPlaying);

            this.SeekBarDragStartedCommand = new RelayCommand(p => SeekBarDragStarted(), p => this.IsPlaying);
            this.SeekBarDragCompletedCommand = new RelayCommand(p => SeekBarDragCompleted(), p => this.IsPlaying);

            this.NextTrackCommand = new RelayCommand(p => NextTrack(), p => this.IsPlaying && this.PlaylistManager.NextTrack != null);
            this.StopAfterCurrentCommand = new RelayCommand(p => StopAfterCurrent(), p => this.IsPlaying);
        }

        private void AddFilesToPlaylist()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Arquivos de áudio (*.wav, *.mp3, *.wma, *.ogg, *.flac) | *.wav; *.mp3; *.wma; *.ogg; *.flac";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                List<SoundFileTrack> tracks = new List<SoundFileTrack>();

                foreach (var item in openFileDialog.FileNames)
                {
                    SoundFileTrack soundFileTrack = new SoundFileTrack(item, Path.GetFileNameWithoutExtension(item));
                    tracks.Add(soundFileTrack);
                }

                this.PlaylistManager.AddTracks(tracks);
            }
        }

        private void RemoveTracksFromPlaylist(object? items)
        {
            if (items == null || !(items is System.Collections.IList))
                return;

            List<SoundFileTrack> tracks = ((System.Collections.IList)items).Cast<SoundFileTrack>().ToList();

            this.PlaylistManager.RemoveTracks(tracks);
        }

        private void StartPlayback()
        {
            if (this.PlaylistManager.NextTrack == null)
                return;

            this.IsPlaying = true;

            PlayTrack(this.PlaylistManager.NextTrack);
        }

        private void PlayTrack(SoundFileTrack soundFileTrack)
        {
            this.IsPaused = false;

            this.PlaylistManager.CurrentTrack = soundFileTrack;
            this.PlaylistManager.UpdateNextTrack();

            // ToDo: Tratamento de erros...
            try
            {
                this.SoundEngine.PlayAudioFile(soundFileTrack.Filename, this.CurrentVolume);

                this.PlaybackState = PlaylistState.PlayingPlaylistTrack; // ToDo: Refatorar quando for evento!
                this.TrackLengthInSeconds = this.SoundEngine.TotalLengthInSeconds;
                this.TrackPositionInSeconds = 0;
            }
            catch (SoundEngineFileException)
            {
                this.PlaybackState = PlaylistState.WaitingNextTrack;
            }
            catch (SoundEngineDeviceException ex)
            {
                // ToDo: Mensagem!

                this.StopPlaybackWithError(ex);
            }
            catch (Exception ex)
            {
                // ToDo: Mensagem!

                this.StopPlaybackWithError(ex);
            }
        }

        private void PlayNextTrackOrStop()
        {
            if (this.PlaylistManager.NextTrack != null)
                PlayTrack(this.PlaylistManager.NextTrack);
            else
                StopPlayback();
        }

        private void StopPlayback()
        {
            this.IsPlaying = false;
            this.IsPaused = false;
            this.PlaybackState = PlaylistState.Stopped;
            this.SoundEngine.Stop();

            this.PlaylistManager.CurrentTrack = null;
            this.TrackLengthInSeconds = 0;
            this.TrackPositionInSeconds = 0;
        }

        private void StopPlaybackWithError(Exception ex)
        {
            this.StopPlayback();

            MessageBox.Show(
                String.Format("Houve um erro no dispositivo de áudio que forçou a parada da reprodução.\n\nErro: {0}", ex.Message),
                "Salamandra", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void PlaySelectedTrack()
        {
            if (this.SelectedTrack == null)
                return;

            this.PlaylistManager.NextTrack = this.SelectedTrack;

            if (this.IsPlaying)
                this.SoundEngine.Stop();
            else
                this.StartPlayback();
        }

        private void SetSelectedAsNextTrack()
        {
            if (this.SelectedTrack != null)
                this.PlaylistManager.NextTrack = this.SelectedTrack;
        }

        private void VolumeControlValueChanged()
        {
            if (this.IsPlaying)
                this.SoundEngine.Volume = this.CurrentVolume;
        }

        private void TogglePlayPause()
        {
            this.SoundEngine.TogglePlayPause();

            this.IsPaused = (this.SoundEngine.State == SoundEngineState.Paused);
        }

        private void SeekBarDragCompleted()
        {
            Debug.WriteLine("SeekBarDragCompleted - Value: " + this.TrackPositionInSeconds.ToString());

            this.SoundEngine.PositionInSeconds = this.TrackPositionInSeconds;
            this.isDraggingTrackPosition = false;
        }

        private void SeekBarDragStarted()
        {
            Debug.WriteLine("SeekBarDragStarted - Value: " + this.TrackPositionInSeconds.ToString());

            this.isDraggingTrackPosition = true;
        }

        private void NextTrack()
        {
            if (this.IsPlaying)
                this.SoundEngine.Stop();
        }

        private void StopAfterCurrent()
        {
            if (this.IsPlaying)
                this.PlaylistManager.NextTrack = null;
        }

        private void SoundEngine_SoundStopped(object? sender, Engine.Events.SoundStoppedEventArgs e)
        {
            if (this.IsPlaying)
                PlayNextTrackOrStop();
        }

        private void SoundEngine_SoundError(object? sender, Engine.Events.SoundErrorEventArgs e)
        {
            if (e.SoundErrorException is SoundEngineFileException)
                this.PlaybackState = PlaylistState.WaitingNextTrack;
            else
                this.StopPlaybackWithError(e.SoundErrorException);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
