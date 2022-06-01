using Salamandra.Engine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Settings
{
    public class PlayerSettings : INotifyPropertyChanged
    {
        public float Volume { get; set; }
        public float PreListenVolume { get; set; }
        public PlaylistMode PlaylistMode { get; set; }
        public bool AskToCloseWhenPlaying { get; set; }
        public bool EnableEvents { get; set; }
        public bool OpenLastPlaylistOnStartup { get; set; }
        public string? LastPlaylist { get; set; }
        public bool ShufflePlaylistOnStartup { get; set; }
        public bool PlayOnStartup { get; set; }
        public bool KeepDeleteModeIfActiveOnStartup { get; set; }
        public bool LastDeleteModeState { get; set; }

        public PlayerSettings()
        {
            this.Volume = 1;
            this.PreListenVolume = 1;

            this.PlaylistMode = PlaylistMode.Default;

            this.AskToCloseWhenPlaying = true;

            this.EnableEvents = true;

            this.LastPlaylist = String.Empty;
            this.ShufflePlaylistOnStartup = false;
            this.PlayOnStartup = false;

            this.KeepDeleteModeIfActiveOnStartup = false;
            this.LastDeleteModeState = false;
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }
}
