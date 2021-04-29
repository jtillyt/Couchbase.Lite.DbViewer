using Couchbase.Lite;
using Dawn;
using DBViewer.Services;
using Newtonsoft.Json;
using Prism.Navigation;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace DBViewer.ViewModels
{
    public class DocumentViewerViewModel : NavigationViewModelBase, INavigationAware
    {
        private readonly IDatabaseConnection _dataService;

        private string _documentId;

        public DocumentViewerViewModel(IDatabaseConnection dataService, INavigationService navigationService)
            : base(navigationService)
        {
            _dataService = Guard.Argument(dataService, nameof(dataService))
                  .NotNull()
                  .Value;

            ShareCommand = ReactiveCommand.CreateFromTask(ExecuteShare);
        }

        public DocumentViewModel DocumentViewModel { get; private set; }

        public ReactiveCommand<Unit, Unit> ShareCommand { get; }

        public Document Document { get; private set; }

        public string DocumentId
        {
            get => _documentId;
            set => this.RaiseAndSetIfChanged(ref _documentId, value, nameof(DocumentId));
        }

        private string _documentText;
        public string DocumentText
        {
            get => _documentText;
            set => this.RaiseAndSetIfChanged(ref _documentText, value);
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.ContainsKey(nameof(DocumentViewModel)))
            {
                DocumentViewModel = parameters.GetValue<DocumentViewModel>(nameof(DocumentViewModel));
            }

            Reload();
        }

        private void Reload()
        {
            string documentText = GetJson();

            RunOnUi(() =>
            {
                DocumentId = DocumentViewModel?.DocumentId;
                DocumentText = documentText;
            });
        }

        private string GetJson()
        {
            if (DocumentViewModel == null)
                return "";

            Document = _dataService.GetDocumentById(DocumentViewModel.DocumentId);
            return JsonConvert.SerializeObject(Document, Formatting.Indented);
        }

        private async Task ExecuteShare()
        {
            var textRequest = new ShareTextRequest(DocumentText);
            await Share.RequestAsync(textRequest);
        }
    }
}