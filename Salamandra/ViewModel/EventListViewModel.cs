using Newtonsoft.Json;
using Salamandra.Commands;
using Salamandra.Engine.Domain.Events;
using Salamandra.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Salamandra.ViewModel
{
    public class EventListViewModel : INotifyPropertyChanged
    {
        private List<ScheduledEvent> originalScheduledEvents;
        public ObservableCollection<ScheduledEvent> Events { get; set; }
        public int LastEventId { get; set; }

        public ICommand NewEventCommand { get; set; }

        public EventListViewModel(List<ScheduledEvent> events)
        {
            this.originalScheduledEvents = new List<ScheduledEvent>(events);
            this.Events = new ObservableCollection<ScheduledEvent>();

            if (events.Count > 0)
                this.LastEventId = events.Last().Id;

            this.NewEventCommand = new RelayCommand(p => NewEvent());
        }

        public void Loading()
        {
            // Todo: Extension method?
            var serialized = JsonConvert.SerializeObject(this.originalScheduledEvents);
            var events = JsonConvert.DeserializeObject<List<ScheduledEvent>>(serialized);

            this.Events = new ObservableCollection<ScheduledEvent>(events);
        }

        private void NewEvent()
        {
            EventViewModel eventViewModel = new EventViewModel();

            EventWindow eventWindow = new EventWindow(eventViewModel);
            eventWindow.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            if (eventWindow.ShowDialog() == true)
            {

            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
