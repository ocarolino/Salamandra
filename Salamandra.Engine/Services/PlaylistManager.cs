using Salamandra.Engine.Domain;
using Salamandra.Engine.Domain.Enums;
using Salamandra.Engine.Exceptions;
using Salamandra.Engine.Services.Playlists;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Salamandra.Engine.Domain.Tracks;
using Salamandra.Engine.Extensions;
using System.Drawing;
using Salamandra.Engine.Comparer;
using System.Windows.Data;

namespace Salamandra.Engine.Services
{
    public class PlaylistManager : INotifyPropertyChanged
    {
        public PlaylistMode PlaylistMode { get; set; }
        public WpfObservableRangeCollection<BaseTrack> Tracks { get; set; }
        public List<BaseTrack> RandomBlacklist { get; set; }

        public BaseTrack? LastTrack { get; set; }
        public BaseTrack? CurrentTrack { get; set; }
        public BaseTrack? NextTrack { get; set; }

        public bool Modified { get; set; }
        public string Filename { get; set; }

        public static string[] SupportedPlaylistFormats = { ".m3u" };

        public PlaylistManager()
        {
            this.PlaylistMode = PlaylistMode.Default;

            this.Tracks = new WpfObservableRangeCollection<BaseTrack>();
            this.RandomBlacklist = new List<BaseTrack>();

            this.CurrentTrack = null;
            this.NextTrack = null;

            this.Modified = false;
            this.Filename = string.Empty;
        }

