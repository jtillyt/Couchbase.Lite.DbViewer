using Dawn;
using DbViewer.Shared;
using DbViewer.Services;
using Prism.Navigation;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Xamarin.Essentials;
using DbViewer.Views;
using DbViewer.Shared.Configuration;

namespace DbViewer.ViewModels
{
    public class HubDetailViewModel : NavigationViewModelBase, INavigatedAware
    {
        public const string HubId_NavParam = nameof(HubId_NavParam);

        private const string LastHubAddressKey = "LastHubAddress";
        private readonly IHubService _hubService;

        public HubDetailViewModel(IHubService hubService, INavigationService navigationService)
            : base(navigationService)
        {
            _hubService = Guard
                .Argument(hubService, nameof(hubService))
                .NotNull()
                .Value;

            RescanCommand = ReactiveCommand.CreateFromTask(ExecuteRescanForAllDatabases);
            DownloadCheckedCommand = ReactiveCommand.CreateFromTask(ExecuteDownloadChecked);
            ViewHubSetupCommand = ReactiveCommand.CreateFromTask(ExecuteViewHubSetup);

            UpdateStatus("");
        }

        public ReactiveCommand<Unit, Unit> RescanCommand { get; }
        public ReactiveCommand<Unit, Unit> DownloadCheckedCommand { get; }
        public ReactiveCommand<Unit, Unit> ViewHubSetupCommand { get; }

        public string HubAddress
        {
            get => _hubAddress;
            set => this.RaiseAndSetIfChanged(ref _hubAddress, value);
        }

        private string _hubName;
        public string HubName
        {
            get => _hubName;
            set => this.RaiseAndSetIfChanged(ref _hubName, value);
        }

        private string _status;
        public string Status
        {
            get => _status;
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        public ObservableCollection<RemoteDatabaseViewModel> RemoteDatabases
        {
            get => _remoteDatabases;
            set => this.RaiseAndSetIfChanged(ref _remoteDatabases, value);
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            UpdateStatus("Loading..");

            if (parameters.ContainsKey(HubId_NavParam))
            {
                var hubId = parameters.GetValue<string>(HubId_NavParam);

                _hubInfo = await _hubService.GetCachedHubAsync(hubId);
                RunOnUi(() =>
                {
                    HubName = _hubInfo.HubName;
                    HubAddress = _hubInfo.HostAddress;
                });

                UpdateStatus($"Loaded");
            }
        }

        private void UpdateStatus(string status)
        {
            RunOnUi(() =>
            {
                Status = status;
            });
        }

        private async Task ExecuteRescanForAllDatabases()
        {
            if (string.IsNullOrEmpty(HubAddress))
            {
                return;
            }

            var hubUri = new Uri(HubAddress);

            Preferences.Set(LastHubAddressKey, HubAddress);

            IEnumerable<DatabaseInfo> dbList = null;

            try
            {
                dbList = await _hubService.ListAllHubDatabasesAsync(hubUri);
            }
            catch (Exception ex)
            {
                //Wire up logging
            }

            if (dbList == null)
            {
                return;
            }

            var viewModels = dbList.Select(db => new RemoteDatabaseViewModel(db));

            RunOnUi(() =>
            {
                RemoteDatabases.Clear();

                foreach (var vm in viewModels)
                {
                    RemoteDatabases.Add(vm);
                }
            });
        }
        private Task ExecuteViewHubSetup()
        {
            var navParams = new NavigationParameters
            {
                { nameof(HubSettingsViewModel.HubId_NavParam), _hubInfo.Id }
            };

            return NavigationService.NavigateAsync(nameof(HubSettingsPage), navParams);
        }

        private async Task ExecuteDownloadChecked()
        {
            var hubUri = new Uri(HubAddress);

            foreach (var vm in RemoteDatabases)
            {
                if (vm.ShouldDownload)
                {
                    await _hubService.DownloadDatabaseAsync(hubUri, vm.DatabaseInfo);
                }
            }

            await NavigationService.NavigateAsync(nameof(CachedDatabaseListPage));
        }

        private HubInfo _hubInfo;
        private string _hubAddress;

        private ObservableCollection<RemoteDatabaseViewModel> _remoteDatabases =
            new ObservableCollection<RemoteDatabaseViewModel>();

    }
}