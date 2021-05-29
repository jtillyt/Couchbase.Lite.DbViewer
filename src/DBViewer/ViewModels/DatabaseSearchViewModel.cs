using Dawn;
using DbViewer.Services;
using DbViewer.Views;
using Newtonsoft.Json;
using Prism.Navigation;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace DbViewer.ViewModels
{
    public class DatabaseSearchViewModel : NavigationViewModelBase, INavigatedAware
    {
        internal const string DocumentIdList_Param = nameof(DocumentIdList_Param);

        private IEnumerable<string> _documentIdsToSearch;

        private readonly IDatabaseCacheService _cacheService;

        public DatabaseSearchViewModel(IDatabaseCacheService cacheService, INavigationService navigationService)
            : base(navigationService)
        {
            _cacheService = Guard
                .Argument(cacheService, nameof(cacheService))
                .NotNull()
                .Value;

            SearchCommand = ReactiveCommand.CreateFromTask(ExecuteSearch);
            ViewSelectedDocumentCommand = ReactiveCommand.CreateFromTask<DocumentModel>(ExecuteViewSelectedDocument);
        }

        public ReactiveCommand<DocumentModel, Unit> ViewSelectedDocumentCommand { get; }

        public ReactiveCommand<Unit, Unit> SearchCommand { get; }

        private string _searchTitle;
        public string SearchTitle
        {
            get => _searchTitle;
            set => this.RaiseAndSetIfChanged(ref _searchTitle, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        private ObservableCollection<DocumentModel> _searchResults;
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
            if (parameters.ContainsKey(DocumentIdList_Param))
            {
                _documentIdsToSearch = parameters.GetValue<IEnumerable<string>>(nameof(DocumentIdList_Param));
            }
        }

        private Task ExecuteSearch()
        {
            return Task.Run(() =>
            {
                if (CurrentDatabaseItemViewModel == null)
                    return;

                var database = CurrentDatabaseItemViewModel.Database;
                var isConnected = database.Connect();

                if (!isConnected)
                    return;

                var connection = database.ActiveConnection;

                var documentIds = connection.ListAllDocumentIds();

                var searchTextCorrected = SearchText.ToLower();

                var documentIdsWithHits = new List<string>();

                foreach (var documentId in documentIds)
                {
                    var document = connection.GetDocumentById(documentId);

                    var documentText = JsonConvert.SerializeObject(document);

                    if (documentText.ToLower().Contains(searchTextCorrected))
                    {
                        documentIdsWithHits.Add(documentId);
                    }
                }

                RunOnUi(() =>
                {
                    SearchResults = new ObservableCollection<DocumentModel>(documentIds.Select(docId => new DocumentModel(CurrentDatabaseItemViewModel.Database, docId)));
                });
            });
        }

        private async Task ExecuteViewSelectedDocument(DocumentModel document)
        {
            var navParams = new NavigationParameters
            {
                { nameof(DocumentModel), document }
            };

            await NavigationService.NavigateAsync(nameof(DocumentViewerPage), navParams);
        }
    }
}