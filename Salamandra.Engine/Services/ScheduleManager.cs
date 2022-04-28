using Newtonsoft.Json;
using Salamandra.Engine.Domain.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Services
{
    public class ScheduleManager
    {
        public List<ScheduledEvent> Events { get; set; }

        public ScheduleManager()
        {
            this.Events = new List<ScheduledEvent>();
        }

        public void SwapEvents(ObservableCollection<ScheduledEvent> events)
        {
            // ToDo: Filename argument!
            this.Events = new List<ScheduledEvent>(events);
        }

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

        public void CleanEvents()
        {
            this.Events = new List<ScheduledEvent>();
        }
    }
}
