using Salamandra.Engine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Events
{
    public class ScheduledEvent
    {
        public int Id { get; set; }
        public bool Immediate { get; set; }
        public TimeScheduleType TimeScheduleType { get; set; }
        public DateTime StartingDateTime { get; set; }
        public ObservableCollection<int> PlayingHours { get; set; }
        public DateTime ExpirationDateTime { get; set; }
        public ObservableCollection<DayOfWeek> DaysOfWeek { get; set; }
        public TrackScheduleType TrackScheduleType { get; set; }
        public string Filename { get; set; }

        public ScheduledEvent()
        {
            this.PlayingHours = new ObservableCollection<int>();
            this.DaysOfWeek = new ObservableCollection<DayOfWeek>();
            this.Filename = String.Empty;
        }
    }
}
