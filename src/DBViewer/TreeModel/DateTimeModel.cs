using System;
using System.ComponentModel;
using System.Linq;

namespace DbViewer.TreeModel
{
    public class DateTimeModel : INotifyPropertyChanged
    {
        private DateTime _TheDateTime;

        public DateTime TheDateTime
        {
            get => _TheDateTime;
            set
            {
                _TheDateTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TheDateTime)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}