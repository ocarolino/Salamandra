using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Tracks
{
    public abstract class SingleFileTrack : BaseTrack
    {
        public string Filename { get; set; }

        public SingleFileTrack() : base()
        {
            this.Filename = String.Empty;
        }
    }
}
