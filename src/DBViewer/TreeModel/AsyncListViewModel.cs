using System.ComponentModel;
using System.Linq;

//Taken from https://github.com/AdaptSolutions/Xamarin.Forms-TreeView
namespace DbViewer.TreeModel
{
    public class AsyncListViewModel : INotifyPropertyChanged
    {
        #region Fields

        private ItemModel _ItemModel;

        #endregion

        #region Public Properties

        public ItemModel ItemModel
        {
            get => _ItemModel;

            set
            {
                _ItemModel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemModel)));
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}