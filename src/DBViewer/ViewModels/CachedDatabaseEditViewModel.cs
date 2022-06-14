using Dawn;
using DbViewer.DataStores;
using DbViewer.Models;
using DbViewer.Services;
using Prism.Navigation;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;

namespace DbViewer.ViewModels
{
    public class CachedDatabaseEditViewModel : NavigationViewModelBase, INavigatedAware
    {
        private readonly IHubService _hubService;
        private readonly IDatabaseDatastore _databaseCacheService;

        public CachedDatabaseEditViewModel(INavigationService navigationService, IDatabaseDatastore databaseCacheService, IHubService hubService)
            :base(navigationService)
        {
            _hubService = Guard.Argument(hubService, nameof(hubService))
                  .NotNull()
                  .Value;

            _databaseCacheService = Guard.Argument(databaseCacheService, nameof(databaseCacheService))
                  .NotNull()
                  .Value;

            SaveCommand = ReactiveCommand.CreateFromTask(ExecuteSaveAsync);
        }

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }

        public CachedDatabase Database { get; set; }

        public string DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }

        public string DownloadTime
        {
            get => _downloadTime;
            set => this.RaiseAndSetIfChanged(ref _downloadTime, value);
        }

        public string HubAddress
        {
            get => _hubAddress;
            set => this.RaiseAndSetIfChanged(ref _hubAddress, value);
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            var cachedVm = parameters.GetValue<CachedDatabaseItemViewModel>(nameof(CachedDatabaseItemViewModel));

            OnDatabaseUpdated(cachedVm.Database);
        }

        private void OnDatabaseUpdated(CachedDatabase cachedDatabase)
        {
            _cachedDatabase = cachedDatabase;

            HubAddress = GetHubAddressString(cachedDatabase);

            var dateTime = cachedDatabase.DownloadTime.DateTime;
            DownloadTime = GetDownloadTimeString(dateTime);

            DisplayName = cachedDatabase.UserDefinedDisplayName ?? cachedDatabase.RemoteDatabaseInfo?
                                                                                 .DisplayDatabaseName;
        }

        private async Task ExecuteSaveAsync(CancellationToken cancellationToken)
        {
            if (_cachedDatabase == null)
            {
                //Do we throw?  Alert?
                return;
            }

            _cachedDatabase.UserDefinedDisplayName = DisplayName;

            _databaseCacheService.SaveDatabase(_cachedDatabase, cancellationToken);
        }

        private static string GetDownloadTimeString(DateTime dateTime)
        {
            return $"{dateTime.ToShortDateString()} {dateTime.ToShortTimeString()}";
        }

        private static string GetHubAddressString(CachedDatabase cachedDatabase)
        {
            return cachedDatabase?.RemoteDatabaseInfo?.RequestAddress?.Host ?? "Unknown";
        }

        private string _displayName;
        private string _downloadTime;
        private string _hubAddress;

        private CachedDatabase _cachedDatabase;
    }
}
