using Salamandra.Engine.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Services
{
    public class PlaylistManager
    {
        public ObservableCollection<SoundFileTrack> Tracks { get; set; }

        public SoundFileTrack CurrentTrack { get; set; }
        public SoundFileTrack NextTrack { get; set; }

        public PlaylistManager()
        {
            this.Tracks = new ObservableCollection<SoundFileTrack>();

            this.CurrentTrack = null;
            this.NextTrack = null;
        }

        public void UpdateNextTrack()
        {

        }
    }
}
