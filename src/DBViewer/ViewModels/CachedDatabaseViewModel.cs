using Dawn;
using DBViewer.Models;
using DBViewer.Services;
using DBViewer.Views;
using Prism.Navigation;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;

namespace DBViewer.ViewModels
{
    public class CachedDatabaseViewModel : NavigationViewModelBase, INavigatedAware
    {
        private readonly IDbCacheService _cacheService;

        public CachedDatabaseViewModel(IDbCacheService cacheService, INavigationService navigationService)
            : base(navigationService)
        {
            _cacheService = Guard
                .Argument(cacheService, nameof(cacheService))
                .NotNull()
                .Value;

            ReloadCommand = ReactiveCommand.Create(ExecuteReload);
            ViewHubCommand = ReactiveCommand.CreateFromTask(ExecuteViewHubAsync);
            ViewSelectedDatabaseCommand = ReactiveCommand.CreateFromTask<CachedDatabaseItemViewModel>(ExecuteViewSelectedDatabase);
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

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            ExecuteReload();
        }

        private void ExecuteReload()
        {
            _cacheService.GetRegistry()
               .Subscribe(OnCacheReceived);
        }

        private void OnCacheReceived(CacheRegistry cacheRegistry)
        {
            RunOnUi(() =>
            {
                CachedDatabases.Clear();

                foreach(var item in cacheRegistry.DatabaseCollection)
                {
                    CachedDatabases.Add(new CachedDatabaseItemViewModel(item));
                }
            });
        }

        private async Task ExecuteViewHubAsync()
        {
            await NavigationService.NavigateAsync(nameof(HubPage));
        }

        private async Task ExecuteViewSelectedDatabase(CachedDatabaseItemViewModel arg)
        {

        }
    }
}