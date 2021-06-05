using Dawn;
using DbViewer.Models;
using DbViewer.Services;
using DbViewer.Views;
using Prism.Navigation;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;

namespace DbViewer.ViewModels
{
    public class CachedDatabaseListViewModel : NavigationViewModelBase, INavigatedAware
    {
        private readonly IDatabaseCacheService _cacheService;
        private readonly IHubService _hubService;

        public CachedDatabaseListViewModel(IDatabaseCacheService cacheService, INavigationService navigationService, IHubService hubService)
               : base(navigationService)
        {
            _cacheService = Guard.Argument(cacheService, nameof(cacheService))
                                 .NotNull()
                                 .Value;

            _hubService = Guard.Argument(hubService, nameof(hubService))
                               .NotNull()
                               .Value;

            ReloadCommand = ReactiveCommand.Create(ExecuteReload);
            ViewHubCommand = ReactiveCommand.CreateFromTask(ExecuteViewHubAsync);
            ViewSelectedDatabaseCommand = ReactiveCommand.CreateFromTask<CachedDatabaseItemViewModel>(
                                          ExecuteViewSelectedDatabase);
        }

        public ReactiveCommand<Unit, Unit> ReloadCommand { get; }

        public ReactiveCommand<Unit, Unit> ViewHubCommand { get; }

        public ReactiveCommand<CachedDatabaseItemViewModel, Unit> ViewSelectedDatabaseCommand { get; }

        public string HubAddress { get => _hubAddress; set => this.RaiseAndSetIfChanged(ref _hubAddress, value); }

        public ObservableCollection<CachedDatabaseItemViewModel> CachedDatabases
        {
            get => _cachedDatabases;
            set => this.RaiseAndSetIfChanged(ref _cachedDatabases, value);
        }

        private string _hubAddress;

        private ObservableCollection<CachedDatabaseItemViewModel> _cachedDatabases =
            new ObservableCollection<CachedDatabaseItemViewModel>();

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(INavigationParameters parameters) { ExecuteReload(); }

        private async void ExecuteReload()
        {
            var registry = await _cacheService.GetRegistry();

            OnCacheReceived(registry);
        }

        private void OnCacheReceived(CachedDatabaseRegistry cacheRegistry)
        {
            RunOnUi(
            () =>
            {
                CachedDatabases.Clear();

                foreach (var item in cacheRegistry.DatabaseCollection)
                {
                    CachedDatabases.Add(new CachedDatabaseItemViewModel(item, _hubService));
                }
            });
        }

        private Task ExecuteViewHubAsync() => NavigationService.NavigateAsync(nameof(HubListPage));

        private Task ExecuteViewSelectedDatabase(CachedDatabaseItemViewModel cachedDatabaseItemViewModel)
        {
            var navParams = new NavigationParameters
                            {
                                {
                                    nameof(CachedDatabaseItemViewModel),
                                    cachedDatabaseItemViewModel
                                }
                            };

            return NavigationService.NavigateAsync(nameof(DatabaseBrowserPage), navParams);
        }
    }
}