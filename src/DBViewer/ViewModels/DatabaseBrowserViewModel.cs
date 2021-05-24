using Dawn;
using DbViewer.Services;
using DbViewer.Views;
using Prism.Navigation;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DbViewer.Models;
using DynamicData;
using Xamarin.Essentials;

namespace DbViewer.ViewModels
{
    public class DatabaseBrowserViewModel : NavigationViewModelBase, INavigatedAware
    {
        private const string UnknownFolder = "- Ungrouped -";
        private const string NoGroupsFolder = "Documents";

        private CachedDatabaseItemViewModel _currentDatabaseItemViewModel;
        private ReadOnlyObservableCollection<DocumentGroupModel> _documents;
        private readonly ISourceCache<DocumentModel, string> _documentCache =
            new SourceCache<DocumentModel, string>(x => x.DocumentId);

        private const string FilterSearch_Key = "FilterSearch";
        private const string DocumentSplit_Key = "DocumentSplit";
        private string _databaseName;
        private string _splitChars = "";
        private string _downloadTime;
        private string _filterText = "";

        private readonly IDatabaseCacheService _cacheService;

        public DatabaseBrowserViewModel(IDatabaseCacheService cacheService, INavigationService navigationService)
            : base(navigationService)
        {
            _cacheService = Guard
                .Argument(cacheService, nameof(cacheService))
                .NotNull()
                .Value;

            this.WhenAnyValue(x => x.FilterText, y => y.SplitChars)
                .Throttle(TimeSpan.FromSeconds(1))
                .Subscribe(_ => SaveSearchState())
                .DisposeWith(Disposables);

            var currentDatabaseChanged =
                this.WhenAnyValue(x => x.CurrentDatabaseItemViewModel)
                .Where(x => x != null)
                .Publish()
                .RefCount();

            currentDatabaseChanged
                .Select(x => x.Database.DownloadTime.LocalDateTime)
                .Select(x => $"{x.ToShortDateString()} {x.ToShortTimeString()}")
                .BindTo(this, x => x.DownloadTime)
                .DisposeWith(Disposables);

            currentDatabaseChanged
                .Do(x => x.Database.Connect())
                .Where(x => x.Database.ActiveConnection.IsConnected)
                .SelectMany(x => x.Database.ActiveConnection.ListAllDocumentIds())
                .Select(x => new DocumentModel(x))
                .Subscribe(_documentCache.AddOrUpdate)
                .DisposeWith(Disposables);

            IObservable<Func<DocumentModel, bool>> filterChanged =
                this.WhenAnyValue(x => x.FilterText)
                    .Throttle(TimeSpan.FromMilliseconds(300))
                    .Select(Filter);

            IObservable<Unit> splitChanged =
                this.WhenAnyValue(x => x.SplitChars)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .Select(x => Unit.Default);

            _documentCache
                .Connect()
                .RefCount()
                .Filter(filterChanged)
                .Group(x => GetGroupNameFromDocumentId(x.DocumentId, SplitChars), splitChanged)
                .Transform(x => new DocumentGroupModel(x, x.Key))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _documents)
                .Subscribe()
                .DisposeWith(Disposables);

            ReloadCommand = ReactiveCommand.Create(ExecuteReload, outputScheduler: RxApp.MainThreadScheduler);
            ViewSelectedDocumentCommand = ReactiveCommand.CreateFromTask<DocumentModel>(ExecuteViewSelectedDocument);
            ViewDatabaseSearchCommand = ReactiveCommand.CreateFromTask(ExecuteViewDatabaseSearch);
            FilterText = Preferences.Get(FilterSearch_Key, "");
            SplitChars = Preferences.Get(DocumentSplit_Key, "");
        }

        private static string GetGroupNameFromDocumentId(string documentId, string splitChar)
        {
            if (string.IsNullOrWhiteSpace(splitChar))
            {
                return NoGroupsFolder;
            }

            if (string.IsNullOrWhiteSpace(documentId))
            {
                return UnknownFolder;
            }

            var split = documentId.Split(splitChar.ToCharArray());

            return split.Length > 1 ? split.First() : UnknownFolder;
        }

        public ReactiveCommand<Unit, Unit> ViewDatabaseSearchCommand { get; set; }
        public ReactiveCommand<DocumentModel, Unit> ViewSelectedDocumentCommand { get; }
        public ReactiveCommand<Unit, Unit> ReloadCommand { get; }

        public string FilterText
        {
            get => _filterText;
            set => this.RaiseAndSetIfChanged(ref _filterText, value);
        }

        public string DatabaseName
        {
            get => _databaseName;
            set => this.RaiseAndSetIfChanged(ref _databaseName, value);
        }

        public string DownloadTime
        {
            get => _downloadTime;
            set => this.RaiseAndSetIfChanged(ref _downloadTime, value);
        }

        public string SplitChars
        {
            get => _splitChars;
            set => this.RaiseAndSetIfChanged(ref _splitChars, value);
        }


        private ObservableCollection<DocumentGroupViewModel> _documentGroups;

        public ObservableCollection<DocumentGroupViewModel> DocumentGroups
        {
            get => _documentGroups;
            set => this.RaiseAndSetIfChanged(ref _documentGroups, value);
        }


        public ReadOnlyObservableCollection<DocumentGroupModel> Documents => _documents;

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
        }

        private static Func<DocumentModel, bool> Filter(string arg) => model =>
        {
            if (string.IsNullOrWhiteSpace(arg))
                return true;

            var sections = arg.Split(',');

            var documentId = model.DocumentId.ToLowerInvariant();

            foreach (var section in sections)
            {
                if (string.IsNullOrWhiteSpace(section))
                    continue;

                var cleanedSection = section.Trim().ToLowerInvariant();

                if (documentId.Contains(cleanedSection.ToLowerInvariant()))
                {
                    return true;
                }
            }

            return false;
        };

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

            var shouldGroup = !string.IsNullOrWhiteSpace(SplitChars) &&
                              documentIds.All(doc => doc.Contains(SplitChars));

            var groupedDocuments = documentIds.GroupBy(key =>
            {
                return shouldGroup ? key.Substring(0, key.IndexOf(SplitChars)) : "Documents";
            });

            RunOnUi(() =>
            {
                DatabaseName = database.RemoteDatabaseInfo.DisplayDatabaseName;

                var dbRoot = database.LocalDatabasePathRoot;

                var localDateTime = database.DownloadTime.LocalDateTime;
                DownloadTime = $"{localDateTime.ToShortDateString()} {localDateTime.ToShortTimeString()}";

                DocumentGroups = new ObservableCollection<DocumentGroupViewModel>();

                foreach (var group in groupedDocuments)
                {
                    var groupViewModel =
                        new DocumentGroupViewModel(database, group.Key, group.ToList(), filteredGroupNames);

                    if (groupViewModel.Count > 0)
                        DocumentGroups.Add(groupViewModel);
                }
            });
        }

        private void SaveSearchState()
        {
            Preferences.Set(FilterSearch_Key, FilterText);
            Preferences.Set(DocumentSplit_Key, SplitChars);
        }

        private async Task ExecuteViewSelectedDocument(DocumentModel document) =>
            await NavigationService.NavigateAsync(nameof(DocumentViewerPage), (nameof(DocumentModel), document), (nameof(CachedDatabase), CurrentDatabaseItemViewModel.Database));

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