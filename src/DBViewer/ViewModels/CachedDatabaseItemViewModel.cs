using Dawn;
using DbViewer.Models;
using DbViewer.Services;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace DbViewer.ViewModels
{
    public class CachedDatabaseItemViewModel : ViewModelBase
    {
        private readonly IHubService _hubService;

        public CachedDatabaseItemViewModel(CachedDatabase cachedDatabase, IHubService hubService)
        {
            Database = Guard.Argument(cachedDatabase, nameof(cachedDatabase))
                                        .NotNull()
                                        .Value;

            _hubService = Guard.Argument(hubService, nameof(hubService))
                  .NotNull()
                  .Value;

            DisplayName = Database.RemoteDatabaseInfo?
                                        .DisplayDatabaseName;

            HubAddress = GetHubAddressString(Database);

            var dateTime = Database.DownloadTime.DateTime;
            DownloadTime = GetDownloadTimeString(dateTime);

            GetLatestCommand = ReactiveCommand.CreateFromTask(ExecuteGetLatest);
        }

        public ReactiveCommand<Unit, Unit> GetLatestCommand { get; }

        public CachedDatabase Database { get; set; }

        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }

        private string _downloadTime;
        public string DownloadTime
        {
            get => _downloadTime;
            set => this.RaiseAndSetIfChanged(ref _downloadTime, value);
        }

        private string _hubAddress;
        public string HubAddress
        {
            get => _hubAddress;
            set => this.RaiseAndSetIfChanged(ref _hubAddress, value);
        }

        private async Task ExecuteGetLatest()
        {
            var remoteInfo = Database.RemoteDatabaseInfo;

            var result = await _hubService.DownloadDatabaseAsync(remoteInfo.RequestAddress, remoteInfo);

            if (result.WasSuccesful)
            {
                RunOnUi(() =>
                {
                    DownloadTime = GetDownloadTimeString(DateTime.Now);
                });
            }
        }

        private static string GetDownloadTimeString(DateTime dateTime)
        {
            return $"{dateTime.ToShortDateString()} {dateTime.ToShortTimeString()}";
        }

        private static string GetHubAddressString(CachedDatabase cachedDatabase)
        {
            return cachedDatabase?.RemoteDatabaseInfo?.RequestAddress?.Host ?? "Unknown";
        }

       
    }
}
