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
using Salamandra.Engine.Extensions;
using Salamandra.Engine.Domain.Settings;
using Salamandra.Views;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using Salamandra.Engine.Comparer;

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

        public BaseTrack? SelectedTrack { get; set; }
        public SongTagsInfo? SelectedTrackTags { get; set; }
        public BitmapImage? SelectedTrackArt { get; set; }

        public DispatcherTimer MainTimer { get; set; }

        public string? WindowTitle { get; set; }

        public bool PlaylistLoading { get; set; }
        public string PlaylistInfoText { get; set; }

        public DirectoryAudioScanner DirectoryAudioScanner { get; set; }
        public ApplicationSettings ApplicationSettings { get; set; }
        public SettingsManager<ApplicationSettings> SettingsManager { get; set; }

        public ScheduleManager ScheduleManager { get; set; }
        public bool EnableEvents { get; set; }

        public string? CurrentTrackFilename { get; set; }
        public string? TrackDisplayName
        {
            get
            {
                if (this.PlaylistManager.CurrentTrack == null)
                    return null;

                if (String.IsNullOrWhiteSpace(this.CurrentTrackFilename))
                    return this.PlaylistManager.CurrentTrack.FriendlyName;
                else
                    return this.CurrentTrackFilename;
            }
        }

        public TimeSpan? RemainingTime { get; set; }
        public TimeSpan? EndingTimeOfDay { get; set; }
        public DateTime CurrentDateTime { get; set; }

        #region Commands Properties
        public ICommand? AddFilesToPlaylistCommand { get; set; }
        public ICommand? AddTimeAnnouncementTrackCommand { get; set; }
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
        public ICommand? AddRandomTrackCommand { get; set; }
        public ICommand? OpenSettingsCommand { get; set; }
        public ICommand? OpenEventListCommand { get; set; }
        public ICommand? PlayLateEventsCommand { get; set; }
        public ICommand? DiscardLateEventsCommand { get; set; }
        #endregion

        public Action? RemovePlaylistAdorner { get; set; }

        public MainViewModel()
        {
            this.SoundEngine = new SoundEngine();
            this.SoundEngine.SoundStopped += SoundEngine_SoundStopped;
            this.SoundEngine.SoundError += SoundEngine_SoundError;
            this.PlaylistManager = new PlaylistManager();
            this.PlaylistManager.PlaylistMode = PlaylistMode.Repeat;

            this.IsPlaying = false;
            this.PlaybackState = PlaylistState.Stopped;

            this.CurrentVolume = 1; // ToDo: Min e Max via SoundEngine

            this.isDraggingTrackPosition = false;
            this.TrackLengthInSeconds = 0;
            this.TrackPositionInSeconds = 0;

            this.MainTimer = new DispatcherTimer();
            this.MainTimer.Interval = TimeSpan.FromMilliseconds(250);
            this.MainTimer.Tick += MainTimer_Tick;

            this.PlaylistLoading = false;
            this.PlaylistInfoText = string.Empty;

            this.SettingsManager = new SettingsManager<ApplicationSettings>("application_settings.json");
            this.ApplicationSettings = new ApplicationSettings();

            this.ScheduleManager = new ScheduleManager();
            this.EnableEvents = true;

            this.DirectoryAudioScanner = new DirectoryAudioScanner();

            UpdateWindowTitle();
            LoadCommands();

            this.SoundEngine.EnumerateDevices();
        }

        private void MainTimer_Tick(object? sender, EventArgs e)
        {
            this.CurrentDateTime = DateTime.Now;

            this.ScheduleManager.UpdateQueuedEventsList();
            (this.PlayLateEventsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (this.DiscardLateEventsCommand as RelayCommand)?.RaiseCanExecuteChanged();

            if (this.EnableEvents && this.ScheduleManager.HasLateImmediateEvent)
            {
                if (this.IsPlaying)
                {
                    this.PlaybackState = PlaylistState.JumpToNextEvent;
                    this.SoundEngine.Stop();
                }
                else
                    this.StartPlayback(true);
            }

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

        public async Task Loading()
        {
            LoadSettingsFile();
            ApplySettings();
            await ApplyStartupSettings();
            LoadEventsFile();
            LoadLibraryFile();

            this.MainTimer.Start();

            if (this.ApplicationSettings.PlayerSettings.PlayOnStartup)
                StartPlayback();
        }

        private void LoadSettingsFile()
        {
            try
            {
                var settings = this.SettingsManager.LoadSettings();

                if (settings != null)
                    this.ApplicationSettings = settings;

                var devices = this.SoundEngine.EnumerateDevices();

                if (devices.FirstOrDefault(x => x.DeviceIndex == this.ApplicationSettings.DeviceSettings.MainOutputDevice) == null)
                    this.ApplicationSettings.DeviceSettings.MainOutputDevice = devices.First().DeviceIndex;
            }
            catch (Exception ex)
            {
                // ToDo: Log ou mensagem!
            }
        }

        private void ApplySettings()
        {
            this.SoundEngine.OutputDevice = this.ApplicationSettings.DeviceSettings.MainOutputDevice;
        }

        private async Task ApplyStartupSettings()
        {
            this.CurrentVolume = this.ApplicationSettings.PlayerSettings.Volume;
            this.PlaylistManager.PlaylistMode = this.ApplicationSettings.PlayerSettings.PlaylistMode;
            this.EnableEvents = this.ApplicationSettings.PlayerSettings.EnableEvents;

            if (this.ApplicationSettings.PlayerSettings.OpenLastPlaylistOnStartup &&
                !String.IsNullOrWhiteSpace(this.ApplicationSettings.PlayerSettings.LastPlaylist) &&
                File.Exists(this.ApplicationSettings.PlayerSettings.LastPlaylist))
            {
                try
                {
                    await this.PlaylistManager.LoadPlaylist(this.ApplicationSettings.PlayerSettings.LastPlaylist);

                    if (this.ApplicationSettings.PlayerSettings.ShufflePlaylistOnStartup)
                    {
                        this.PlaylistManager.ShufflePlaylist();
                        this.PlaylistManager.UpdateNextTrack();
                    }
                }
                catch (Exception ex)
                {
                    // ToDo: Log/Notification!
                }
            }
        }

        private void LoadEventsFile()
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(this.ApplicationSettings.ScheduledEventSettings.ScheduledEventFilename))
                    this.ScheduleManager.LoadFromFile(this.ApplicationSettings.ScheduledEventSettings.ScheduledEventFilename);
            }
            catch (Exception ex)
            {
                this.ScheduleManager.CleanEvents();
                // ToDo: Log ou mensagem!
            }
        }

        private void LoadLibraryFile()
        {
            string filename = Path.Combine(Environment.CurrentDirectory, "directory_library.json");

            if (File.Exists(filename))
                this.DirectoryAudioScanner.LoadFromFile(filename);

            this.DirectoryAudioScanner.ScanLibrary();
        }

        public bool Closing()
        {
            if (this.IsPlaying && this.ApplicationSettings.PlayerSettings.AskToCloseWhenPlaying)
            {
                if (MessageBox.Show("A playlist ainda está tocando. Tem certeza que deseja fechar?", "Salamandra", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return false;
            }

            if (!CheckPlaylistModified())
                return false;

            this.DirectoryAudioScanner.StopScanning();
            this.StopPlayback();

            SaveSettings();
            SaveLibrary();

            return true;
        }

        private void SaveSettings()
        {
            this.ApplicationSettings.PlayerSettings.PlaylistMode = this.PlaylistManager.PlaylistMode;
            this.ApplicationSettings.PlayerSettings.Volume = this.CurrentVolume;
            this.ApplicationSettings.PlayerSettings.LastPlaylist = this.PlaylistManager.Filename;

            this.SettingsManager.SaveSettings(this.ApplicationSettings);
        }

        private void SaveLibrary()
        {
            string filename = Path.Combine(Environment.CurrentDirectory, "directory_library.json");

            this.DirectoryAudioScanner.SaveToFile(filename);
        }

        private void LoadCommands()
        {
            this.AddFilesToPlaylistCommand = new RelayCommandAsync(p => AddFilesToPlaylist(), p => HandlePlaylistException(p), p => !this.PlaylistLoading);
            this.AddTimeAnnouncementTrackCommand = new RelayCommand(p => AddTimeAnnouncementTrack(), p => !this.PlaylistLoading);
            this.AddRandomTrackCommand = new RelayCommand(p => AddRandomTrack(), p => !this.PlaylistLoading);
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

            this.ShufflePlaylistCommand = new RelayCommand(p => this.PlaylistManager.ShufflePlaylist(true), p => !this.PlaylistLoading);

            this.OpenSettingsCommand = new RelayCommand(p => OpenSettings());
            this.OpenEventListCommand = new RelayCommand(p => OpenEventList());

            this.PlayLateEventsCommand = new RelayCommand(p => PlayLateEvents(), p => this.ScheduleManager.HasLateEvent && this.EnableEvents);
            this.DiscardLateEventsCommand = new RelayCommand(p => DiscardLateEvents(), p => this.ScheduleManager.HasLateEvent);
        }

        private async Task AddFilesToPlaylist()
        {
            this.PlaylistLoading = true;
            this.PlaylistInfoText = "Adicionando arquivos a playlist...";

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Arquivos de áudio (*.wav, *.mp3, *.wma, *.ogg, *.flac) | *.wav; *.mp3; *.wma; *.ogg; *.flac";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
                await this.PlaylistManager.AddFiles(openFileDialog.FileNames.ToList(),
                    this.PlaylistManager.Tracks.IndexOf(this.SelectedTrack!));

            this.PlaylistLoading = false;
            this.PlaylistInfoText = string.Empty;
        }

        private void RemoveTracksFromPlaylist(object? items)
        {
            if (items == null || !(items is System.Collections.IList))
                return;

            List<BaseTrack> tracks = ((System.Collections.IList)items).Cast<BaseTrack>().ToList();

            this.PlaylistManager.RemoveTracks(tracks);
        }

        private void StartPlayback(bool startWithEvent = false)
        {
            if (!startWithEvent && this.PlaylistManager.NextTrack == null)
                return;

            if (startWithEvent && !this.EnableEvents)
                return;

            this.IsPlaying = true;

            var track = this.PlaylistManager.NextTrack;

            if (startWithEvent)
            {
                var upcomingEvent = this.ScheduleManager.DequeueLateEvent();

                if (upcomingEvent != null)
                    track = upcomingEvent.Track;
            }

            PlayTrack(track!, !startWithEvent);
        }

        private void PlayAudioFile(string filename)
        {
            try
            {
                this.SoundEngine.PlayAudioFile(filename, this.CurrentVolume);

                this.PlaybackState = PlaylistState.PlayingPlaylistTrack; // ToDo: Refatorar quando for evento!
                this.TrackLengthInSeconds = this.SoundEngine.TotalLengthInSeconds;
                this.TrackPositionInSeconds = 0;
                this.CalculateEndingTimeOfDay(false);

                this.AllowSeekDrag = true;
            }
            catch (SoundEngineFileException)
            {
                this.PlaybackState = PlaylistState.WaitingNextTrack;
            }
            catch (SoundEngineDeviceException ex)
            {
                this.StopPlaybackWithError(ex);
            }
            catch (Exception ex)
            {
                this.StopPlaybackWithError(ex);
            }
        }

        private void PlayTrack(BaseTrack track, bool updateNextTrack = true, bool resetSequence = true)
        {
            this.IsPaused = false;

            this.CurrentTrackFilename = null;
            this.PlaylistManager.CurrentTrack = track;

            if (updateNextTrack)
                this.PlaylistManager.UpdateNextTrack();

            switch (track)
            {
                case AudioFileTrack audioFileTrack:
                    PlayAudioFile(audioFileTrack.Filename!);

                    if (this.PlaybackState == PlaylistState.PlayingPlaylistTrack)
                    {
                        if (audioFileTrack.Duration == null)
                            audioFileTrack.Duration = TimeSpan.FromSeconds(this.SoundEngine.TotalLengthInSeconds);
                    }
                    break;
                case RotationTrack rotationTrack:
                    if (rotationTrack is SequentialTrack sequentialTrack && resetSequence)
                    {
                        if (sequentialTrack is TimeAnnouncementTrack timeAnnouncementTrack)
                            timeAnnouncementTrack.AudioFilesDirectory = @"C:\Users\Matheus\Desktop\Pacote ZaraRadio\Pacote ZaraRadio\Horas\(ZaraRadio) Horas masculino sem efeito";

                        sequentialTrack.ResetSequence();
                    }

                    string? file = rotationTrack.GetFile();
                    this.CurrentTrackFilename = Path.GetFileNameWithoutExtension(file);

                    if (!String.IsNullOrEmpty(file))
                        PlayAudioFile(file);
                    else
                    {
                        // Try to avoid looping an invalid track.
                        // ToDo: Actually, I don't know if this is really necessary.
                        if (this.PlaylistManager.NextTrack == track)
                            this.PlaylistManager.UpdateNextTrack();

                        this.PlaybackState = PlaylistState.WaitingNextTrack;
                    }
                    break;
                case RandomFileTrack randomTrack:
                    this.DirectoryAudioScanner.EnqueueAndScan(randomTrack.Filename!);

                    randomTrack.Filenames = this.DirectoryAudioScanner.GetFilesFromDirectory(randomTrack.Filename!.EnsureHasDirectorySeparatorChar());
                    string? randomFile = randomTrack.GetFile();
                    this.CurrentTrackFilename = Path.GetFileNameWithoutExtension(randomFile);

                    if (!String.IsNullOrEmpty(randomFile))
                        PlayAudioFile(randomFile);
                    else
                        this.PlaybackState = PlaylistState.WaitingNextTrack;
                    break;
                default:
                    throw new NotImplementedException();
            }

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrackDisplayName)));
        }

        private void PlayNextTrackOrStop()
        {
            if (this.EnableEvents && this.PlaybackState == PlaylistState.JumpToNextEvent)
            {
                PlayTrack(this.ScheduleManager.DequeueLateEvent()!.Track!, false);
                return;
            }

            if (!(this.PlaybackState == PlaylistState.JumpToNextTrack))
            {
                if (this.PlaylistManager.CurrentTrack != null && !this.PlaylistManager.CurrentTrack.HasTrackFinished)
                {
                    PlayTrack(this.PlaylistManager.CurrentTrack, false, false);
                    return;
                }
            }

            if (this.PlaylistManager.NextTrack != null)
            {
                if (this.EnableEvents && !(this.PlaybackState == PlaylistState.JumpToNextTrack) && this.ScheduleManager.HasLateEvent)
                {
                    PlayTrack(this.ScheduleManager.DequeueLateEvent()!.Track!, false);
                    return;
                }

                PlayTrack(this.PlaylistManager.NextTrack);
            }
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
            this.CurrentTrackFilename = null;
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
            {
                this.PlaybackState = PlaylistState.JumpToNextTrack;
                this.SoundEngine.Stop();
            }
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
            {
                this.PlaybackState = PlaylistState.JumpToNextTrack;
                this.SoundEngine.Stop();
            }
        }

        private void StopAfterCurrent()
        {
            if (this.IsPlaying)
                this.PlaylistManager.NextTrack = null;
        }

        private async Task OpenPlaylist()
        {
            if (!CheckPlaylistModified())
                return;

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
                    MessageBox.Show(String.Format("Houve um erro ao processar a playlist.\n\n{0}", ex.Message),
                        "Salamandra", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (IOException ex)
                {
                    MessageBox.Show(String.Format("Houve um erro de acesso ao arquivo.\n\n{0}", ex.Message),
                        "Salamandra", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format("Houve um erro ao abrir a playlist.\n\n{0}", ex.Message),
                        "Salamandra", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                UpdateWindowTitle();

                this.PlaylistLoading = false;
                this.PlaylistInfoText = string.Empty;

                this.RemovePlaylistAdorner?.Invoke();
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

            try
            {
                this.PlaylistManager.SavePlaylist(filename);
            }
            catch (IOException ex)
            {
                MessageBox.Show(String.Format("Houve um erro de acesso ao arquivo.\n\n{0}", ex.Message),
                    "Salamandra", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Houve um erro ao salvar o arquivo.\n\n{0}", ex.Message),
                    "Salamandra", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            UpdateWindowTitle();
        }

        private void NewPlaylist()
        {
            if (CheckPlaylistModified())
            {
                this.PlaylistManager.ClearPlaylist();

                this.RemovePlaylistAdorner?.Invoke();
            }
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

        private void AddTimeAnnouncementTrack()
        {
            this.PlaylistManager.AddTimeAnnouncementTrack(this.PlaylistManager.Tracks.IndexOf(this.SelectedTrack!));
        }

        private void AddRandomTrack()
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog vistaFolderBrowserDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            if (vistaFolderBrowserDialog.ShowDialog() == true)
            {
                this.DirectoryAudioScanner.EnqueueAndScan(vistaFolderBrowserDialog.SelectedPath);
                this.PlaylistManager.AddRandomTrack(vistaFolderBrowserDialog.SelectedPath,
                    this.PlaylistManager.Tracks.IndexOf(this.SelectedTrack!));
            }
        }

        private void OpenSettings()
        {
            // ToDo: WindowService?
            SettingsViewModel settingsViewModel = new SettingsViewModel(this.ApplicationSettings, this.SoundEngine);

            SettingsWindow settingsWindow = new SettingsWindow(settingsViewModel);
            settingsWindow.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            if (settingsWindow.ShowDialog() == true)
            {
                this.ApplicationSettings = settingsViewModel.Settings;
                ApplySettings();
            }
        }

        private void OpenEventList()
        {
            EventListViewModel eventListViewModel = new EventListViewModel(this.ScheduleManager.Events, this.ApplicationSettings.ScheduledEventSettings.ScheduledEventFilename);

            EventListWindow eventListWindow = new EventListWindow(eventListViewModel);
            eventListWindow.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            if (eventListWindow.ShowDialog() == true)
            {
                this.ApplicationSettings.ScheduledEventSettings.ScheduledEventFilename = eventListViewModel.Filename;
                this.ScheduleManager.SwapEvents(eventListViewModel.Events, eventListViewModel.HasFileChanged);

                PersistEvents();
                ScanScheduledRandomFiles();
            }
        }

        private void PersistEvents()
        {
            if (String.IsNullOrWhiteSpace(this.ApplicationSettings.ScheduledEventSettings.ScheduledEventFilename))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Playlist de Eventos (*.sche) | *.sche";

                if (!saveFileDialog.ShowDialog() == true)
                    this.ApplicationSettings.ScheduledEventSettings.ScheduledEventFilename = saveFileDialog.FileName;
                else
                    return;
            }

            try
            {
                this.ScheduleManager.SaveToFile(this.ApplicationSettings.ScheduledEventSettings.ScheduledEventFilename);
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Houve um erro ao salvar a planilha de eventos.\n\n{0}", ex.Message),
                    "Salamandra", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ScanScheduledRandomFiles()
        {
            var directoriesEvents = this.ScheduleManager.Events.Where(x => x.TrackScheduleType == TrackScheduleType.RandomFileTrack);

            if (directoriesEvents.Count() == 0)
                return;

            foreach (var item in directoriesEvents)
                this.DirectoryAudioScanner.Enqueue(item.Filename);

            this.DirectoryAudioScanner.StartScanning();
        }

        private void PlayLateEvents()
        {
            if (this.IsPlaying)
            {
                this.PlaybackState = PlaylistState.JumpToNextEvent;
                this.SoundEngine.Stop();
            }
            else
            {
                this.StartPlayback(true);
            }
        }

        private void DiscardLateEvents()
        {
            this.ScheduleManager.DiscardLateEvents();
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

        public void UpdateSelectedTrackTags()
        {
            if (this.SelectedTrack == null)
            {
                this.SelectedTrackTags = null;
                this.SelectedTrackArt = null;
                return;
            }

            if (SelectedTrack is AudioFileTrack audioFileTrack)
            {
                this.SelectedTrackTags = this.PlaylistManager.GetAudioFileTags(audioFileTrack.Filename!);

                if (this.SelectedTrackTags!.CoverArt != null)
                {
                    try
                    {
                        ConvertCoverArtToBitmapImage();
                    }
                    catch (Exception ex)
                    {
                        this.SelectedTrackArt = null;
                    }
                }
                else
                {
                    this.SelectedTrackArt = null;
                }
            }
        }

        private void ConvertCoverArtToBitmapImage()
        {
            using (var stream = new MemoryStream(this.SelectedTrackTags!.CoverArt))
            {
                if (stream != null && stream.Length > 4096)
                {
                    stream.Seek(0, SeekOrigin.Begin);

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    this.SelectedTrackArt = bitmap;
                }
            }
        }

        public bool CanSortPlaylist()
        {
            return !this.PlaylistLoading && !(this.PlaylistManager.Tracks.Count == 0);
        }

        public void SortPlaylist(PlaylistSortCriteria sortBy, SortDirection sortDirection)
        {
            this.PlaylistManager.Sort(sortBy, sortDirection);
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }
}
