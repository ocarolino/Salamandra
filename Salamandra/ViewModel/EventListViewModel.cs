using Microsoft.Win32;
using Newtonsoft.Json;
using Salamandra.Commands;
using Salamandra.Engine.Domain.Events;
using Salamandra.Engine.Services.Playlists;
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
        public ICommand OpenEventListCommand { get; set; }
        public ICommand SaveEventListAsCommand { get; set; }

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
            this.OpenEventListCommand = new RelayCommand(p => OpenEventList());
            this.SaveEventListAsCommand = new RelayCommand(p => SaveEventListAs());
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

        private void OpenEventList()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Lista de Eventos (*.sche) | *.sche";

            if (openFileDialog.ShowDialog() == true)
            {
                JsonEventsLoader jsonEventsLoader = new JsonEventsLoader();

                try
                {
                    var list = jsonEventsLoader.Load(openFileDialog.FileName);
                    this.Events = new ObservableCollection<ScheduledEvent>(list);

                    this.Filename = openFileDialog.FileName;

                    if (this.Events.Count > 0)
                        this.LastEventId = this.Events.Last().Id;

                    // ToDo: Don't really care if the user opened the same file. Maybe it can be changed in the future.
                    this.HasFileChanged = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Houve um erro ao abrir a lista.\n\n" + ex.Message, "Salamandra", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveEventListAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Lista de Eventos (*.sche) | *.sche";

            if (saveFileDialog.ShowDialog() == true)
            {
                JsonEventsLoader jsonEventsLoader = new JsonEventsLoader();

                try
                {
                    jsonEventsLoader.Save(saveFileDialog.FileName, this.Events.ToList());
                    this.Filename = saveFileDialog.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Houve um erro ao salvar a lista.\n\n" + ex.Message, "Salamandra", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
