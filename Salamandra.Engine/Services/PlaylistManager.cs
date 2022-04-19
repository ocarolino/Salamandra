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

namespace Salamandra.Engine.Services
{
    public class PlaylistManager : INotifyPropertyChanged
    {
        public PlaylistMode PlaylistMode { get; set; }
        public ObservableCollection<SoundFileTrack> Tracks { get; set; }

        public SoundFileTrack? CurrentTrack { get; set; }
        public SoundFileTrack? NextTrack { get; set; }

        public bool Modified { get; set; }
        public string Filename { get; set; }

        public PlaylistManager()
        {
            this.PlaylistMode = PlaylistMode.Default;

            this.Tracks = new ObservableCollection<SoundFileTrack>();

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
        public void AddTracks(List<SoundFileTrack> tracks)
        {
            foreach (var item in tracks)
                this.Tracks.Add(item);

            if (this.NextTrack == null)
                UpdateNextTrack();

            this.Modified = true;
        }

        public async Task AddFiles(List<string> filenames)
        {
            List<SoundFileTrack> tracks = new List<SoundFileTrack>();

            foreach (var item in filenames)
            {
                SoundFileTrack soundFileTrack = new SoundFileTrack() { Filename = item, FriendlyName = Path.GetFileNameWithoutExtension(item) };

                var duration = await Task.Run(() => GetAudioFileDuration(item));
                soundFileTrack.Duration = duration;

                tracks.Add(soundFileTrack);
            }

            AddTracks(tracks);
        }

        public void RemoveTracks(List<SoundFileTrack> tracks)
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
        #endregion
        public void ClearPlaylist()
        {
            this.Tracks = new ObservableCollection<SoundFileTrack>();
            this.NextTrack = null;

            this.Filename = string.Empty;
            this.Modified = false;
        }

        public async Task LoadPlaylist(string filename)
        {
            M3UPlaylistLoader playlistLoader = new M3UPlaylistLoader();

            List<PlaylistEntryInfo> entries = playlistLoader.Load(filename);
            List<SoundFileTrack> tracks = new List<SoundFileTrack>();

            foreach (var item in entries)
            {
                var track = new SoundFileTrack() { Filename = item.Filename, FriendlyName = item.FriendlyName, Duration = item.Duration };

                if (String.IsNullOrEmpty(track.FriendlyName))
                    track.FriendlyName = Path.GetFileNameWithoutExtension(track.Filename);

                if (track.Duration == null)
                {
                    var duration = await Task.Run(() => GetAudioFileDuration(track.Filename!));
                    track.Duration = duration;
                }

                tracks.Add(track);
            }

            this.Tracks = new ObservableCollection<SoundFileTrack>(tracks);

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
                PlaylistEntryInfo entry = new PlaylistEntryInfo() { Filename = item.Filename, FriendlyName = item.FriendlyName, Duration = item.Duration };
                entries.Add(entry);
            }

            playlistLoader.Save(filename, entries);

            this.Filename = filename;
            this.Modified = false;
        }

        public void ShufflePlaylist()
        {
            if (this.Tracks.Count == 0)
                return;

            Random random = new Random();

            int n = this.Tracks.Count;

            for (int i = 0; i < n - 1; i++)
            {
                int j = i + random.Next(n - i);

                SoundFileTrack track = this.Tracks[j];

                this.Tracks[j] = this.Tracks[i];
                this.Tracks[i] = track;
            }
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67

    }
}
