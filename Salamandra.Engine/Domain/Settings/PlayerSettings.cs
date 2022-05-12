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
        public PlaylistMode PlaylistMode { get; set; }
        public bool AskToCloseWhenPlaying { get; set; }

        public PlayerSettings()
        {
            this.Volume = 1;
            this.PlaylistMode = PlaylistMode.Default;
            this.AskToCloseWhenPlaying = true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
