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
        public DateTime StartDateTime { get; set; }
        public bool Immediate { get; set; }
        public int QueueOrder { get; set; }
        public BaseTrack? Track { get; set; }
    }
}
