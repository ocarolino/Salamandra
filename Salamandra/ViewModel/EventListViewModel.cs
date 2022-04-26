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
    public class EventListViewModel : INotifyPropertyChanged
    {
        private List<ScheduledEvent> originalScheduledEvents;
        public ObservableCollection<ScheduledEvent> Events { get; set; }
        public int LastEventId { get; set; }

        public EventListViewModel(List<ScheduledEvent> events)
        {
            this.originalScheduledEvents = new List<ScheduledEvent>(events);
            this.Events = new ObservableCollection<ScheduledEvent>();

            if (events.Count > 0)
                this.LastEventId = events.Last().Id;
        }

        public void Loading()
        {
            // Todo: Extension method?
            var serialized = JsonConvert.SerializeObject(this.originalScheduledEvents);
            var events = JsonConvert.DeserializeObject<List<ScheduledEvent>>(serialized);

            this.Events = new ObservableCollection<ScheduledEvent>(events);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
