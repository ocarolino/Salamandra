using Salamandra.Engine.Domain.Enums;
using Salamandra.Engine.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Extensions
{
    public static class ScheduledEventExtensions
    {
        public static bool IsFriendlyNameLocalizable(this ScheduledEvent input)
        {
            switch (input.TrackScheduleType)
            {
                case TrackScheduleType.TimeAnnouncementTrack:
                case TrackScheduleType.StartPlaylistTrack:
                case TrackScheduleType.StopPlaylistTrack:
                    return true;
            }

            return false;
        }
    }
}
