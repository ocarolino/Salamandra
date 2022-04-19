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
using Salamandra.Engine.Domain.Tracks;

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

        public bool AllowSeekDrag { get; set; }

        private bool isDraggingTrackPosition;
        public double TrackLengthInSeconds { get; set; }
        public double TrackPositionInSeconds { get; set; }
        public TimeSpan TrackPositionTime { get => TimeSpan.FromSeconds(this.TrackPositionInSeconds); }
        public TimeSpan TrackLengthTime { get => TimeSpan.FromSeconds(this.TrackLengthInSeconds); }

        public AudioFileTrack? SelectedTrack { get; set; }

        public DispatcherTimer MainTimer { get; set; }

        public string? WindowTitle { get; set; }

        public bool PlaylistLoading { get; set; }
        public string PlaylistInfoText { get; set; }

        public TimeSpan? RemainingTime { get; set; }
        public TimeSpan? EndingTimeOfDay { get; set; }
        public DateTime CurrentDateTime { get; set; }

        #region Commands Properties
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
        public ICommand? UpdateNextTrackCommand { get; set; }
        public ICommand? OpenPlaylistCommand { get; set; }
        public ICommand? SavePlaylistCommand { get; set; }
        public ICommand? SavePlaylistAsCommand { get; set; }
        public ICommand? NewPlaylistCommand { get; set; }
        public ICommand? ShufflePlaylistCommand { get; set; }
        #endregion

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

            this.PlaylistLoading = false;
            this.PlaylistInfoText = string.Empty;

            UpdateWindowTitle();
            LoadCommands();

            this.SoundEngine.EnumerateDevices();
        }

        private void MainTimer_Tick(object? sender, EventArgs e)
        {
            this.CurrentDateTime = DateTime.Now;

            if (!this.IsPlaying)
                return;

            if (this.PlaybackState == PlaylistState.WaitingNextTrack)
                PlayNextTrackOrStop();

            if (this.SoundEngine.State == SoundEngineState.Playing && !this.isDraggingTrackPosition)
            {
                this.TrackPositionInSeconds = this.SoundEngine.PositionInSeconds;
                this.RemainingTime = TimeSpan.FromSeconds(this.TrackLengthInSeconds - this.TrackPositionInSeconds);
            }
        }

        public bool Closing()
        {
            if (this.IsPlaying)
            {
                if (MessageBox.Show("A playlist ainda está tocando. Tem certeza que deseja fechar?", "Salamandra", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return false;
            }

            if (!CheckPlaylistModified())
                return false;

            this.StopPlayback();

            return true;
        }

        private void LoadCommands()
        {
            this.AddFilesToPlaylistCommand = new RelayCommandAsync(p => AddFilesToPlaylist(), p => HandlePlaylistException(p), p => !this.PlaylistLoading);
            this.RemoveTracksFromPlaylistCommand = new RelayCommand(p => RemoveTracksFromPlaylist(p), p => !this.PlaylistLoading);

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

            this.UpdateNextTrackCommand = new RelayCommand(p => this.PlaylistManager.UpdateNextTrack(), p => true);

            this.OpenPlaylistCommand = new RelayCommandAsync(p => OpenPlaylist(), p => HandlePlaylistException(p), p => !this.PlaylistLoading);
            this.SavePlaylistCommand = new RelayCommand(p => SavePlaylist(), p => !this.PlaylistLoading);
            this.SavePlaylistAsCommand = new RelayCommand(p => SavePlaylist(true), p => !this.PlaylistLoading);
            this.NewPlaylistCommand = new RelayCommand(p => NewPlaylist(), p => !this.PlaylistLoading);

            this.ShufflePlaylistCommand = new RelayCommand(p => this.PlaylistManager.ShufflePlaylist(), p => !this.PlaylistLoading);
        }

        private async Task AddFilesToPlaylist()
        {
            this.PlaylistLoading = true;
            this.PlaylistInfoText = "Adicionando arquivos a playlist...";

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Arquivos de áudio (*.wav, *.mp3, *.wma, *.ogg, *.flac) | *.wav; *.mp3; *.wma; *.ogg; *.flac";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
                await this.PlaylistManager.AddFiles(openFileDialog.FileNames.ToList());

            this.PlaylistLoading = false;
            this.PlaylistInfoText = string.Empty;
        }

        private void RemoveTracksFromPlaylist(object? items)
        {
            if (items == null || !(items is System.Collections.IList))
                return;

            List<AudioFileTrack> tracks = ((System.Collections.IList)items).Cast<AudioFileTrack>().ToList();

            this.PlaylistManager.RemoveTracks(tracks);
        }

        private void StartPlayback()
        {
            if (this.PlaylistManager.NextTrack == null)
                return;

            this.IsPlaying = true;

            PlayTrack(this.PlaylistManager.NextTrack);
        }

        private void PlayTrack(AudioFileTrack soundFileTrack)
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
                this.CalculateEndingTimeOfDay(false);

                if (soundFileTrack.Duration == null) // ToDo: Refatorar para garantir que isso seja necessário!
                    soundFileTrack.Duration = TimeSpan.FromSeconds(this.SoundEngine.TotalLengthInSeconds);

                this.AllowSeekDrag = true;
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
            this.RemainingTime = null;
            this.EndingTimeOfDay = null;
            this.AllowSeekDrag = false;

            if (this.PlaylistManager.NextTrack == null)
                this.PlaylistManager.UpdateNextTrack(); // ToDo: Quando houver manual.
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

            if (this.IsPaused)
                this.EndingTimeOfDay = null;
            else
                CalculateEndingTimeOfDay();
        }

        private void SeekBarDragCompleted()
        {
            Debug.WriteLine("SeekBarDragCompleted - Value: " + this.TrackPositionInSeconds.ToString());

            this.SoundEngine.PositionInSeconds = this.TrackPositionInSeconds;

            if (!this.IsPaused)
            {
                this.RemainingTime = TimeSpan.FromSeconds(this.TrackLengthInSeconds - this.TrackPositionInSeconds);
                this.CalculateEndingTimeOfDay();
            }

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

        private async Task OpenPlaylist()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Playlist M3U (*.m3u) | *.m3u";

            if (openFileDialog.ShowDialog() == true)
            {
                this.PlaylistLoading = true;
                this.PlaylistInfoText = "Carregando playlist...";

                try
                {
                    await this.PlaylistManager.LoadPlaylist(openFileDialog.FileName);
                }
                catch (PlaylistLoaderException ex)
                {
                    MessageBox.Show("Houve um erro ao abrir a playlist.\n\n" + ex.Message, "Salamandra", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                UpdateWindowTitle();

                this.PlaylistLoading = false;
                this.PlaylistInfoText = string.Empty;
            }
        }

        private void SavePlaylist(bool saveDialog = false)
        {
            string filename = this.PlaylistManager.Filename;

            if (saveDialog || String.IsNullOrEmpty(filename))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Playlist M3U (*.m3u) | *.m3u";

                if (saveFileDialog.ShowDialog() == true)
                    filename = saveFileDialog.FileName;
                else
                    return;
            }

            this.PlaylistManager.SavePlaylist(filename);

            UpdateWindowTitle();
        }

        private void NewPlaylist()
        {
            if (CheckPlaylistModified())
                this.PlaylistManager.ClearPlaylist();
        }

        private bool CheckPlaylistModified()
        {
            if (this.PlaylistManager.Modified)
            {
                var result = MessageBox.Show("Há modificações não salvas na playlist. Deseja salvá-las?", "Salamandra", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                    SavePlaylist();
                else if (result == MessageBoxResult.Cancel)
                    return false;
            }

            return true;
        }

        private void UpdateWindowTitle()
        {
            string title = "Sem título";

            if (!String.IsNullOrEmpty(this.PlaylistManager.Filename))
                title = Path.GetFileName(this.PlaylistManager.Filename);

            if (!String.IsNullOrEmpty(title))
                title = title + " - ";

            title = title + "Salamandra";

            this.WindowTitle = title;
        }

        private void HandlePlaylistException(Exception ex)
        {
            MessageBox.Show("Houve um erro ao manipular a playlist.\n\n" + ex.Message, "Salamandra", MessageBoxButton.OK, MessageBoxImage.Error);
            this.PlaylistLoading = false;
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
                this.StopPlaybackWithError(e.SoundErrorException!);
        }

        private void CalculateEndingTimeOfDay(bool useRemainingTime = true)
        {
            this.EndingTimeOfDay = DateTime.Now.TimeOfDay + (useRemainingTime ? this.RemainingTime : this.TrackLengthTime);
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }
}
