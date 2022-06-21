﻿using Microsoft.Win32;
using Newtonsoft.Json;
using Salamandra.Commands;
using Salamandra.Engine.Domain.Events;
using Salamandra.Engine.Domain.Settings;
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
        public ApplicationSettings ApplicationSettings { get; set; }

        private List<ScheduledEvent> originalScheduledEvents;
        public ObservableCollection<ScheduledEvent> Events { get; set; }
        public int LastEventId { get; set; }

        public string Filename { get; set; }
        public bool HasFileChanged { get; set; }

        public ScheduledEvent? SelectedScheduledEvent { get; set; }

        public ICommand CreateEventCommand { get; set; }
        public ICommand EditEventCommand { get; set; }
        public ICommand DeleteEventsCommand { get; set; }
        public ICommand OpenEventListCommand { get; set; }
        public ICommand SaveEventListAsCommand { get; set; }

        public ICommand CopyEventsCommand { get; set; }
        public ICommand PasteEventsCommand { get; set; }

        public EventListViewModel(List<ScheduledEvent> events, ApplicationSettings applicationSettings)
        {
            this.ApplicationSettings = applicationSettings;

            this.originalScheduledEvents = new List<ScheduledEvent>(events);

            this.Events = new ObservableCollection<ScheduledEvent>();

            this.Filename = this.ApplicationSettings.ScheduledEventSettings.ScheduledEventFilename;
            this.HasFileChanged = false;

            if (events.Count > 0)
                this.LastEventId = events.Last().Id;

            this.CreateEventCommand = new RelayCommand(p => CreateEvent());
            this.EditEventCommand = new RelayCommand(p => EditEvent(), p => this.SelectedScheduledEvent != null);
            this.DeleteEventsCommand = new RelayCommand(p => DeleteEvents(p), p => this.SelectedScheduledEvent != null);
            this.OpenEventListCommand = new RelayCommand(p => OpenEventList());
            this.SaveEventListAsCommand = new RelayCommand(p => SaveEventListAs());

            this.CopyEventsCommand = new RelayCommand(p => CopyEvents(p), p => this.SelectedScheduledEvent != null);
            this.PasteEventsCommand = new RelayCommand(p => PasteEvents());
        }

        public void Loading()
        {
            // Todo: Extension method?
            var serialized = JsonConvert.SerializeObject(this.originalScheduledEvents);
            var events = JsonConvert.DeserializeObject<List<ScheduledEvent>>(serialized);

            if (events != null)
                this.Events = new ObservableCollection<ScheduledEvent>(events);
        }

        private void CreateEvent()
        {
            EventViewModel eventViewModel = new EventViewModel(this.ApplicationSettings);

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

            EventViewModel eventViewModel = new EventViewModel(this.SelectedScheduledEvent, this.ApplicationSettings);

            EventWindow eventWindow = new EventWindow(eventViewModel);
            eventWindow.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            if (eventWindow.ShowDialog() == true)
            {
                int index = this.Events.IndexOf(this.SelectedScheduledEvent);

                this.Events[index] = eventViewModel.ScheduledEvent;
                this.SelectedScheduledEvent = this.Events[index];
            }
        }

        private void DeleteEvents(object? items)
        {
            if (items == null || !(items is System.Collections.IList))
                return;

            List<ScheduledEvent> events = ((System.Collections.IList)items).Cast<ScheduledEvent>().ToList();

            if (MessageBox.Show("Tem certeza que deseja excluir os eventos selecionados?",
                "Eventos", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                foreach (var item in events)
                    this.Events.Remove(item);
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

        // ToDo: Refactor all of this serialization to the JsonEventsLoader class, separating between files and strings.
        private void CopyEvents(object? items)
        {
            if (items == null || !(items is System.Collections.IList))
                return;

            List<ScheduledEvent> events = ((System.Collections.IList)items).Cast<ScheduledEvent>().ToList();

            try
            {
                string json = JsonConvert.SerializeObject(events);

                Clipboard.SetText(json);
            }
            catch (Exception)
            {

            }
        }

        private void PasteEvents()
        {
            var text = Clipboard.GetText();

            if (String.IsNullOrWhiteSpace(text))
                return;

            try
            {
                var list = JsonConvert.DeserializeObject<List<ScheduledEvent>>(text);

                if (list == null || list.Count == 0)
                    return;

                foreach (var item in list)
                {
                    // Todo: Create a get id method?
                    this.LastEventId++;
                    item.Id = this.LastEventId;

                    this.Events.Add(item); // ToDo: AddRange.
                }
            }
            catch (Exception ex)
            {
            }
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }
}
