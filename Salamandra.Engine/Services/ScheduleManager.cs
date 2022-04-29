using Newtonsoft.Json;
using Salamandra.Engine.Domain.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Services
{
    public class ScheduleManager
    {
        public List<ScheduledEvent> Events { get; set; }
        public ObservableCollection<UpcomingEvent> EventsQueue { get; set; }
        public int LastHourChecked { get; set; }

        public ScheduleManager()
        {
            this.Events = new List<ScheduledEvent>();
            this.EventsQueue = new ObservableCollection<UpcomingEvent>();

            this.LastHourChecked = -1;
        }

        public void SwapEvents(ObservableCollection<ScheduledEvent> events)
        {
            // ToDo: Filename argument!
            this.Events = new List<ScheduledEvent>(events);
        }

        #region Saving, Loading and Reseting a List
        // ToDo: Criar um Save e Load genéricos depois!
        public void SaveToFile(string filename)
        {
            string json = JsonConvert.SerializeObject(this.Events);
            File.WriteAllText(filename, json);
        }

        public void LoadFromFile(string filename)
        {
            var list = JsonConvert.DeserializeObject<List<ScheduledEvent>>(File.ReadAllText(filename));

            if (list != null)
                this.Events = list;
            else
                this.Events = new List<ScheduledEvent>();
        }
        #endregion

        public void CleanEvents()
        {
            this.Events = new List<ScheduledEvent>();
        }

        public bool CheckEventSchedule(ScheduledEvent scheduledEvent, DateTime dateToTest)
        {
            if (scheduledEvent.StartingDateTime.Date <= dateToTest.Date)
                return false;

            if (scheduledEvent.UseDaysOfWeek)
            {
                if (!scheduledEvent.DaysOfWeek.Contains(dateToTest.DayOfWeek))
                    return false;
            }
            else
            {
                if (scheduledEvent.StartingDateTime.Date != dateToTest.Date)
                    return false;
            }

            if (scheduledEvent.UsePlayingHours)
            {
                if (!scheduledEvent.PlayingHours.Contains(dateToTest.Hour))
                    return false;
            }
            else
            {
                if (scheduledEvent.StartingDateTime.Hour != dateToTest.Hour)
                    return false;
            }

            DateTime runningDate = new DateTime(dateToTest.Year, dateToTest.Month, dateToTest.Day,
                dateToTest.Hour, scheduledEvent.StartingDateTime.Minute, scheduledEvent.StartingDateTime.Second);

            if (runningDate < dateToTest || runningDate > scheduledEvent.ExpirationDateTime)
                return false;

            return true;
        }

        public void UpdateQueuedEventsList()
        {
            if (DateTime.Now.Hour != this.LastHourChecked)
            {
                CreateUpcomingEvents(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                    DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second));
                this.LastHourChecked = DateTime.Now.Hour;
            }
        }

        private void CreateUpcomingEvents(DateTime startFromDate)
        {
            foreach (var item in this.Events)
            {
                if (CheckEventSchedule(item, startFromDate))
                    Debug.WriteLine(String.Format("Queueing event: {0}", item.FriendlyName));
            }
        }
    }
}
