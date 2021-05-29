using Dawn;
using DbViewer.Extensions;
using DbViewer.Models;
using DbViewer.Services;
using DbViewer.Views;
using DynamicData;
using DynamicData.PLinq;
using Prism.Navigation;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace DbViewer.ViewModels
{
    public class DatabaseBrowserViewModel : NavigationViewModelBase, INavigatedAware
    {
        private const string UnknownFolder = "- Ungrouped -";
        private const string NoGroupsFolder = "Documents";
        private const int HumanEntryThrottleMs = 250;

        private string[] _possibleSplitDelims = { "::", "_" };

        private CachedDatabaseItemViewModel _currentDatabaseItemViewModel;
        private ReadOnlyObservableCollection<DocumentGroupViewModel> _documents;

        private readonly ISourceCache<DocumentModel, string> _documentCache =
            new SourceCache<DocumentModel, string>(x => x.DocumentId);

        private const string FilterSearch_Key = "FilterSearch";
        private const string DocumentSplit_Key = "DocumentSplit";
        private string _databaseName;
        private string _splitChars = null;
        private string _downloadTime;
        private string _filterText = null;

        private readonly IDatabaseCacheService _cacheService;

        public DatabaseBrowserViewModel(IDatabaseCacheService cacheService, INavigationService navigationService)
            : base(navigationService)
        {
            _cacheService = Guard
                .Argument(cacheService, nameof(cacheService))
                .NotNull()
                .Value;

            this.WhenAnyValue(x => x.FilterText)
                .Throttle(TimeSpan.FromMilliseconds(HumanEntryThrottleMs))
                .Subscribe(_ => SaveUserState())
                .DisposeWith(Disposables);

            var currentDatabaseChanged =
                this.WhenAnyValue(x => x.CurrentDatabaseItemViewModel)
                .Where(x => x != null)
                .Publish()
                .RefCount();

            currentDatabaseChanged
                .Select(x => x.Database.RemoteDatabaseInfo.DisplayDatabaseName)
                .BindTo(this, x => x.DatabaseName)
                .DisposeWith(Disposables);

            currentDatabaseChanged
                .Select(x => x.Database.DownloadTime.LocalDateTime)
                .Select(x => $"{x.ToShortDateString()} {x.ToShortTimeString()}")
                .BindTo(this, x => x.DownloadTime)
                .DisposeWith(Disposables);

            currentDatabaseChanged
                .Do(x => x.Database.Connect())
                .Where(x => x.Database.ActiveConnection.IsConnected)
                .Subscribe(x =>
                {
                    var docs = x.Database.ActiveConnection.ListAllDocumentIds(true);
                    var docVms = docs.Select(docId => new DocumentModel(x.Database, docId));

                    FindGroupChar(docs.Take(10));

                    _documentCache.Edit(cache =>
                    {
                        cache.AddOrUpdate(docVms);
                    });
                })
                .DisposeWith(Disposables);

            IObservable<Func<DocumentModel, bool>> filterChanged =
                this.WhenAnyValue(x => x.FilterText)
                    .Throttle(TimeSpan.FromMilliseconds(HumanEntryThrottleMs))
                    .Select(Filter);

            _documentCache
                .Connect()
                .RefCount()
                .Filter(filterChanged)
                .Group(x => GetGroupNameFromDocumentId(x.DocumentId, _splitChars))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Transform(x => new DocumentGroupViewModel(x, x.Key))
                .Bind(out _documents)
                .LogManagedThread("Browser - After Bind")
                .Subscribe()
                .DisposeWith(Disposables);

            RefreshCommand = ReactiveCommand.Create(ExecuteDatabaseRefresh, outputScheduler: RxApp.MainThreadScheduler);
            ViewSelectedDocumentCommand = ReactiveCommand.CreateFromTask<DocumentModel>(ExecuteViewSelectedDocument);
            ViewDatabaseSearchCommand = ReactiveCommand.CreateFromTask(ExecuteViewDatabaseSearch);
            FilterText = Preferences.Get(FilterSearch_Key, null);
        }

        private void FindGroupChar(IEnumerable<string> samples)
        {
            if (samples == null)
            {
                return;
            }

            foreach (var delim in from sample in samples
                                  from delim in _possibleSplitDelims
                                  let index = sample.IndexOf(delim)
                                  where index > 0
                                  select delim)
            {
                _splitChars = delim;
                break;
            }
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
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

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

        public ReadOnlyObservableCollection<DocumentGroupViewModel> Documents => _documents;

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

        private void ExecuteDatabaseRefresh()
        {
            // TODO: <James Thomas: 5/29/21> Add DB refresh and test
        }

        private void SaveUserState()
        {
            Preferences.Set(FilterSearch_Key, FilterText);
        }

        private async Task ExecuteViewSelectedDocument(DocumentModel document) =>
            await NavigationService.NavigateAsync(nameof(DocumentViewerPage), (nameof(DocumentModel), document), (nameof(CachedDatabase), CurrentDatabaseItemViewModel.Database));

        private Task ExecuteViewDatabaseSearch()
        {
            var documentsToSearch = Documents.SelectMany(dg => dg.Select(d => d.DocumentId));
            var navParams = new NavigationParameters
            {
                { nameof(CachedDatabaseItemViewModel), CurrentDatabaseItemViewModel },
                { nameof(DatabaseSearchViewModel.DocumentIdList_Param), documentsToSearch }
            };

            return NavigationService.NavigateAsync(nameof(DatabaseSearchPage), navParams);
        }
    }
}