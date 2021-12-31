using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using DbViewer.Services;
using DbViewer.Shared.Dtos;
using DbViewer.Views;
using DynamicData;
using Prism.Navigation;
using ReactiveUI;
using Serilog;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace DbViewer.ViewModels
{
    public class HubListViewModel : NavigationViewModelBase, INavigatedAware
    {
        public const string HubIdNavParam = nameof(HubIdNavParam);
        private const string LastHubAddressKey = "LastHubAddress";

        private readonly IPreferences _preferences;
        private readonly IHubService _hubService;
        private readonly ILogger _logger = Log.ForContext<HubListViewModel>();

        private string _hubAddress;

        private ObservableCollection<HubItemViewModel> _knownHubs =
            new ObservableCollection<HubItemViewModel>();

        public HubListViewModel(IHubService hubService, IPreferences preferences, INavigationService navigationService)
            : base(navigationService)
        {
            _hubService = Guard
                .Argument(hubService, nameof(hubService))
                .NotNull()
                .Value;

            _preferences = Guard.Argument(preferences, nameof(preferences))
                .NotNull()
                .Value;

            AddHubCommand = ReactiveCommand.CreateFromTask(ExecuteAddHubAsync);

            DeleteSelectedHubCommand = ReactiveCommand.CreateFromTask<HubItemViewModel>(ExecuteDeleteHubAsync);

            HubAddress = _preferences.Get(LastHubAddressKey, "http://127.0.0.1:5020");

            ReloadHubsCommand = ReactiveCommand.CreateFromTask(ExecuteReloadHubsAsync);

            ViewSelectedHubCommand = ReactiveCommand.CreateFromTask<HubItemViewModel>(ExecuteViewSelectedHubAsync);
        }

        public ReactiveCommand<Unit, Unit> AddHubCommand { get; }
        public ReactiveCommand<Unit, Unit> ReloadHubsCommand { get; }
        public ReactiveCommand<HubItemViewModel, Unit> DeleteSelectedHubCommand { get; }
        public ReactiveCommand<HubItemViewModel, Unit> ViewSelectedHubCommand { get; }

        public string HubAddress
        {
            get => _hubAddress;
            set => this.RaiseAndSetIfChanged(ref _hubAddress, value);
        }

        public ObservableCollection<HubItemViewModel> KnownHubs
        {
            get => _knownHubs;
            set => this.RaiseAndSetIfChanged(ref _knownHubs, value);
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            try
            {
                await ExecuteReloadHubsAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, nameof(OnNavigatedTo));
            }
        }

        private async Task ExecuteAddHubAsync(CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrEmpty(HubAddress) || 
                    KnownHubs.Any(h => h.HostAddress.Equals(HubAddress, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                var hubUri = new Uri(HubAddress);

                var hubInfo = await _hubService.TryAddHubAsync(hubUri, cancellationToken)
                    .ConfigureAwait(false);

                if (hubInfo != null)
                {
                    hubInfo.HostAddress = HubAddress;

                    AddHubToView(hubInfo);

                    _preferences.Set(LastHubAddressKey, HubAddress);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, nameof(ExecuteAddHubAsync));
            }
        }

        private async Task ExecuteDeleteHubAsync(HubItemViewModel hubItemViewModel, CancellationToken cancellationToken)
        {
            var result = await _hubService.TryDeleteHub(hubItemViewModel.HubId, cancellationToken)
                                          .ConfigureAwait(false);

            if (result)
            {
                KnownHubs.Remove(hubItemViewModel);
            }
        }

        private async Task ExecuteReloadHubsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var hubInfos = await _hubService.ListAllKnownHubsAsync(cancellationToken)
                .ConfigureAwait(false);

            if (hubInfos == null) return;

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

            RunOnUi(() => { KnownHubs.Add(hubViewModel); });
        }

        private Task ExecuteViewSelectedHubAsync(HubItemViewModel hubVm, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var navParams = new NavigationParameters
            {
                {nameof(HubDetailViewModel.HubIdNavParam), hubVm.HubId}
            };

            return NavigationService.NavigateAsync(nameof(HubDetailPage), navParams);
        }
    }
}