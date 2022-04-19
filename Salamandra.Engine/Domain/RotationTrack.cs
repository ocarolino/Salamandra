using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain
{
    public class RotationTrack : BaseTrack
    {
        public List<string> Files { get; set; }
        public int CurrentFile { get; set; }

        public override bool HasTrackFinished => true;

        public RotationTrack()
        {
            this.Files = new List<string>();
            this.CurrentFile = 0;
        }

        public string? GetCurrentFile()
        {
            if (this.Files.Count == 0)
                return null;

            string file = this.Files[this.CurrentFile];

            this.CurrentFile++;

            if (this.CurrentFile >= this.Files.Count)
                this.CurrentFile = 0;

            return file;
        }
    }
}
