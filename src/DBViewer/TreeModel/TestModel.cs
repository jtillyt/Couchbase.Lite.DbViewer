using System;
using System.ComponentModel;
using System.Linq;

//Taken from https://github.com/AdaptSolutions/Xamarin.Forms-TreeView

namespace DBViewer.TreeModel
{
    public class TestModel : INotifyPropertyChanged
    {
        private DateTime _TheDate;

        public DateTime TheDate
        {
            get => _TheDate;
            set
            {
                _TheDate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TheDate)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}