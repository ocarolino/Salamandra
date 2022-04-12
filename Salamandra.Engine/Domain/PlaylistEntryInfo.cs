using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain
{
    public class PlaylistEntryInfo
    {
        public string? Filename { get; set; }
        public string? FriendlyName { get; set; }
        public TimeSpan? Duration { get; set; }
    }
}