        public void UpdateNextTrack(bool ignoreManualMode = false)
        {
            if (this.Tracks.Count == 0)
            {
                this.NextTrack = null;
                return;
            }

            int nextTrackIndex = 0;

            if (this.CurrentTrack != null)
                nextTrackIndex = this.Tracks.IndexOf(this.CurrentTrack) + 1;

            var mode = this.PlaylistMode;

            if (ignoreManualMode && mode == PlaylistMode.Manual)
                mode = PlaylistMode.Repeat;

            switch (mode)
            {
                case PlaylistMode.Default:
                case PlaylistMode.Repeat:
                case PlaylistMode.Shuffle:
                    if (nextTrackIndex >= this.Tracks.Count)
                    {
                        if (this.PlaylistMode == PlaylistMode.Default)
                        {
                            this.NextTrack = null;
                            return;
                        }

                        if (this.PlaylistMode == PlaylistMode.Shuffle)
                            ShufflePlaylist();

                        this.NextTrack = this.Tracks.First();
                    }
                    else
                        this.NextTrack = this.Tracks[nextTrackIndex];
                    break;
                case PlaylistMode.Random:
                    this.NextTrack = this.Tracks.GetRandomWithBlacklist(this.RandomBlacklist);
                    break;
                case PlaylistMode.Manual:
                    this.NextTrack = null;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void AddToBlacklist(BaseTrack track)
        {
            if (this.Tracks.Contains(track))
                this.RandomBlacklist.Add(track);
        }

        public void SetAsLastTrack(BaseTrack track)
        {
            if (this.Tracks.Contains(track))
                this.LastTrack = track;
        }

        #region Add and Remove Tracks
        public void AddTracks(List<BaseTrack> tracks, int index = -1)
        {
            int oldTrackCount = this.Tracks.Count;

            if (index >= this.Tracks.Count)
                index = -1;

            if (index != -1)
                this.Tracks.InsertRange(index, tracks);
            else
                this.Tracks.AddRange(tracks);

            if (this.NextTrack == null || oldTrackCount == 1)
                UpdateNextTrack();

            this.Modified = true;
        }

        public async Task AddFiles(List<string> filenames, int index = -1)
        {
            List<AudioFileTrack> tracks = new List<AudioFileTrack>();

            foreach (var item in filenames)
            {
                AudioFileTrack soundFileTrack = await CreateAudioFileTrackFromFilename(item);

                tracks.Add(soundFileTrack);
            }

            AddTracks(tracks.Cast<BaseTrack>().ToList(), index);
        }

        private async Task<AudioFileTrack> CreateAudioFileTrackFromFilename(string item)
        {
            AudioFileTrack soundFileTrack = new AudioFileTrack() { Filename = item, FriendlyName = Path.GetFileNameWithoutExtension(item) };

            var duration = await Task.Run(() => GetAudioFileDuration(item));
            soundFileTrack.Duration = duration;

            return soundFileTrack;
        }

        public async Task AddTracksFromPaths(List<string> paths, int index = -1)
        {
            List<BaseTrack> tracks = new List<BaseTrack>();

            foreach (var item in paths)
            {
                FileAttributes attr = File.GetAttributes(item);

                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    tracks.Add(CreateRandomTrackFromPath(item));
                else
                {
                    if (SoundEngine.SupportedAudioFormats.Any(x => item.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
                        tracks.Add(await CreateAudioFileTrackFromFilename(item));
                    else if (PlaylistManager.SupportedPlaylistFormats.Any(x => item.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
                        tracks.Add(CreatePlaylistTrackFromFilename(item));
                }
            }

            if (tracks.Count > 0)
                AddTracks(tracks, index);
        }

        public void AddTimeAnnouncementTrack(int index = -1)
        {
            TimeAnnouncementTrack timeAnnouncementTrack = new TimeAnnouncementTrack();

            AddTracks(new List<BaseTrack>() { timeAnnouncementTrack }, index);
        }

        public void AddRandomTrack(string directoryPath, int index = -1)
        {
            RandomFileTrack randomTrack = CreateRandomTrackFromPath(directoryPath);

            AddTracks(new List<BaseTrack>() { randomTrack }, index);
        }

        public void AddStopTrack(int index = -1)
        {
            PlayerCommandTrack playerCommandTrack = new PlayerCommandTrack(PlayerCommandType.Stop);

            AddTracks(new List<BaseTrack>() { playerCommandTrack }, index);
        }

        public void AddPlaylistTrack(string filename, int index = -1)
        {
            AddTracks(new List<BaseTrack>() { CreatePlaylistTrackFromFilename(filename) }, index);
        }

        private RandomFileTrack CreateRandomTrackFromPath(string directoryPath)
        {
            RandomFileTrack randomTrack = new RandomFileTrack() { Filename = directoryPath.EnsureHasDirectorySeparatorChar() };
            randomTrack.FriendlyName = Path.GetFileName(randomTrack.Filename.TrimEnd(Path.DirectorySeparatorChar));
            return randomTrack;
        }

        private PlaylistFileTrack CreatePlaylistTrackFromFilename(string filename)
        {
            PlaylistFileTrack playlistFileTrack = new PlaylistFileTrack()
            {
                Filename = filename,
                FriendlyName = Path.GetFileNameWithoutExtension(filename)
            };

            return playlistFileTrack;
        }

        public void RemoveTracks(List<BaseTrack> tracks, bool modified = true)
        {
            this.Tracks.RemoveRange(tracks);

            if (!this.Tracks.Contains(this.NextTrack!))
                UpdateNextTrack();

            this.Modified = modified;
        }

        public TimeSpan? GetAudioFileDuration(string filename)
        {
            try
            {
                TagLib.File file = TagLib.File.Create(filename);
                return file.Properties.Duration;
            }
            catch
            {
                return null;
            }
        }

        public SongTagsInfo? GetAudioFileTags(string filename)
        {
            try
            {
                TagLib.File file = TagLib.File.Create(filename);

                SongTagsInfo songTagsInfo = new SongTagsInfo();
                songTagsInfo.Artist = file.Tag.FirstPerformer;
                songTagsInfo.Title = file.Tag.Title;
                songTagsInfo.Album = file.Tag.Album;
                songTagsInfo.Year = (int?)file.Tag.Year;
                songTagsInfo.Genre = file.Tag.FirstGenre;
                songTagsInfo.Comment = file.Tag.Comment;

                if (file.Tag.Pictures.Length > 0)
                    songTagsInfo.CoverArt = file.Tag.Pictures[0].Data.Data;

                return songTagsInfo;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        public void ClearPlaylist()
        {
            this.Tracks = new WpfObservableRangeCollection<BaseTrack>();
            this.NextTrack = null;

            this.Filename = string.Empty;
            this.Modified = false;
        }

        public async Task LoadPlaylist(string filename)
        {
            M3UPlaylistLoader playlistLoader = new M3UPlaylistLoader();

            List<PlaylistEntryInfo> entries = playlistLoader.Load(filename);
            List<BaseTrack> tracks = new List<BaseTrack>();

            foreach (var item in entries)
            {
                BaseTrack? track = null;

                if (item.Filename!.EndsWith(".time", StringComparison.OrdinalIgnoreCase))
                    track = new TimeAnnouncementTrack();
                else if (item.Filename.EndsWith(".stop", StringComparison.OrdinalIgnoreCase))
                    track = new PlayerCommandTrack(PlayerCommandType.Stop);
                else if (item.Filename.EndsWith(".dir", StringComparison.OrdinalIgnoreCase))
                {
                    string dir = item.Filename.Substring(0, item.Filename.Length - 4);

                    track = new RandomFileTrack()
                    {
                        Filename = dir.EnsureHasDirectorySeparatorChar(),
                        FriendlyName = Path.GetFileName(dir.TrimEnd('\\'))
                    };
                }
                else if (PlaylistManager.SupportedPlaylistFormats.Any(x => item.Filename.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
                {
                    track = new PlaylistFileTrack()
                    {
                        Filename = item.Filename.NullToEmpty(),
                        FriendlyName = item.FriendlyName.NullToEmpty(),
                    };
                }
                else
                {
                    track = new AudioFileTrack()
                    {
                        Filename = item.Filename.NullToEmpty(),
                        FriendlyName = item.FriendlyName.NullToEmpty(),
                        Duration = item.Duration
                    };

                    if (String.IsNullOrEmpty(track.FriendlyName))
                        track.FriendlyName = Path.GetFileNameWithoutExtension(item.Filename);

                    if (track.Duration == null)
                    {
                        var duration = await Task.Run(() => GetAudioFileDuration(item.Filename!));
                        track.Duration = duration;
                    }
                }

                tracks.Add(track);
            }

            this.Tracks = new WpfObservableRangeCollection<BaseTrack>(tracks);

            this.UpdateNextTrack();

            this.Filename = filename;
            this.Modified = false;
        }

        public void SavePlaylist(string filename)
        {
            M3UPlaylistLoader playlistLoader = new M3UPlaylistLoader();

            List<PlaylistEntryInfo> entries = new List<PlaylistEntryInfo>();

            foreach (var item in this.Tracks)
            {
                switch (item)
                {
                    case RandomFileTrack randomFileTrack:
                        entries.Add(new PlaylistEntryInfo()
                        {
                            Filename = randomFileTrack.Filename.TrimEnd(Path.DirectorySeparatorChar) + ".dir",
                            FriendlyName = randomFileTrack.FriendlyName,
                            Duration = randomFileTrack.Duration
                        });
                        break;
                    case SingleFileTrack singleFileTrack:
                        entries.Add(new PlaylistEntryInfo()
                        {
                            Filename = singleFileTrack.Filename,
                            FriendlyName = singleFileTrack.FriendlyName,
                            Duration = singleFileTrack.Duration
                        });
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            playlistLoader.Save(filename, entries);

            this.Filename = filename;
            this.Modified = false;
        }

        public void ShufflePlaylist(bool setAsModified = false)
        {
            if (this.Tracks.Count == 0)
                return;

            Random random = new Random();

            int n = this.Tracks.Count;

            for (int i = 0; i < n - 1; i++)
            {
                int j = i + random.Next(n - i);

                BaseTrack track = this.Tracks[j];

                this.Tracks[j] = this.Tracks[i];
                this.Tracks[i] = track;
            }

            if (setAsModified)
                this.Modified = true;
        }

        public void Sort(PlaylistSortCriteria sortBy, SortDirection sortDirection)
        {
            if (this.Tracks.Count == 0)
                return;

            switch (sortBy)
            {
                case PlaylistSortCriteria.FriendlyName:
                    if (sortDirection == SortDirection.Ascending)
                        this.Tracks = new WpfObservableRangeCollection<BaseTrack>(this.Tracks.OrderBy(x => x.FriendlyName));
                    else
                        this.Tracks = new WpfObservableRangeCollection<BaseTrack>(this.Tracks.OrderByDescending(x => x.FriendlyName));
                    break;
                case PlaylistSortCriteria.Duration:
                    if (sortDirection == SortDirection.Ascending)
                        this.Tracks = new WpfObservableRangeCollection<BaseTrack>(this.Tracks.OrderBy(x => x.Duration));
                    else
                        this.Tracks = new WpfObservableRangeCollection<BaseTrack>(this.Tracks.OrderByDescending(x => x.Duration));
                    break;
                default:
                    throw new NotImplementedException();
            }

            this.Modified = true;
        }

        public void SwapTracks(BaseTrack sourceItem, BaseTrack targetItem)
        {
            int i = this.Tracks.IndexOf(sourceItem);
            int j = this.Tracks.IndexOf(targetItem);

            if (i != -1 && j != -1 && i != j)
            {
                this.Tracks[i] = targetItem;
                this.Tracks[j] = sourceItem;
            }

            this.Modified = true;
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }
}
