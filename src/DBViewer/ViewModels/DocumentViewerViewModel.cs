using Couchbase.Lite;
using DbViewer.Models;
using Newtonsoft.Json;
using Prism.Navigation;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace DbViewer.ViewModels
{
    public class DocumentViewerViewModel : NavigationViewModelBase, INavigationAware
    {
        private string _documentId;
        private CachedDatabase _database;

        public DocumentViewerViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            ShareCommand = ReactiveCommand.CreateFromTask(ExecuteShare);
        }

        public DocumentModel DocumentModel { get; private set; }

        public ReactiveCommand<Unit, Unit> ShareCommand { get; }

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
            if (parameters.ContainsKey(nameof(ViewModels.DocumentModel)))
            {
                DocumentModel = parameters.GetValue<DocumentModel>(nameof(ViewModels.DocumentModel));
            }

            Reload();
        }

        private void Reload()
        {
            string documentText = GetJson();
            _database = DocumentModel?.Database;

            RunOnUi(() =>
            {
                DocumentId = DocumentModel?.DocumentId;
                DocumentText = documentText;
            });
        }

        private string GetJson()
        {
            if (DocumentModel?.Database == null)
                return "";

            var dbDoc = DocumentModel.Database.ActiveConnection.GetDocumentById(DocumentModel.DocumentId);
            return JsonConvert.SerializeObject(dbDoc, Formatting.Indented);
        }

        private async Task ExecuteShare()
        {
            var textRequest = new ShareTextRequest(DocumentText);
            await Share.RequestAsync(textRequest);
        }
    }
}