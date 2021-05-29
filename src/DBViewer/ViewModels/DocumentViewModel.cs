using Couchbase.Lite;
using Dawn;
using DbViewer.Models;
using System.Collections.Generic;
using System.ComponentModel;

namespace DbViewer.ViewModels
{
    public class DocumentModel : INotifyPropertyChanged
    {
        private string _documentId;

        public DocumentModel(CachedDatabase database, string documentId)
        {
            DocumentId = Guard.Argument(documentId, nameof(documentId))
                .NotNull()
                .Value;

            Database = Guard.Argument(database, nameof(database))
                .NotNull()
                .Value;
        }

        public Document Document { get; }

        public CachedDatabase Database { get; }

        public string DocumentId
        {
            get => _documentId;
            set
            {
                if (_documentId != value)
                {
                    _documentId = value;

                    OnPropertyChanged(nameof(DocumentId));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class Comparer : IComparer<DocumentModel>
        {
            public int Compare(DocumentModel x, DocumentModel y)
            {
                if (x.DocumentId == null || y.DocumentId == null)
                {
                    return 0;
                }

                return string.Compare(x.DocumentId, y.DocumentId, true);
            }
        }
    }

    //public class DocumentModel : INotifyPropertyChanged
    //{
    //    private string _documentId;

    //    public DocumentModel(string id)
    //    {
    //        _documentId = id;
    //    }

    //    public string DocumentId
    //    {
    //        get => _documentId;
    //        set
    //        {
    //            if (_documentId != value)
    //            {
    //                _documentId = value;

    //                OnPropertyChanged(nameof(DocumentId));
    //            }
    //        }
    //    }

    //    public event PropertyChangedEventHandler PropertyChanged;

    //    private void OnPropertyChanged(string propertyName)
    //    {
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //    }

    //    public class Comparer : IComparer<DocumentModel>
    //    {
    //        public int Compare(DocumentModel x, DocumentModel y)
    //        {
    //            if (x.DocumentId == null || y.DocumentId == null)
    //            {
    //                return 0;
    //            }

    //            return string.Compare(x.DocumentId, y.DocumentId, true);
    //        }
    //    }
    //}
}