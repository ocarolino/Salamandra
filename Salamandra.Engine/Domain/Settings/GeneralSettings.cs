using Salamandra.Engine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Domain.Settings
{
    public class GeneralSettings : INotifyPropertyChanged
    {
        public ViewLanguage ViewLanguage { get; set; }
        public string TimeAnnouncementFilesPath { get; set; }

        public GeneralSettings()
        {
            this.ViewLanguage = ViewLanguage.English;
            this.TimeAnnouncementFilesPath = String.Empty;
        }

        // ToDo: Maybe put this in a utils class or extension method?
        public string ViewLanguageToCultureName(ViewLanguage viewLanguage)
        {
            switch (viewLanguage)
            {
                case ViewLanguage.Portuguese:
                    return "pt-BR";
                case ViewLanguage.English:
                    return "en-US";
                default:
                    throw new NotImplementedException();
            }
        }

#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }
}
