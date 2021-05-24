using Couchbase.Lite;
using Dawn;
using DbViewer.Models;
using ReactiveUI;
using System.Collections;
using System.Collections.Generic;

namespace DbViewer.ViewModels
{
    public class DocumentViewModel : ReactiveObject
    {
        private string _documentId;

        public DocumentViewModel(DocumentGroupViewModel groupViewModel, CachedDatabase database,
            string documentId)
        {
            GroupViewModel = Guard.Argument(groupViewModel, nameof(groupViewModel))
                  .NotNull()
                  .Value;

            DocumentId = Guard.Argument(documentId, nameof(documentId))
                  .NotNull()
                  .Value;

            Database = Guard.Argument(database, nameof(database))
                  .NotNull()
                  .Value;
        }

        public DocumentGroupViewModel GroupViewModel { get; }

        public Document Document { get; }
        public CachedDatabase Database { get; }

        public string DocumentId
        {
            get => _documentId;
            set => this.RaiseAndSetIfChanged(ref _documentId, value, nameof(DocumentId));
        }
    }

    public class DocumentModel : ReactiveObject
    {
        private string _documentId;

        public DocumentModel(string id)
        {
            _documentId = id;
        }

        public string DocumentId
        {
            get => _documentId;
            set => this.RaiseAndSetIfChanged(ref _documentId, value, nameof(DocumentId));
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
}