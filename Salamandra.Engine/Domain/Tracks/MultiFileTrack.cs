using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Tracks
{
    public abstract class MultiFileTrack : SingleFileTrack
    {
        public List<string> Filenames { get; set; }

        public MultiFileTrack()
        {
            this.Filenames = new List<string>();
        }

        public abstract string? GetFile();
    }
}
