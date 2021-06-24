using DbViewer.Models;
using Newtonsoft.Json;
using Prism.Navigation;
using ReactiveUI;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace DbViewer.ViewModels
{
    public class DocumentViewerViewModel : NavigationViewModelBase, INavigationAware
    {
        private string _documentId;

        public DocumentViewerViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            ShareCommand = ReactiveCommand.CreateFromTask(ExecuteShareAsync);
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
            if (parameters.ContainsKey(nameof(Models.DocumentModel)))
            {
                DocumentModel = parameters.GetValue<DocumentModel>(nameof(Models.DocumentModel));
            }

            Reload();
        }

        private void Reload()
        {
            var documentText = GetJson();

            RunOnUi(() =>
            {
                DocumentId = DocumentModel?.DocumentId;
                DocumentText = documentText;
            });
        }

        private string GetJson()
        {
            if (DocumentModel?.Database == null)
            {
                return "";
            }

            var dbDoc = DocumentModel.Database.ActiveConnection.GetDocumentById(DocumentModel.DocumentId);
            return JsonConvert.SerializeObject(dbDoc, Formatting.Indented);
        }

        private async Task ExecuteShareAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var textRequest = new ShareTextRequest(DocumentText);
            await Share.RequestAsync(textRequest).ConfigureAwait(false);
        }
    }
}