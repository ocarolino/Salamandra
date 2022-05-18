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

namespace Salamandra.Engine.Services
{
    public class PlaylistManager : INotifyPropertyChanged
    {
        public PlaylistMode PlaylistMode { get; set; }
        public ObservableCollection<BaseTrack> Tracks { get; set; }

        public BaseTrack? CurrentTrack { get; set; }
        public BaseTrack? NextTrack { get; set; }

        public bool Modified { get; set; }
        public string Filename { get; set; }

        public PlaylistManager()
        {
            this.PlaylistMode = PlaylistMode.Default;

            this.Tracks = new ObservableCollection<BaseTrack>();

            this.CurrentTrack = null;
            this.NextTrack = null;

            this.Modified = false;
            this.Filename = string.Empty;
        }

        public void UpdateNextTrack()
        {
            if (this.Tracks.Count == 0)
            {
                this.NextTrack = null;
                return;
            }

            int nextTrackIndex = this.Tracks.IndexOf(this.CurrentTrack!) + 1;

            switch (this.PlaylistMode)
            {
                case PlaylistMode.Default:
                case PlaylistMode.Repeat:
                    if (nextTrackIndex >= this.Tracks.Count)
                    {
                        if (this.PlaylistMode == PlaylistMode.Default)
                            this.NextTrack = null;
                        else
                            this.NextTrack = this.Tracks.First();
                    }
                    else
                        this.NextTrack = this.Tracks[nextTrackIndex];
                    break;
                case PlaylistMode.Random:
                    Random random = new Random(); // ToDo: Random singleton.

                    this.NextTrack = this.Tracks[random.Next(0, this.Tracks.Count)];
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        #region Add and Remove Tracks
        public void AddTracks(List<BaseTrack> tracks, int index = -1)
        {
            if (index >= this.Tracks.Count)
                index = -1;
            else if (index <= 0)
                index = 0;

            foreach (var item in tracks)
            {
                if (index != -1)
                {
                    this.Tracks.Insert(index, item);
                    index++;
                }
                else
                    this.Tracks.Add(item);
            }

            if (this.NextTrack == null)
                UpdateNextTrack();

            this.Modified = true;
        }

        public async Task AddFiles(List<string> filenames, int index = -1)
        {
            List<AudioFileTrack> tracks = new List<AudioFileTrack>();

            foreach (var item in filenames)
            {
                AudioFileTrack soundFileTrack = new AudioFileTrack() { Filename = item, FriendlyName = Path.GetFileNameWithoutExtension(item) };

                var duration = await Task.Run(() => GetAudioFileDuration(item));
                soundFileTrack.Duration = duration;

                tracks.Add(soundFileTrack);
            }

            AddTracks(tracks.Cast<BaseTrack>().ToList(), index);
        }

        public void AddTimeAnnouncementTrack(int index = -1)
        {
            TimeAnnouncementTrack timeAnnouncementTrack = new TimeAnnouncementTrack();

            AddTracks(new List<BaseTrack>() { timeAnnouncementTrack }, index);
        }

        public void AddRandomTrack(string directoryPath, int index = -1)
        {
            RandomFileTrack randomTrack = new RandomFileTrack() { Filename = directoryPath.EnsureHasDirectorySeparatorChar() };
            randomTrack.FriendlyName = Path.GetFileName(randomTrack.Filename.TrimEnd(Path.DirectorySeparatorChar));

            AddTracks(new List<BaseTrack>() { randomTrack }, index);
        }


        public void RemoveTracks(List<BaseTrack> tracks)
        {
            foreach (var item in tracks)
                this.Tracks.Remove(item);

            if (!this.Tracks.Contains(this.NextTrack!))
                UpdateNextTrack();

            this.Modified = true;
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
            this.Tracks = new ObservableCollection<BaseTrack>();
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

                if (item.Filename!.EndsWith(".time"))
                    track = new TimeAnnouncementTrack();
                else if (item.Filename.EndsWith(".dir"))
                {
                    string dir = item.Filename.Substring(0, item.Filename.Length - 4);
                    track = new RandomFileTrack() { Filename = dir.EnsureHasDirectorySeparatorChar(), FriendlyName = Path.GetFileName(dir) };
                }
                else
                {
                    track = new AudioFileTrack() { Filename = item.Filename, FriendlyName = item.FriendlyName, Duration = item.Duration };

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

            this.Tracks = new ObservableCollection<BaseTrack>(tracks);

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
                        entries.Add(new PlaylistEntryInfo() { Filename = randomFileTrack.Filename!.TrimEnd(Path.DirectorySeparatorChar) + ".dir", FriendlyName = randomFileTrack.FriendlyName, Duration = randomFileTrack.Duration });
                        break;
                    case SingleFileTrack singleFileTrack:
                        entries.Add(new PlaylistEntryInfo() { Filename = singleFileTrack.Filename, FriendlyName = singleFileTrack.FriendlyName, Duration = singleFileTrack.Duration });
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
                        this.Tracks = new ObservableCollection<BaseTrack>(this.Tracks.OrderBy(x => x.FriendlyName));
                    else
                        this.Tracks = new ObservableCollection<BaseTrack>(this.Tracks.OrderByDescending(x => x.FriendlyName));
                    break;
                case PlaylistSortCriteria.Duration:
                    if (sortDirection == SortDirection.Ascending)
                        this.Tracks = new ObservableCollection<BaseTrack>(this.Tracks.OrderBy(x => x.Duration));
                    else
                        this.Tracks = new ObservableCollection<BaseTrack>(this.Tracks.OrderByDescending(x => x.Duration));
                    break;
                default:
                    throw new NotImplementedException();
                    break;
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
