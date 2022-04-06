using Salamandra.Engine.Domain;
using Salamandra.Engine.Domain.Enums;
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

        public SoundFileTrack CurrentTrack { get; set; }
        public SoundFileTrack NextTrack { get; set; }

        public PlaylistManager()
        {
            this.PlaylistMode = PlaylistMode.Default;

            this.Tracks = new ObservableCollection<SoundFileTrack>();

            this.CurrentTrack = null;
            this.NextTrack = null;
        }

        public void UpdateNextTrack()
        {
            if (this.Tracks.Count == 0)
            {
                this.NextTrack = null;
                return;
            }

            int nextTrackIndex = this.Tracks.IndexOf(this.CurrentTrack) + 1;

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

        public void AddTracks(List<SoundFileTrack> tracks)
        {
            foreach (var item in tracks)
                this.Tracks.Add(item);

            if (this.NextTrack == null)
                UpdateNextTrack();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void RemoveTracks(List<SoundFileTrack> tracks)
        {
            foreach (var item in tracks)
                this.Tracks.Remove(item);

            if (!this.Tracks.Contains(this.NextTrack))
                UpdateNextTrack();
        }
    }
}
