using Couchbase.Lite;
using Dawn;
using DBViewer.Services;
using ReactiveUI;

namespace DBViewer.ViewModels
{
    public class DocumentViewModel : ReactiveObject
    {
        private readonly IDatabaseConnection _dataService;

        private string _documentId;

        public DocumentViewModel(DocumentGroupViewModel groupViewModel, IDatabaseConnection dataService,
            string documentId)
        {
            GroupViewModel = Guard.Argument(groupViewModel, nameof(groupViewModel))
                  .NotNull()
                  .Value;

            DocumentId = Guard.Argument(documentId, nameof(documentId))
                  .NotNull()
                  .Value;

            _dataService = Guard.Argument(dataService, nameof(dataService))
                  .NotNull()
                  .Value;
        }

        public DocumentGroupViewModel GroupViewModel {get; }

        public Document Document { get; }

        public string DocumentId
        {
            get => _documentId;
            set => this.RaiseAndSetIfChanged(ref _documentId, value, nameof(DocumentId));
        }
    }
}