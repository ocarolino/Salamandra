using Salamandra.Engine.Domain.Events;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Comparer
{
    public enum SortDirection
    {
        Ascending,
        Descending,
    }

    public class ScheduledEventTimeComparer : System.Collections.IComparer
    {
        public SortDirection SortDirection { get; set; }

        public ScheduledEventTimeComparer(SortDirection sortDirection)
        {
            this.SortDirection = sortDirection;
        }

        public int Compare(object? x, object? y)
        {
            var objX = (ScheduledEvent)x!;
            var objY = (ScheduledEvent)y!;

            int compare = objX.UsePlayingHours.CompareTo(objY.UsePlayingHours);

            if (compare == 0)
            {
                if (objX.UsePlayingHours)
                {
                    TimeSpan timeX = new TimeSpan(0, objX.StartingDateTime.TimeOfDay.Minutes, objX.StartingDateTime.TimeOfDay.Seconds);
                    TimeSpan timeY = new TimeSpan(0, objY.StartingDateTime.TimeOfDay.Minutes, objY.StartingDateTime.TimeOfDay.Seconds);

                    compare = timeX.CompareTo(timeY);
                }
                else
                    compare = objX.StartingDateTime.TimeOfDay.CompareTo(objY.StartingDateTime.TimeOfDay);
            }

            return compare * (SortDirection == SortDirection.Descending ? -1 : 1);
        }
    }
}
