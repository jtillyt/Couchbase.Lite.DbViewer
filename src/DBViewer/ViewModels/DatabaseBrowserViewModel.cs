using Dawn;
using DBViewer.Services;
using Prism.Navigation;
using ReactiveUI;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System;
using System.Reactive.Disposables;
using System.Collections.Generic;
using DBViewer.Views;
using Xamarin.Essentials;

namespace DBViewer.ViewModels
{
    public class DatabaseBrowserViewModel : NavigationViewModelBase, INavigatedAware
    {
        private const string FilterSearch_Key = "FilterSearch";

        private readonly IDatabaseCacheService _cacheService;
        private readonly IDatabaseService _databaseService;

        public DatabaseBrowserViewModel(IDatabaseCacheService cacheService, IDatabaseService databaseService, INavigationService navigationService)
            : base(navigationService)
        {
            _cacheService = Guard
                .Argument(cacheService, nameof(cacheService))
                .NotNull()
                .Value;

            _databaseService = Guard.Argument(databaseService, nameof(databaseService))
                  .NotNull()
                  .Value;

            this.WhenAnyValue(x => x.FilterGroupText)
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Subscribe(_ => ReloadDatabase())
                .DisposeWith(Disposables);

            this.WhenAnyValue(x => x.FilterGroupText)
                .Throttle(TimeSpan.FromSeconds(5))
                .Subscribe(_ => SaveSearchState())
                .DisposeWith(Disposables);

            ReloadCommand = ReactiveCommand.Create(ExecuteReload);
            ViewSelectedDocumentCommand = ReactiveCommand.CreateFromTask<DocumentViewModel>(ExecuteViewSelectedDocument);
            FilterGroupText = Preferences.Get(FilterSearch_Key,"");
        }

        public ReactiveCommand<DocumentViewModel, Unit> ViewSelectedDocumentCommand { get; }
        public ReactiveCommand<Unit, Unit> ReloadCommand { get; }

        private string _filterGroupTextText;
        public string FilterGroupText
        {
            get => _filterGroupTextText;
            set => this.RaiseAndSetIfChanged(ref _filterGroupTextText, value);
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

        private ObservableCollection<DocumentGroupViewModel> _documentGroups =
            new ObservableCollection<DocumentGroupViewModel>();

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
            ReloadDatabase();
        }

        private void ReloadDatabase()
        {
            // TODO: <James Thomas: 3/18/21> Handle
            if (CurrentDatabaseItemViewModel == null)
                return;

            _databaseService.Disconnect();

            var cachedDatabaseInfo = CurrentDatabaseItemViewModel.CachedDatabase;
            bool connected = _databaseService.Connect(cachedDatabaseInfo.LocalDatabasePathRoot, cachedDatabaseInfo.RemoteDatabaseInfo.DisplayDatabaseName);

            if (!connected)
                return;

            var filteredGroupNames = (FilterGroupText ?? "").Split(',');
            var documentIds = _databaseService.ListAllDocumentIds();
            var groupedDocuments = documentIds.GroupBy(key => { return key.Substring(0, key.IndexOf("::")); });
            var filteredGroups = groupedDocuments.Where(group => filteredGroupNames.Any(gn => gn.Count() == 0 || group.Key.ToLower().Contains(gn.ToLower())));
            var documentViewModels = new List<DocumentGroupViewModel>();

            foreach (var group in filteredGroups)
            {
                var groupViewModel = new DocumentGroupViewModel(_databaseService, group.Key, group.ToList());
                documentViewModels.Add(groupViewModel);
            }

            RunOnUi(() =>
            {
                DatabaseName = cachedDatabaseInfo.RemoteDatabaseInfo.DisplayDatabaseName;

                var dbRoot = cachedDatabaseInfo.LocalDatabasePathRoot;
                Debug.WriteLine(dbRoot);

                var localDateTime = cachedDatabaseInfo.DownloadTime.LocalDateTime;
                DownloadTime = $"{localDateTime.ToShortDateString()} {localDateTime.ToShortTimeString()}";

                DocumentGroups.Clear();

                foreach (var groupVm in documentViewModels)
                {
                    DocumentGroups.Add(groupVm);
                }
            });
        }

        private void SaveSearchState()
        {
            Preferences.Set(FilterSearch_Key, FilterGroupText);
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