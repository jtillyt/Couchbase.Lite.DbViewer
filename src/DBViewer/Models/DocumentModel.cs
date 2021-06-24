using Dawn;
using System.Collections.Generic;
using System.ComponentModel;

namespace DbViewer.Models
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

        public CachedDatabase Database { get; }

        public string DocumentId
        {
            get => _documentId;
            set
            {
                if (!string.Equals(_documentId, value))
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
            public int Compare(DocumentModel x, DocumentModel y) => string.Compare(x.DocumentId, y.DocumentId, true);
        }
    }
}