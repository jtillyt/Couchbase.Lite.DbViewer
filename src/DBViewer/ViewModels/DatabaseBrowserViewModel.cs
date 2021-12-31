using Dawn;
using DbViewer.Extensions;
using DbViewer.Models;
using DbViewer.Services;
using DbViewer.Shared.Dtos;
using DbViewer.Views;
using DynamicData;
using DynamicData.PLinq;
using Prism.Navigation;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace DbViewer.ViewModels
{
    public class DatabaseBrowserViewModel : NavigationViewModelBase, INavigatedAware
    {
        private static ILogger _logger = Log.ForContext<DatabaseBrowserViewModel>();

        private readonly IHubService _hubService;

        private const string FilterSearchKey = "FilterSearch";

        private const string UnknownFolder = "- Ungrouped -";
        private const string NoGroupsFolder = "Documents";

        private const int HumanEntryThrottleMs = 250;

        private readonly string[] _possibleSplitDelims = { "::", "_" };

        private readonly ReadOnlyObservableCollection<DocumentGroupViewModel> _documents;

        private CachedDatabaseItemViewModel _currentDatabaseItemViewModel;

        private readonly ISourceCache<DocumentModel, string> _documentCache =
            new SourceCache<DocumentModel, string>(x => x.DocumentId);

        private string _databaseName;
        private string _splitChars;
        private string _downloadTime;
        private string _filterText;

        public DatabaseBrowserViewModel(INavigationService navigationService, IHubService hubService)
            : base(navigationService)
        {
            _hubService = Guard.Argument(hubService, nameof(hubService))
                               .NotNull()
                               .Value;

            this.WhenAnyValue(x => x.FilterText)
                .Throttle(TimeSpan.FromMilliseconds(HumanEntryThrottleMs))
                .Subscribe(_ => SaveUserState())
                .DisposeWith(Disposables);

            var currentDatabaseChanged =
                this.WhenAnyValue(x => x.CurrentDatabaseItemViewModel).Where(x => x != null).Publish().RefCount();

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
                .Subscribe(
                    x =>
                {
                    var localDatabasePath = x.Database.LocalDatabasePathFull.Replace(@"\", @"/");
                    var databaseInfo = new { localDatabasePath, x.Database.RemoteDatabaseInfo.DisplayDatabaseName };
                    _logger.Verbose("Current database has changed to {@DatabaseInfo}", databaseInfo);

                    var docs = x.Database.ActiveConnection.ListAllDocumentIds(true);
                    var docVms = docs.Select(docId => new DocumentModel(x.Database, docId));

                    FindGroupChar(docs.Take(10));

                    _documentCache.Edit(
                        cache => { cache.AddOrUpdate(docVms); });
                })
                .DisposeWith(Disposables);

            var filterChanged =
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

            RefreshCommand = ReactiveCommand.CreateFromTask(
                ExecuteDatabaseRefreshAsync,
                outputScheduler: RxApp.MainThreadScheduler);

            ViewSelectedDocumentCommand =
                ReactiveCommand.CreateFromTask<DocumentModel>(ExecuteViewSelectedDocumentAsync);

            ViewDatabaseSearchCommand = ReactiveCommand.CreateFromTask(ExecuteViewDatabaseSearchAsync);

            DeleteDocumentCommand = ReactiveCommand.CreateFromTask<DocumentModel>(ExecuteDeleteDocumentAsync);

            FilterText = Preferences.Get(FilterSearchKey, null);
        }

        public ReactiveCommand<Unit, Unit> ViewDatabaseSearchCommand { get; set; }

        public ReactiveCommand<DocumentModel, Unit> ViewSelectedDocumentCommand { get; }
        public ReactiveCommand<DocumentModel, Unit> DeleteDocumentCommand { get; }

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
                CurrentDatabaseItemViewModel = parameters.GetValue<CachedDatabaseItemViewModel>(
                    nameof(CachedDatabaseItemViewModel));
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

        private static Func<DocumentModel, bool> Filter(string arg) => model =>
        {
            if (string.IsNullOrWhiteSpace(arg))
            {
                return true;
            }

            var sections = arg.Split(',');

            var documentId = model.DocumentId.ToLowerInvariant();

            foreach (var section in sections)
            {
                if (string.IsNullOrWhiteSpace(section))
                {
                    continue;
                }

                var cleanedSection = section.Trim().ToLowerInvariant();

                if (documentId.Contains(cleanedSection.ToLowerInvariant()))
                {
                    return true;
                }
            }

            return false;
        };

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

        private Task ExecuteDatabaseRefreshAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // TODO: <James Thomas: 5/29/21> Add DB refresh and test
            return Task.CompletedTask;
        }

        private void SaveUserState()
        {
            Preferences.Set(FilterSearchKey, FilterText);
        }

        private Task ExecuteViewSelectedDocumentAsync(DocumentModel document, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return NavigationService.NavigateAsync(
                nameof(DocumentViewerPage),
                (nameof(DocumentModel), document),
                (nameof(CachedDatabase), CurrentDatabaseItemViewModel.Database));
        }
        private async Task ExecuteDeleteDocumentAsync(DocumentModel document, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var deletedDocument = await _hubService.DeleteDocument(document.Database.RemoteDatabaseInfo, document.DocumentId, cancellationToken);

                if (deletedDocument)
                {
                    var documentGroup = Documents.FirstOrDefault(group => group.Any(doc => doc.DocumentId == document.DocumentId));

                    documentGroup.Remove(document);
                }

                CurrentDatabaseItemViewModel.Database.ActiveConnection.DeleteDocumentById(document.DocumentId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to delete document");
            }
        }

        private Task ExecuteViewDatabaseSearchAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var documentsToSearch = Documents.SelectMany(dg => dg.Select(d => d.DocumentId));
            var navParams = new NavigationParameters
            {
                {
                    nameof(CachedDatabaseItemViewModel),
                    CurrentDatabaseItemViewModel
                },
                {
                    nameof(DatabaseSearchViewModel.DocumentIdListParam),
                    documentsToSearch
                }
            };

            return NavigationService.NavigateAsync(nameof(DatabaseSearchPage), navParams);
        }

    }
}