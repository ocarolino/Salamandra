using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.ViewModel
{
    public class EventViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<int> Hours { get; set; }
        public ObservableCollection<DayOfWeek> Days { get; set; }

        public EventViewModel()
        {
            this.Hours = new ObservableCollection<int>();
            this.Days = new ObservableCollection<DayOfWeek>();

            for (int i = 0; i < 24; i++)
                this.Hours.Add(i);

            foreach (DayOfWeek item in Enum.GetValues(typeof(DayOfWeek)))
                this.Days.Add(item);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
