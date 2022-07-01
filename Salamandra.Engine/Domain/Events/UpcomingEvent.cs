using Salamandra.Engine.Domain.Enums;
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
        public TimeSpan? MaximumWaitTime { get; set; }
        public MaximumWaitAction MaximumWaitAction { get; set; }
        public EventPriority EventPriority { get; set; }
        public BaseTrack? Track { get; set; }
    }
}
