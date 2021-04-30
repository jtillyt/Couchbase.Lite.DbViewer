using Couchbase.Lite;
using Dawn;
using DBViewer.Models;
using ReactiveUI;

namespace DBViewer.ViewModels
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

        public DocumentGroupViewModel GroupViewModel {get; }

        public Document Document { get; }
        public CachedDatabase Database { get; }

        public string DocumentId
        {
            get => _documentId;
            set => this.RaiseAndSetIfChanged(ref _documentId, value, nameof(DocumentId));
        }
    }
}