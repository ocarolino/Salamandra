using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Events
{
    public class UpcomingEvent
    {
        public int EventId { get; set; }
        public DateTime StartTime { get; set; }
        public bool Imediate { get; set; }
        public BaseTrack? Track { get; set; }
    }
}
