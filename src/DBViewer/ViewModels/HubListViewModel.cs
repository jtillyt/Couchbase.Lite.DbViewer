using Dawn;
using DbViewer.Services;
using DbViewer.Shared.Configuration;
using DbViewer.Views;
using DynamicData;
using Prism.Navigation;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace DbViewer.ViewModels
{
    public class HubListViewModel : NavigationViewModelBase, INavigatedAware
    {
        public const string HubId_NavParam = nameof(HubId_NavParam);

        private const string LastHubAddressKey = "LastHubAddress";
        private readonly IHubService _hubService;

        public HubListViewModel(IHubService hubService, INavigationService navigationService)
            : base(navigationService)
        {
            _hubService = Guard
                .Argument(hubService, nameof(hubService))
                .NotNull()
                .Value;

            AddHubCommand = ReactiveCommand.CreateFromTask(ExecuteAddHub);

            // TODO: <James Thomas: 3/14/21> Move preferences to DI 
            HubAddress = Preferences.Get(LastHubAddressKey, "http://127.0.0.1:5020");
            ReloadHubsCommand = ReactiveCommand.CreateFromTask(ExecuteReloadHubs);
            ViewSelectedHubCommand = ReactiveCommand.CreateFromTask<HubItemViewModel>(ExecuteViewSelectedHub);
        }

        public ReactiveCommand<Unit, Unit> AddHubCommand { get; }
        public ReactiveCommand<Unit, Unit> ReloadHubsCommand { get; }
        public ReactiveCommand<HubItemViewModel, Unit> ViewSelectedHubCommand { get; }

        public string HubAddress
        {
            get => _hubAddress;
            set => this.RaiseAndSetIfChanged(ref _hubAddress, value);
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            await ExecuteReloadHubs();
        }

        public ObservableCollection<HubItemViewModel> KnownHubs
        {
            get => _knownHubs;
            set => this.RaiseAndSetIfChanged(ref _knownHubs, value);
        }

        private async Task ExecuteAddHub()
        {
            if (string.IsNullOrEmpty(HubAddress))
            {
                return;
            }

            var hubUri = new Uri(HubAddress);

            var hubInfo = await _hubService.TryAddHubAsync(hubUri);

            if (hubInfo != null)
            {
                hubInfo.HostAddress = HubAddress;

                AddHubToView(hubInfo);

                Preferences.Set(LastHubAddressKey, HubAddress);
            }
        }

        private async Task ExecuteReloadHubs()
        {
            var hubInfos = await _hubService.ListAllKnownHubsAsync();

            if (hubInfos == null)
                return;

            var hubViewModels = hubInfos.Select(hubInfo => new HubItemViewModel(hubInfo));

            RunOnUi(() =>
            {
                KnownHubs.Clear();

                KnownHubs.AddRange(hubViewModels);
            });
        }

        private void AddHubToView(HubInfo hubInfo)
        {
            var hubViewModel = new HubItemViewModel(hubInfo);

            RunOnUi(() =>
            {
                KnownHubs.Add(hubViewModel);
            });
        }

        private Task ExecuteViewSelectedHub(HubItemViewModel hubVm)
        {
            var navParams = new NavigationParameters
            {
                { nameof(HubDetailViewModel.HubId_NavParam), hubVm.HubId }
            };

            return NavigationService.NavigateAsync(nameof(HubDetailPage), navParams);
        }

        private string _hubAddress;

        private ObservableCollection<HubItemViewModel> _knownHubs =
            new ObservableCollection<HubItemViewModel>();
    }
}