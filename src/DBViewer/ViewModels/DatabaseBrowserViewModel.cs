using Dawn;
using DBViewer.Services;
using DBViewer.Views;
using Prism.Navigation;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace DBViewer.ViewModels
{
    public class DatabaseBrowserViewModel : NavigationViewModelBase, INavigatedAware
    {
        private const string FilterSearch_Key = "FilterSearch";

        private readonly IDatabaseCacheService _cacheService;

        public DatabaseBrowserViewModel(IDatabaseCacheService cacheService, INavigationService navigationService)
            : base(navigationService)
        {
            _cacheService = Guard
                .Argument(cacheService, nameof(cacheService))
                .NotNull()
                .Value;

            this.WhenAnyValue(x => x.FilterText)
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Subscribe(_ => ExecuteReload())
                .DisposeWith(Disposables);

            this.WhenAnyValue(x => x.FilterText)
                .Throttle(TimeSpan.FromSeconds(5))
                .Subscribe(_ => SaveSearchState())
                .DisposeWith(Disposables);

            ReloadCommand = ReactiveCommand.Create(ExecuteReload);
            ViewSelectedDocumentCommand = ReactiveCommand.CreateFromTask<DocumentViewModel>(ExecuteViewSelectedDocument);
            ViewDatabaseSearchCommand = ReactiveCommand.CreateFromTask(ExecuteViewDatabaseSearch);
            FilterText = Preferences.Get(FilterSearch_Key, "");
        }

        public ReactiveCommand<Unit, Unit> ViewDatabaseSearchCommand { get; set; }
        public ReactiveCommand<DocumentViewModel, Unit> ViewSelectedDocumentCommand { get; }
        public ReactiveCommand<Unit, Unit> ReloadCommand { get; }

        private string _filterText;
        public string FilterText
        {
            get => _filterText;
            set => this.RaiseAndSetIfChanged(ref _filterText, value);
        }

        private string _databaseName;
        public string DatabaseName
        {
            get => _databaseName;
            set => this.RaiseAndSetIfChanged(ref _databaseName, value);
        }

        private string _downloadTime;
        public string DownloadTime
        {
            get => _downloadTime;
            set => this.RaiseAndSetIfChanged(ref _downloadTime, value);
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
            // TODO: <James Thomas: 3/18/21> Standardize params
            if (parameters.ContainsKey(nameof(CachedDatabaseItemViewModel)))
            {
                CurrentDatabaseItemViewModel = parameters.GetValue<CachedDatabaseItemViewModel>(nameof(CachedDatabaseItemViewModel));
            }

            ExecuteReload();
        }

        private void ExecuteReload()
        {
            // TODO: <James Thomas: 3/18/21> Handle
            if (CurrentDatabaseItemViewModel == null)
                return;

            var database = CurrentDatabaseItemViewModel.Database;
            var isConnected = database.Connect();

            if (!isConnected)
                return;

            var connection = database.ActiveConnection;

            var filteredGroupNames = (FilterText ?? "").Split(',');
            var documentIds = connection.ListAllDocumentIds();
            var groupedDocuments = documentIds.GroupBy(key => { return key.Substring(0, key.IndexOf("::")); });
            //var filteredGroups = groupedDocuments.Where(group => filteredGroupNames.Any(gn => gn.Count() == 0 || group.Key.ToLower().Contains(gn.ToLower())));

            RunOnUi(() =>
            {
                DatabaseName = database.RemoteDatabaseInfo.DisplayDatabaseName;

                var dbRoot = database.LocalDatabasePathRoot;

                var localDateTime = database.DownloadTime.LocalDateTime;
                DownloadTime = $"{localDateTime.ToShortDateString()} {localDateTime.ToShortTimeString()}";

                DocumentGroups = new ObservableCollection<DocumentGroupViewModel>();

                foreach (var group in groupedDocuments)
                {
                    var groupViewModel = new DocumentGroupViewModel(database, group.Key, group.ToList(), filteredGroupNames);

                    if (groupViewModel.Count > 0)
                        DocumentGroups.Add(groupViewModel);
                }
            });
        }

        private void SaveSearchState()
        {
            Preferences.Set(FilterSearch_Key, FilterText);
        }

        private async Task ExecuteViewSelectedDocument(DocumentViewModel document)
        {
            var navParams = new NavigationParameters
            {
                { nameof(DocumentViewModel), document }
            };

            await NavigationService.NavigateAsync(nameof(DocumentViewerPage), navParams);
        }

        private async Task ExecuteViewDatabaseSearch()
        {
            var documentsToSearch = DocumentGroups.SelectMany(dg => dg.Select(d => d.DocumentId));
            var navParams = new NavigationParameters
            {
                { nameof(CachedDatabaseItemViewModel), CurrentDatabaseItemViewModel },
                { nameof(DatabaseSearchViewModel.DocumentIdList_Param), documentsToSearch }
            };

            await NavigationService.NavigateAsync(nameof(DatabaseSearchPage), navParams);
        }
    }
}