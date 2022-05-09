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

        public string Filename { get; set; }
        public bool HasFileChanged { get; set; }

        public ScheduledEvent? SelectedScheduledEvent { get; set; }

        public ICommand CreateEventCommand { get; set; }
        public ICommand EditEventCommand { get; set; }
        public ICommand DeleteEventCommand { get; set; }

        public EventListViewModel(List<ScheduledEvent> events, string filename)
        {
            this.originalScheduledEvents = new List<ScheduledEvent>(events);

            this.Events = new ObservableCollection<ScheduledEvent>();

            this.Filename = filename;
            this.HasFileChanged = false;

            if (events.Count > 0)
                this.LastEventId = events.Last().Id;

            this.CreateEventCommand = new RelayCommand(p => CreateEvent());
            this.EditEventCommand = new RelayCommand(p => EditEvent(), p => this.SelectedScheduledEvent != null);
            this.DeleteEventCommand = new RelayCommand(p => DeleteEvent(), p => this.SelectedScheduledEvent != null);
        }

        public void Loading()
        {
            // Todo: Extension method?
            var serialized = JsonConvert.SerializeObject(this.originalScheduledEvents);
            var events = JsonConvert.DeserializeObject<List<ScheduledEvent>>(serialized);

            this.Events = new ObservableCollection<ScheduledEvent>(events);
        }

        private void CreateEvent()
        {
            EventViewModel eventViewModel = new EventViewModel();

            EventWindow eventWindow = new EventWindow(eventViewModel);
            eventWindow.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            if (eventWindow.ShowDialog() == true)
            {
                this.LastEventId++;

                ScheduledEvent scheduledEvent = eventViewModel.ScheduledEvent;
                scheduledEvent.Id = this.LastEventId;

                this.Events.Add(scheduledEvent);
            }
        }

        private void EditEvent()
        {
            if (this.SelectedScheduledEvent == null)
                return;

            EventViewModel eventViewModel = new EventViewModel(this.SelectedScheduledEvent);

            EventWindow eventWindow = new EventWindow(eventViewModel);
            eventWindow.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            if (eventWindow.ShowDialog() == true)
            {
                int index = this.Events.IndexOf(this.SelectedScheduledEvent);

                this.Events[index] = eventViewModel.ScheduledEvent;
                this.SelectedScheduledEvent = this.Events[index];
            }
        }

        private void DeleteEvent()
        {
            if (this.SelectedScheduledEvent == null)
                return;

            if (MessageBox.Show(String.Format("Tem certeza que deseja excluir o evento {0}?", this.SelectedScheduledEvent.FriendlyName),
                "Eventos", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                this.Events.Remove(this.SelectedScheduledEvent);
                this.SelectedScheduledEvent = null;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
