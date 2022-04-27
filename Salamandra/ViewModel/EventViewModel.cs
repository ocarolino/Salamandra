using Newtonsoft.Json;
using Salamandra.Engine.Domain.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.ViewModel
{
    public class EventViewModel : INotifyPropertyChanged
    {
        private ScheduledEvent? originalScheduledEvent;

        public ScheduledEvent ScheduledEvent { get; set; }

        public ObservableCollection<int> Hours { get; set; }
        public ObservableCollection<DayOfWeek> Days { get; set; }

        public EventViewModel()
        {
            this.Hours = new ObservableCollection<int>();
            this.Days = new ObservableCollection<DayOfWeek>();

            for (int i = 0; i < 24; i++)
                this.Hours.Add(i);

            foreach (DayOfWeek item in Enum.GetValues(typeof(DayOfWeek)))
                this.Days.Add(item);

            this.ScheduledEvent = new ScheduledEvent();
        }

        public EventViewModel(ScheduledEvent scheduledEvent) : this()
        {
            this.originalScheduledEvent = scheduledEvent;
        }

        public void Loading()
        {
            if (this.originalScheduledEvent != null)
            {
                var serialized = JsonConvert.SerializeObject(this.originalScheduledEvent);
                var scheduledEvent = JsonConvert.DeserializeObject<ScheduledEvent>(serialized);

                this.ScheduledEvent = scheduledEvent;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
