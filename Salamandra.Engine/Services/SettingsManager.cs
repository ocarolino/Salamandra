using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Services
{
    // Based on: https://stackoverflow.com/questions/56847571/equivalent-to-usersettings-applicationsettings-in-wpf-net-5-net-6-or-net-c/56961180#56961180 (2022-04-22)
    public class SettingsManager<T> where T : class
    {
        private readonly string _filePath;

        public SettingsManager(string fileName)
        {
            _filePath = GetLocalFilePath(fileName);
        }

        private string GetLocalFilePath(string fileName)
        {
            string appData = Environment.CurrentDirectory;
            return Path.Combine(appData, fileName);
        }

        public T? LoadSettings() => File.Exists(_filePath) ? JsonConvert.DeserializeObject<T>(File.ReadAllText(_filePath)) : null;

        public void SaveSettings(T settings)
        {
            string json = JsonConvert.SerializeObject(settings);
            File.WriteAllText(_filePath, json);
        }
    }
}
