using Dawn;
using DbViewer.DataStores;
using DbViewer.Models;
using DbViewer.Services;
using DbViewer.Views;
using Prism.Navigation;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace DbViewer.ViewModels
{
    public class CachedDatabaseListViewModel : NavigationViewModelBase, INavigatedAware
    {
        private readonly ILogger _logger = Log.ForContext<CachedDatabaseItemViewModel>();

        private readonly IDatabaseDatastore _cacheService;
        private readonly IHubService _hubService;

        public CachedDatabaseListViewModel(
            IDatabaseDatastore cacheService,
            INavigationService navigationService,
            IHubService hubService)
            : base(navigationService)
        {
            _cacheService = Guard.Argument(cacheService, nameof(cacheService)).NotNull().Value;
            _hubService = Guard.Argument(hubService, nameof(hubService)).NotNull().Value;

            _cacheService.CacheUpdated.Subscribe(OnCacheReceived)
                .DisposeWith(Disposables);

            ReloadCommand = ReactiveCommand.CreateFromTask(ExecuteReloadAsync);
            ViewHubCommand = ReactiveCommand.CreateFromTask(ExecuteViewHubAsync);
            ViewSelectedDatabaseCommand = ReactiveCommand.CreateFromTask<CachedDatabaseItemViewModel>(
                ExecuteViewSelectedDatabaseAsync);
        }

        public ReactiveCommand<Unit, Unit> ReloadCommand { get; }

        public ReactiveCommand<Unit, Unit> ViewHubCommand { get; }

        public ReactiveCommand<CachedDatabaseItemViewModel, Unit> ViewSelectedDatabaseCommand { get; }

        public string HubAddress
        {
            get => _hubAddress;
            set => this.RaiseAndSetIfChanged(ref _hubAddress, value);
        }

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

        public void OnNavigatedTo(INavigationParameters parameters) =>
            ExecuteReloadAsync(CancellationToken.None).ConfigureAwait(false);

        private async Task ExecuteReloadAsync(CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var registry = await _cacheService.GetRegistryAsync(CancellationToken.None).ConfigureAwait(false);

                OnCacheReceived(registry);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, nameof(ExecuteReloadAsync));
            }
        }

        private void OnCacheReceived(CachedDatabaseRegistry cacheRegistry)
        {
            RunOnUi(
                () =>
                {
                    CachedDatabases.Clear();

                    foreach (var item in cacheRegistry.DatabaseCollection)
                    {
                        CachedDatabases.Add(new CachedDatabaseItemViewModel(item, _cacheService, _hubService));
                    }
                });
        }

        private Task ExecuteViewHubAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return NavigationService.NavigateAsync(nameof(HubListPage));
        }

        private Task ExecuteViewSelectedDatabaseAsync(CachedDatabaseItemViewModel cachedDatabaseItemViewModel,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

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