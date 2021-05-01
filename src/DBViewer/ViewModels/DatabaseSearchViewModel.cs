using Dawn;
using DBViewer.Services;
using DBViewer.Views;
using Newtonsoft.Json;
using Prism.Navigation;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace DBViewer.ViewModels
{
    public class DatabaseSearchViewModel : NavigationViewModelBase, INavigatedAware
    {
        internal const string DocumentIdList_Param = nameof(DocumentIdList_Param);

        private IEnumerable<string> _documentIds;

        private readonly IDatabaseCacheService _cacheService;

        public DatabaseSearchViewModel(IDatabaseCacheService cacheService, INavigationService navigationService)
            : base(navigationService)
        {
            _cacheService = Guard
                .Argument(cacheService, nameof(cacheService))
                .NotNull()
                .Value;

            SearchCommand = ReactiveCommand.Create(ExecuteSearch);
            ViewSelectedDocumentCommand = ReactiveCommand.CreateFromTask<DocumentViewModel>(ExecuteViewSelectedDocument);
        }

        public ReactiveCommand<DocumentViewModel, Unit> ViewSelectedDocumentCommand { get; }
        public ReactiveCommand<Unit, Unit> SearchCommand { get; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        private ObservableCollection<DocumentGroupViewModel> _documentGroups;

        public ObservableCollection<DocumentGroupViewModel> DocumentGroups
        {
            get => _documentGroups;
            set => this.RaiseAndSetIfChanged(ref _documentGroups, value);
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
                _documentIds = parameters.GetValue<IEnumerable<string>>(nameof(DocumentIdList_Param));
            }
        }

        private void ExecuteSearch()
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
                    documentIdsWithHits.Add(documentId);
            }

            var groupedDocuments = documentIdsWithHits.GroupBy(key => { return "Matches"; });

            RunOnUi(() =>
            {
                var dbRoot = database.LocalDatabasePathRoot;

                var localDateTime = database.DownloadTime.LocalDateTime;

                DocumentGroups = new ObservableCollection<DocumentGroupViewModel>();

                foreach (var group in groupedDocuments)
                {
                    var groupViewModel = new DocumentGroupViewModel(database, group.Key, group.ToList());

                    if (groupViewModel.Count > 0)
                        DocumentGroups.Add(groupViewModel);
                }
            });
        }

        private async Task ExecuteViewSelectedDocument(DocumentViewModel document)
        {
            var navParams = new NavigationParameters
            {
                { nameof(DocumentViewModel), document }
            };

            await NavigationService.NavigateAsync(nameof(DocumentViewerPage), navParams);
        }
    }
}