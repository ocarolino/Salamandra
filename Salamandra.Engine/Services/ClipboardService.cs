using Newtonsoft.Json;
using Salamandra.Engine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Salamandra.Engine.Services
{
    public class ClipboardService<T> where T : class
    {
        private readonly ClipboardDataType clipboardData;
        public bool HasData { get => Clipboard.ContainsData(this.clipboardData.ToString()); }

        public ClipboardService(ClipboardDataType clipboardData)
        {
            this.clipboardData = clipboardData;
        }

        public bool Copy(List<T> items)
        {
            try
            {
                var json = JsonConvert.SerializeObject(items, Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });

                Clipboard.SetData(this.clipboardData.ToString(), (object)json);

                return true;
            }
            catch (Exception)
            {

            }

            return false;
        }

        public List<T> Paste()
        {
            var json = (string)Clipboard.GetData(this.clipboardData.ToString());
            var list = new List<T>();

            try
            {
                if (String.IsNullOrWhiteSpace(json))
                    return list;

                var jsonList = JsonConvert.DeserializeObject<List<T>>(json);

                if (jsonList != null && jsonList.Count > 0)
                    list.AddRange(jsonList);
            }
            catch (Exception)
            {

            }

            return list;
        }
    }
}
