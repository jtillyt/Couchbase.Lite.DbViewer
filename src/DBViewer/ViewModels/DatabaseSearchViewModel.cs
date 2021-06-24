using Dawn;
using DbViewer.DataStores;
using DbViewer.Models;
using DbViewer.Views;
using Newtonsoft.Json;
using Prism.Navigation;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;

namespace DbViewer.ViewModels
{
    public class DatabaseSearchViewModel : NavigationViewModelBase, INavigatedAware
    {
        internal const string DocumentIdListParam = nameof(DocumentIdListParam);

        private readonly IDatabaseDatastore _cacheService;

        private IEnumerable<string> _documentIdsToSearch;
        private string _searchTitle;
        private string _searchText = "";
        private ObservableCollection<DocumentModel> _searchResults;

        public DatabaseSearchViewModel(IDatabaseDatastore cacheService, INavigationService navigationService)
            : base(navigationService)
        {
            _cacheService = Guard
                .Argument(cacheService, nameof(cacheService))
                .NotNull()
                .Value;

            SearchCommand = ReactiveCommand.CreateFromTask(ExecuteSearchAsync);
            ViewSelectedDocumentCommand = ReactiveCommand.CreateFromTask<DocumentModel>(ExecuteViewSelectedDocumentAsync);
        }

        public ReactiveCommand<DocumentModel, Unit> ViewSelectedDocumentCommand { get; }

        public ReactiveCommand<Unit, Unit> SearchCommand { get; }

        public string SearchTitle
        {
            get => _searchTitle;
            set => this.RaiseAndSetIfChanged(ref _searchTitle, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        public ObservableCollection<DocumentModel> SearchResults
        {
            get => _searchResults;
            set => this.RaiseAndSetIfChanged(ref _searchResults, value);
        }

        private CachedDatabaseItemViewModel _currentDatabaseItemViewModel;
        public CachedDatabaseItemViewModel CurrentDatabaseItemViewModel
        {
            get => _currentDatabaseItemViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentDatabaseItemViewModel, value);
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.ContainsKey(nameof(CachedDatabaseItemViewModel)))
            {
                CurrentDatabaseItemViewModel = parameters.GetValue<CachedDatabaseItemViewModel>(nameof(CachedDatabaseItemViewModel));
            }
            if (parameters.ContainsKey(DocumentIdListParam))
            {
                _documentIdsToSearch = parameters.GetValue<IEnumerable<string>>(nameof(DocumentIdListParam));
            }
        }

        private Task ExecuteSearchAsync(CancellationToken cancellationToken)
        {
            RunOnUi(() =>
            {
                if (SearchResults != null)
                {
                    SearchResults.Clear();
                }
            });

            cancellationToken.ThrowIfCancellationRequested();

            return Task.Run(() =>
            {
                if (CurrentDatabaseItemViewModel == null)
                {
                    return;
                }

                var database = CurrentDatabaseItemViewModel.Database;
                var isConnected = database.Connect();

                if (!isConnected)
                {
                    return;
                }

                var connection = database.ActiveConnection;

                var documentIds = connection.ListAllDocumentIds();

                var searchTextCorrected = SearchText.ToLower();

                var documentIdsWithHits = new List<string>();

                foreach (var documentId in documentIds)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var document = connection.GetDocumentById(documentId);

                    var documentText = JsonConvert.SerializeObject(document);

                    if (documentText.ToLower().Contains(searchTextCorrected))
                    {
                        documentIdsWithHits.Add(documentId);
                    }
                }

                RunOnUi(() =>
                {
                    SearchResults = new ObservableCollection<DocumentModel>(documentIdsWithHits.Select(docId => new DocumentModel(CurrentDatabaseItemViewModel.Database, docId)));
                });
            }, cancellationToken);
        }

        private Task ExecuteViewSelectedDocumentAsync(DocumentModel document, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var navParams = new NavigationParameters
            {
                { nameof(DocumentModel), document }
            };

            return NavigationService.NavigateAsync(nameof(DocumentViewerPage), navParams);
        }
    }
}