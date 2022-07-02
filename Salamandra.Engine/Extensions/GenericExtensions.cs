using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Extensions
{
    // Source: https://www.wwt.com/article/how-to-clone-objects-in-dotnet-core (2022-07-01)
    public static class GenericExtensions
    {
        public static T DeepCopy<T>(this T self)
        {
            var serialized = JsonConvert.SerializeObject(self);

#pragma warning disable CS8603 // Possible null reference return.
            return JsonConvert.DeserializeObject<T>(serialized);
#pragma warning restore CS8603 // Possible null reference return.
        }

        public static T GetRandomWithBlacklist<T>(this ICollection<T> self, ICollection<T> blacklist)
        {
            Random random = new Random();

            var availableItems = self.Except(blacklist).ToList();

            if (availableItems.Count == 0)
            {
                availableItems.AddRange(self);
                blacklist.Clear();
            }

            return availableItems[random.Next(availableItems.Count)];
        }
    }
}
