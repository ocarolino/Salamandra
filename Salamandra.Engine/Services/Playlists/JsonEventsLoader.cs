using Newtonsoft.Json;
using Salamandra.Engine.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Services.Playlists
{
    public class JsonEventsLoader
    {
        public List<ScheduledEvent> Load(string filename)
        {
            var list = JsonConvert.DeserializeObject<List<ScheduledEvent>>(File.ReadAllText(filename));

            if (list == null)
                return new List<ScheduledEvent>();

            CheckForDuplicateIds(list);

            return list;
        }

        private void CheckForDuplicateIds(List<ScheduledEvent> listToCheck)
        {
            var groups = listToCheck.GroupBy(x => x.Id)
                .Where(g => g.Count() > 1)
                .Select(y => y.Key)
                .ToList();

            if (groups.Count > 0)
            {
                for (int i = 0; i < listToCheck.Count; i++)
                    listToCheck[i].Id = i + 1;
            }
        }

        public void Save(string filename, List<ScheduledEvent> entries)
        {
            string json = JsonConvert.SerializeObject(entries);
            File.WriteAllText(filename, json);
        }
    }
}
