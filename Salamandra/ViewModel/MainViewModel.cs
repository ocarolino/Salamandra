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
using GongSolutions.Wpf.DragDrop;
using System.Collections.Specialized;
using Salamandra.Engine.Domain.Events;

namespace Salamandra.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged, IDropTarget
    {
        public SoundEngine SoundEngine { get; set; }
        public PlaylistManager PlaylistManager { get; set; }

        public PlaylistState PlaybackState { get; set; }
        public bool IsPlaying { get; set; }
        public bool IsPaused { get; set; }
        public bool StopAfterCurrentTrack { get; set; }
        private bool isChangingVolumeMutedProgramatically;
        public float CurrentVolume { get; set; }
        public float PreviousVolume { get; set; }
        public bool VolumeMuted { get; set; }
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
        public bool IsEventPlaying { get; set; }
        public EventPriority CurrentEventPriority { get; set; }

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

        public bool EnableLoopMode { get; set; }
        public bool EnableDeleteMode { get; set; }

        public LogManager? ApplicationLogManager { get; set; }
        public LogManager? PlayerLogManager { get; set; }

        #region Commands Properties
        public ICommand? AddFilesToPlaylistCommand { get; set; }
        public ICommand? AddPlaylistTrackCommand { get; set; }
        public ICommand? AddTimeAnnouncementTrackCommand { get; set; }
        public ICommand? AddStopTrackCommand { get; set; }
        public ICommand? RemoveTracksFromPlaylistCommand { get; set; }
        public ICommand? RemoveAllTracksCommand { get; set; }
        public ICommand? StartPlaybackCommand { get; set; }
        public ICommand? StopPlaybackCommand { get; set; }
        public ICommand? PlaySelectedTrackCommand { get; set; }
        public ICommand? OpenPreListenCommand { get; set; }
        public ICommand? SelectedAsNextTrackCommand { get; set; }
        public ICommand? VolumeControlValueChangedCommand { get; set; }
        public ICommand? VolumeMutedChangedCommand { get; set; }
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
        public ICommand? FocusOnCurrentTrackCommand { get; set; }
        public ICommand? FocusOnNextTrackCommand { get; set; }
        public ICommand? OpenLogFolderCommand { get; set; }
        #endregion

        public Action? RemovePlaylistAdorner { get; set; }
        public Action<BaseTrack?>? FocusOnTrack { get; set; }

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
            this.MainTimer.Interval = TimeSpan.FromMilliseconds(200);
            this.MainTimer.Tick += MainTimer_Tick;

            this.PlaylistLoading = false;
            this.PlaylistInfoText = string.Empty;

            this.SettingsManager = new SettingsManager<ApplicationSettings>("application_settings.json");
            this.ApplicationSettings = new ApplicationSettings();

            this.ScheduleManager = new ScheduleManager();
            this.EnableEvents = true;

            this.DirectoryAudioScanner = new DirectoryAudioScanner();

            RefreshWindowTitle();
            LoadCommands();

            this.SoundEngine.EnumerateDevices();
        }

        private void MainTimer_Tick(object? sender, EventArgs e)
        {
            this.CurrentDateTime = DateTime.Now;

            this.ScheduleManager.UpdateQueuedEventsList(this.IsEventPlaying, this.CurrentEventPriority);
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
            this.ApplicationLogManager = new LogManager(String.Empty, "Salamandra", false);
            this.ApplicationLogManager.InitializeLog();

            LoadSettingsFile();
            ApplyRunningSettings();
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

                this.ApplicationSettings.DeviceSettings.CheckDevices(devices);
            }
            catch (Exception ex)
            {
                this.ApplicationLogManager?.Fatal(String.Format("Error loading settings file. Resetting to default settings. ({0})", ex.Message),
                    "Settings");
            }
        }

        private void LoadPlayerLogger(LoggingSettings loggingSettings)
        {
            this.PlayerLogManager = null;

            if (loggingSettings.EnableLogging && !String.IsNullOrWhiteSpace(loggingSettings.LoggingOutputPath))
            {
                this.PlayerLogManager = new LogManager(
                    loggingSettings.LoggingOutputPath.EnsureHasDirectorySeparatorChar(),
                    "Salamandra Player Log - ",
                    true);
                this.PlayerLogManager.InitializeLog();
            }
        }

        private void ApplyRunningSettings()
        {
            this.SoundEngine.OutputDevice = this.ApplicationSettings.DeviceSettings.MainOutputDevice;

            LoadPlayerLogger(this.ApplicationSettings.LoggingSettings);
        }

        private async Task ApplyStartupSettings()
        {
            this.CurrentVolume = this.ApplicationSettings.PlayerSettings.Volume;
            this.PlaylistManager.PlaylistMode = this.ApplicationSettings.PlayerSettings.PlaylistMode;

            if (!this.ApplicationSettings.PlayerSettings.AlwaysEnableEventsOnStartup)
                this.EnableEvents = this.ApplicationSettings.PlayerSettings.EnableEvents;
            else
                this.EnableEvents = true;

            if (this.ApplicationSettings.PlayerSettings.OpenLastPlaylistOnStartup &&
                !String.IsNullOrWhiteSpace(this.ApplicationSettings.PlayerSettings.LastPlaylist) &&
                File.Exists(this.ApplicationSettings.PlayerSettings.LastPlaylist))
            {
                string filename = this.ApplicationSettings.PlayerSettings.LastPlaylist;

                try
                {
                    this.PlayerLogManager?.Information(filename, "Playlist");

                    await this.PlaylistManager.LoadPlaylist(filename);

                    if (this.ApplicationSettings.PlayerSettings.ShufflePlaylistOnStartup)
                    {
                        this.PlaylistManager.ShufflePlaylist();
                        this.PlaylistManager.UpdateNextTrack(true);
                    }
                }
                catch (Exception ex)
                {
                    this.PlayerLogManager?.Error(String.Format("{0} ({1})", ex.Message, filename), "Playlist");
                }

                // ToDo: Tornar genérico esse loadplaylist para sempre atualizar o título
                RefreshWindowTitle();
            }

            if (this.ApplicationSettings.PlayerSettings.KeepDeleteModeLastState &&
                this.ApplicationSettings.PlayerSettings.EnableDeleteMode)
                this.EnableDeleteMode = true;
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

                this.ApplicationLogManager?.Error(String.Format("There was an error when loading the scheduled events at {0}. ({1})",
                    this.ApplicationSettings.ScheduledEventSettings.ScheduledEventFilename, ex.Message), "Events");
            }
        }

        private void LoadLibraryFile()
        {
            string filename = Path.Combine(Environment.CurrentDirectory, "directory_library.json");

            if (File.Exists(filename))
            {
                try
                {
                    this.DirectoryAudioScanner.LoadFromFile(filename);
                }
                catch (Exception ex)
                {
                    this.ApplicationLogManager?.Error(String.Format("There was an error when loading the directory library at {0}. ({1})",
                        filename, ex.Message), "Library");
                }
            }
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
            this.MainTimer.Stop();

            SaveSettings();
            SaveLibrary();

            return true;
        }

        private void SaveSettings()
        {
            this.ApplicationSettings.PlayerSettings.PlaylistMode = this.PlaylistManager.PlaylistMode;
            this.ApplicationSettings.PlayerSettings.Volume = this.VolumeMuted ? this.PreviousVolume : this.CurrentVolume;
            this.ApplicationSettings.PlayerSettings.LastPlaylist = this.PlaylistManager.Filename;
            this.ApplicationSettings.PlayerSettings.EnableDeleteMode = this.EnableDeleteMode;

            this.ApplicationSettings.PlayerSettings.EnableEvents = this.EnableEvents;

            try
            {
                this.SettingsManager.SaveSettings(this.ApplicationSettings);
            }
            catch (Exception ex)
            {
                this.ApplicationLogManager?.Fatal(String.Format("Error saving settings file. ({0})", ex.Message), "Settings");
            }
        }

        private void SaveLibrary()
        {
            string filename = Path.Combine(Environment.CurrentDirectory, "directory_library.json");

            try
            {
                this.DirectoryAudioScanner.SaveToFile(filename);
            }
            catch (Exception ex)
            {
                this.ApplicationLogManager?.Error(String.Format("There was an error when saving the directory library at {0}. ({1})",
                    filename, ex.Message), "Library");
            }
        }

        private void LoadCommands()
        {
            this.AddFilesToPlaylistCommand = new RelayCommandAsync(p => AddFilesToPlaylist(), p => HandlePlaylistException(p), p => !this.PlaylistLoading);
            this.AddPlaylistTrackCommand = new RelayCommand(p => AddPlaylistTrack(), p => !this.PlaylistLoading);
            this.AddTimeAnnouncementTrackCommand = new RelayCommand(p => AddTimeAnnouncementTrack(), p => !this.PlaylistLoading);
            this.AddRandomTrackCommand = new RelayCommand(p => AddRandomTrack(), p => !this.PlaylistLoading);
            this.AddStopTrackCommand = new RelayCommand(p => AddStopTrack(), p => !this.PlaylistLoading);
            this.RemoveTracksFromPlaylistCommand = new RelayCommand(p => RemoveTracksFromPlaylist(p), p => !this.PlaylistLoading);
            this.RemoveAllTracksCommand = new RelayCommand(p => RemoveAllTracks(), p => !this.PlaylistLoading);

            this.StartPlaybackCommand = new RelayCommand(p => StartPlayback(false, true), p => !this.IsPlaying);
            this.StopPlaybackCommand = new RelayCommand(p => StopPlayback(true), p => this.IsPlaying);

            this.PlaySelectedTrackCommand = new RelayCommand(p => PlaySelectedTrack(), p => this.SelectedTrack != null);
            this.OpenPreListenCommand = new RelayCommand(p => OpenPreListen(), p => this.SelectedTrack != null && this.SelectedTrack is AudioFileTrack);
            this.SelectedAsNextTrackCommand = new RelayCommand(p => SetSelectedAsNextTrack(), p => this.SelectedTrack != null);

            this.VolumeControlValueChangedCommand = new RelayCommand(p => VolumeControlValueChanged(), p => true);
            this.VolumeMutedChangedCommand = new RelayCommand(p => VolumeMutedChanged(), p => true);

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

            this.FocusOnCurrentTrackCommand = new RelayCommand(p => FocusOnTrack?.Invoke(this.PlaylistManager.CurrentTrack));
            this.FocusOnNextTrackCommand = new RelayCommand(p => FocusOnTrack?.Invoke(this.PlaylistManager.NextTrack));

            this.OpenLogFolderCommand = new RelayCommand(p => OpenLogFolder());
        }

        private async Task AddFilesToPlaylist()
        {
            SetPlaylistLoading(true, "Adicionando arquivos a playlist...");

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Arquivos de áudio (*.wav, *.mp3, *.wma, *.ogg, *.flac) | *.wav; *.mp3; *.wma; *.ogg; *.flac";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
                await this.PlaylistManager.AddFiles(openFileDialog.FileNames.ToList(),
                    this.PlaylistManager.Tracks.IndexOf(this.SelectedTrack!));

            SetPlaylistLoading(false);
        }

        private void AddPlaylistTrack()
        {
            // ToDo: Alguma forma de tornar essas questões de formatos e filtros genéricos!
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Playlist M3U (*.m3u) | *.m3u";

            if (openFileDialog.ShowDialog() == true)
                this.PlaylistManager.AddPlaylistTrack(openFileDialog.FileName, this.PlaylistManager.Tracks.IndexOf(this.SelectedTrack!));
        }

        private void RemoveTracksFromPlaylist(object? items)
        {
            if (items == null || !(items is System.Collections.IList))
                return;

            List<BaseTrack> tracks = ((System.Collections.IList)items).Cast<BaseTrack>().ToList();

            this.PlaylistManager.RemoveTracks(tracks);
        }

        private void RemoveAllTracks()
        {
            this.PlaylistManager.RemoveTracks(this.PlaylistManager.Tracks.ToList());
        }

        private void StartPlayback(bool startWithEvent = false, bool manual = false)
        {
            if (!startWithEvent && this.PlaylistManager.NextTrack == null)
                return;

            if (startWithEvent && !this.EnableEvents)
                return;

            this.IsPlaying = true;
            this.IsEventPlaying = false;

            var track = this.PlaylistManager.NextTrack;


            if (manual)
                this.PlayerLogManager?.Information("Starting playback - Manual", "Player");
            else
                this.PlayerLogManager?.Information("Starting playback - Automatic", "Player");

            if (startWithEvent)
            {
                var upcomingEvent = this.ScheduleManager.DequeueLateEvent();

                if (upcomingEvent != null)
                {
                    PlayEvent(upcomingEvent);
                    return;
                }
            }

            PlayTrack(track!, !startWithEvent);
        }

        private void PlayAudioFile(string filename, bool logSuccess = true, string? friendlyName = null, bool onlyFriendlyName = false)
        {
            try
            {
                this.SoundEngine.PlayAudioFile(filename, this.CurrentVolume);

                this.PlaybackState = PlaylistState.PlayingTrack;
                this.TrackLengthInSeconds = this.SoundEngine.TotalLengthInSeconds;
                this.TrackPositionInSeconds = 0;
                this.CalculateEndingTimeOfDay(false);

                this.AllowSeekDrag = true;

                // ToDo: Maybe this is too much for this method. Should we just return a boolean in success and let the success log formatting for the PlayTrack?
                if (logSuccess)
                {
                    if (String.IsNullOrEmpty(friendlyName))
                        this.PlayerLogManager?.Information(filename, "Player");
                    else
                    {
                        if (!onlyFriendlyName)
                            this.PlayerLogManager?.Information(String.Format("{0} ({1})", friendlyName, filename), "Player");
                        else
                            this.PlayerLogManager?.Information(friendlyName, "Player");
                    }
                }
            }
            catch (SoundEngineFileException soundEngineFileException)
            {
                this.PlaybackState = PlaylistState.WaitingNextTrack;

                this.PlayerLogManager?.Error(String.Format("{0} ({1})", soundEngineFileException.Message, filename), "Player");
            }
            catch (SoundEngineDeviceException soundEngineDeviceException)
            {
                this.StopPlaybackWithError(soundEngineDeviceException);
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLowerInvariant().Contains("frame at sample rate"))
                    this.PlaybackState = PlaylistState.WaitingNextTrack;
                else
                    this.StopPlaybackWithError(ex);
            }
        }

        private void PlayTrack(BaseTrack track, bool updateNextTrack = true, bool resetSequence = true)
        {
            this.IsPaused = false;

            this.CurrentTrackFilename = null;
            this.PlaylistManager.CurrentTrack = track;

            if (updateNextTrack)
            {
                this.PlaylistManager.SetAsLastTrack(track);
                this.PlaylistManager.AddToBlacklist(track);
                this.PlaylistManager.UpdateNextTrack();
            }

            switch (track)
            {
                case AudioFileTrack audioFileTrack:
                    PlayAudioFile(audioFileTrack.Filename!);

                    if (this.PlaybackState == PlaylistState.PlayingTrack)
                    {
                        if (audioFileTrack.Duration == null)
                            audioFileTrack.Duration = TimeSpan.FromSeconds(this.SoundEngine.TotalLengthInSeconds);
                    }
                    break;
                case RotationTrack rotationTrack:
                    bool shouldLog = true;
                    bool onlyFriendlyName = false;

                    if (rotationTrack is SequentialTrack sequentialTrack && resetSequence)
                    {
                        if (sequentialTrack is TimeAnnouncementTrack timeAnnouncementTrack)
                        {
                            timeAnnouncementTrack.AudioFilesDirectory = this.ApplicationSettings.GeneralSettings.TimeAnnouncementFilesPath;
                            onlyFriendlyName = true;
                        }

                        sequentialTrack.ResetSequence();
                    }

                    string? file = rotationTrack.GetFile();
                    this.CurrentTrackFilename = Path.GetFileNameWithoutExtension(file);


                    if (rotationTrack is TimeAnnouncementTrack && !resetSequence)
                        shouldLog = false;

                    if (!String.IsNullOrEmpty(file))
                        PlayAudioFile(file, shouldLog, rotationTrack.FriendlyName, onlyFriendlyName);
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
                    this.DirectoryAudioScanner.EnqueueAndScan(randomTrack.Filename);

                    string? randomFile = this.DirectoryAudioScanner.GetRandomFileFromDirectory(randomTrack.Filename);
                    this.CurrentTrackFilename = Path.GetFileNameWithoutExtension(randomFile);

                    if (!String.IsNullOrEmpty(randomFile))
                        PlayAudioFile(randomFile, true, randomTrack.FriendlyName);
                    else
                        this.PlaybackState = PlaylistState.WaitingNextTrack;
                    break;
                case PlayerCommandTrack playerCommandTrack:
                    switch (playerCommandTrack.Command)
                    {
                        case PlayerCommandType.Play:
                            this.PlaybackState = PlaylistState.WaitingNextTrack;
                            break;
                        case PlayerCommandType.Stop:
                            StopPlayback();
                            break;
                        default:
                            break;
                    }
                    break;
                case PlaylistFileTrack playlistFileTrack:
                    SetPlaylistLoading(true, "Carregando playlist...");

                    this.PlayerLogManager?.Information(playlistFileTrack.Filename, "Playlist");

                    var task = this.PlaylistManager.LoadPlaylist(playlistFileTrack.Filename);

                    task.ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            this.PlayerLogManager?.Error(String.Format("{0} ({1})", t.Exception!.Message, playlistFileTrack.Filename), "Playlist");

                            ResetPlaylist();
                        }

                        // Maybe an event has started playing when loading a big playlist, so we check just to be safe.
                        if (this.PlaylistManager.CurrentTrack == playlistFileTrack)
                            this.PlaybackState = PlaylistState.WaitingNextTrack;

                        RefreshWindowTitle();
                        SetPlaylistLoading(false);
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                    break;
                case SystemProcessTrack systemProcessTrack:
                    this.PlayerLogManager?.Information(String.Format("Executing process ({0})", systemProcessTrack.Filename), "Playlist");

                    try
                    {
                        StartProcess(systemProcessTrack.Filename);
                    }
                    catch (Exception ex)
                    {
                        this.PlayerLogManager?.Error(String.Format("Error executing process {0} ({1})", systemProcessTrack.Filename, ex.Message), "Playlist");
                    }
                    this.PlaybackState = PlaylistState.WaitingNextTrack;
                    break;
                default:
                    throw new NotImplementedException();
            }

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrackDisplayName)));
        }

        private static void StartProcess(string filename)
        {
            ProcessStartInfo sinfo = new ProcessStartInfo();
            sinfo.UseShellExecute = true;
            sinfo.FileName = filename;

            Process.Start(sinfo);
        }

        private void PlayEvent(UpcomingEvent upcomingEvent)
        {
            this.IsEventPlaying = true;
            this.CurrentEventPriority = upcomingEvent.EventPriority;

            PlayTrack(upcomingEvent.Track!, false);
        }

        private void PlayNextTrackOrStop()
        {
            if (this.EnableDeleteMode)
                this.PlaylistManager.RemoveTracks(new List<BaseTrack>() { this.PlaylistManager.CurrentTrack! }, false);

            // Do we have to play an immediate event or the user asked to play late events?
            if (this.EnableEvents && this.PlaybackState == PlaylistState.JumpToNextEvent)
            {
                PlayEvent(this.ScheduleManager.DequeueLateEvent()!);
                return;
            }

            // Is the current track a sequential track?
            if (!(this.PlaybackState == PlaylistState.JumpToNextTrack))
            {
                if (this.PlaylistManager.CurrentTrack != null && !this.PlaylistManager.CurrentTrack.HasTrackFinished)
                {
                    PlayTrack(this.PlaylistManager.CurrentTrack, false, false);
                    return;
                }
            }

            // Has user asked to stop after current track?
            if (this.StopAfterCurrentTrack)
            {
                StopPlayback();
                return;
            }

            // Do we have a late queued event?
            if (this.EnableEvents && !(this.PlaybackState == PlaylistState.JumpToNextTrack) && this.ScheduleManager.HasLateEvent)
            {
                var upcomingEvent = this.ScheduleManager.DequeueLateEvent();

                if (upcomingEvent != null)
                {
                    PlayEvent(upcomingEvent);
                    return;
                }
            }

            // Is loop mode enabled?
            if (this.EnableLoopMode && this.PlaylistManager.LastTrack != null)
            {
                this.IsEventPlaying = false;

                PlayTrack(this.PlaylistManager.LastTrack, false);
                return;
            }

            // Do we have a next track?
            if (this.PlaylistManager.NextTrack != null)
            {
                this.IsEventPlaying = false;

                PlayTrack(this.PlaylistManager.NextTrack);
                return;
            }

            StopPlayback();
        }

        private void StopPlayback(bool manual = false)
        {
            this.IsPlaying = false;
            this.IsPaused = false;
            this.StopAfterCurrentTrack = false;
            this.PlaybackState = PlaylistState.Stopped;
            this.SoundEngine.Stop();

            if (this.PlaylistManager.NextTrack == null)
                this.PlaylistManager.UpdateNextTrack(true);

            this.PlaylistManager.CurrentTrack = null;

            this.CurrentTrackFilename = null;
            this.TrackLengthInSeconds = 0;
            this.TrackPositionInSeconds = 0;
            this.RemainingTime = null;
            this.EndingTimeOfDay = null;
            this.AllowSeekDrag = false;

            this.IsEventPlaying = false;

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TrackDisplayName)));

            if (manual)
                this.PlayerLogManager?.Information("Stopping playback - Manual", "Player");
            else
                this.PlayerLogManager?.Information("Stopping playback - Automatic", "Player");
        }

        private void StopPlaybackWithError(Exception ex)
        {
            this.PlayerLogManager?.Fatal(String.Format("{0} ({1})", ex.Message, this.PlaylistManager.CurrentTrack!.FriendlyName), "Device");

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
                NextTrack();
            else
                this.StartPlayback();
        }

        private void SetSelectedAsNextTrack()
        {
            if (this.SelectedTrack != null)
                this.PlaylistManager.NextTrack = this.SelectedTrack;
        }

        private void VolumeControlValueChanged(bool userChanged = true)
        {
            if (this.IsPlaying)
                this.SoundEngine.Volume = this.CurrentVolume;

            if (!userChanged)
                return;

            this.isChangingVolumeMutedProgramatically = true;

            if (this.CurrentVolume > 0)
                this.VolumeMuted = false;
            else
                this.VolumeMuted = true;

            this.isChangingVolumeMutedProgramatically = false;
        }

        private void VolumeMutedChanged()
        {
            if (this.isChangingVolumeMutedProgramatically == true)
                return;

            if (this.VolumeMuted)
            {
                this.PreviousVolume = this.CurrentVolume;
                this.CurrentVolume = 0;
            }
            else
            {
                this.CurrentVolume = this.PreviousVolume;

                if (this.CurrentVolume <= 0)
                    this.CurrentVolume = 1; // ToDo: Max e Min volume
            }

            this.VolumeControlValueChanged(false);
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
                this.IsEventPlaying = false;
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
                SetPlaylistLoading(true, "Carregando playlist...");

                try
                {
                    this.PlayerLogManager?.Information(openFileDialog.FileName, "Playlist");

                    await this.PlaylistManager.LoadPlaylist(openFileDialog.FileName);
                }
                catch (PlaylistLoaderException ex)
                {
                    MessageBox.Show(String.Format("Houve um erro ao processar a playlist.\n\n{0}", ex.Message),
                        "Salamandra", MessageBoxButton.OK, MessageBoxImage.Error);

                    this.PlayerLogManager?.Error(String.Format("{0} ({1})", ex.Message, openFileDialog.FileName), "Playlist");
                }
                catch (IOException ex)
                {
                    MessageBox.Show(String.Format("Houve um erro de acesso ao arquivo.\n\n{0}", ex.Message),
                        "Salamandra", MessageBoxButton.OK, MessageBoxImage.Error);

                    this.PlayerLogManager?.Error(String.Format("{0} ({1})", ex.Message, openFileDialog.FileName), "Playlist");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format("Houve um erro ao abrir a playlist.\n\n{0}", ex.Message),
                        "Salamandra", MessageBoxButton.OK, MessageBoxImage.Error);

                    this.PlayerLogManager?.Error(String.Format("{0} ({1})", ex.Message, openFileDialog.FileName), "Playlist");
                }

                if (this.PlaybackState == PlaylistState.Stopped && this.PlaylistManager.PlaylistMode == PlaylistMode.Manual)
                    this.PlaylistManager.UpdateNextTrack(true);

                RefreshWindowTitle();
                SetPlaylistLoading(false);

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

            RefreshWindowTitle();
        }

        private void NewPlaylist()
        {
            if (CheckPlaylistModified())
                ResetPlaylist();

            RefreshWindowTitle(); // ToDo: Criar um evento PlaylistChanged, que chamará sempre esse método.
        }

        private void ResetPlaylist()
        {
            this.PlaylistManager.ClearPlaylist();

            this.RemovePlaylistAdorner?.Invoke();
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

        private void RefreshWindowTitle()
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

        private void AddStopTrack()
        {
            this.PlaylistManager.AddStopTrack(this.PlaylistManager.Tracks.IndexOf(this.SelectedTrack!));
        }

        private void OpenSettings()
        {
            // ToDo: WindowService?
            SettingsViewModel settingsViewModel = new SettingsViewModel(this.ApplicationSettings, this.SoundEngine);

            SettingsWindow settingsWindow = new SettingsWindow(settingsViewModel);
            settingsWindow.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            if (settingsWindow.ShowDialog() == true)
            {
                this.ApplicationSettings = settingsViewModel.Settings!;
                ApplyRunningSettings();
            }
        }

        private void OpenEventList()
        {
            EventListViewModel eventListViewModel = new EventListViewModel(this.ScheduleManager.Events, this.ApplicationSettings);

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
                this.ApplicationLogManager?.Error(String.Format("There was an error when saving the scheduled events at {0}. ({1})",
                    this.ApplicationSettings.ScheduledEventSettings.ScheduledEventFilename,
                    ex.Message), "Events");

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
            {
                this.PlaybackState = PlaylistState.WaitingNextTrack;

                this.PlayerLogManager?.Fatal(e.SoundErrorException.Message, "Player");
            }
            else
            {
                this.StopPlaybackWithError(e.SoundErrorException!);
            }
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

                if (this.SelectedTrackTags == null || this.SelectedTrackTags.CoverArt == null)
                {
                    this.SelectedTrackArt = null;
                    return;
                }

                try
                {
                    ConvertCoverArtToBitmapImage();
                }
                catch (Exception)
                {
                    this.SelectedTrackArt = null;
                }
            }
        }

        private void ConvertCoverArtToBitmapImage()
        {
            if (this.SelectedTrackTags == null || this.SelectedTrackTags.CoverArt == null)
                return;

            using (var stream = new MemoryStream(this.SelectedTrackTags.CoverArt))
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

        public void OpenPreListen()
        {
            if (this.SelectedTrack == null || !(this.SelectedTrack is AudioFileTrack))
                return;

            var track = this.SelectedTrack as AudioFileTrack;

            PreListenViewModel preListenViewModel = new PreListenViewModel(this.ApplicationSettings, track!.Filename);

            PreListenWindow preListenWindow = new PreListenWindow(preListenViewModel);
            preListenWindow.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
            preListenWindow.ShowDialog();
        }

        #region DragDrop Handlers
        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;

            var dataObject = dropInfo.Data as IDataObject;

            if (dataObject != null && dataObject.GetDataPresent(DataFormats.FileDrop))
            {
                dropInfo.Effects = DragDropEffects.Copy;
            }
            else
            {
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            var dataObject = dropInfo.Data as DataObject;

            if (dataObject != null && dataObject.ContainsFileDropList())
            {
                // ToDo: Tornar esses textos genéricos.
                SetPlaylistLoading(true, "Adicionando arquivos a playlist...");

                var task = this.HandleDropActionAsync(dropInfo, dataObject.GetFileDropList());

                task.ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        this.PlayerLogManager?.Error(String.Format("Error when dropping files on player: {0}", t.Exception!.Message), "Player");
                    }

                    SetPlaylistLoading(false);
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);

                this.PlaylistManager.Modified = true;
            }
        }

        private async Task HandleDropActionAsync(IDropInfo dropInfo, StringCollection fileOrDirDropList)
        {
            await this.PlaylistManager.AddTracksFromPaths(fileOrDirDropList.Cast<string>().ToList(), dropInfo.InsertIndex);

            foreach (var item in this.PlaylistManager.Tracks.OfType<RandomFileTrack>())
                this.DirectoryAudioScanner.Enqueue(item.Filename!);

            this.DirectoryAudioScanner.ScanLibrary();
        }
        #endregion

        private void SetPlaylistLoading(bool loading, string message = "")
        {
            if (loading)
            {
                this.PlaylistLoading = true;
                this.PlaylistInfoText = message;
            }
            else
            {
                this.PlaylistLoading = false;
                this.PlaylistInfoText = String.Empty;
            }
        }

        private void OpenLogFolder()
        {
            if (!String.IsNullOrEmpty(this.ApplicationSettings.LoggingSettings.LoggingOutputPath))
            {
                try
                {
                    StartProcess(this.ApplicationSettings.LoggingSettings.LoggingOutputPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format("Houve um erro ao abrir a pasta de registros.\n\nErro: {0}", ex.Message),
                        "Salamandra", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }
}
