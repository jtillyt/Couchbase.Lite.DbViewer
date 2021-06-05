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
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace DbViewer.ViewModels
{
    public class HubSettingsViewModel : NavigationViewModelBase, INavigatedAware
    {
        public const string HubId_NavParam = nameof(HubId_NavParam);

        private const string LastHubAddressKey = "LastHubAddress";
        private readonly IHubService _hubService;

        public HubSettingsViewModel(IHubService hubService, INavigationService navigationService)
            : base(navigationService)
        {
            _hubService = Guard
                .Argument(hubService, nameof(hubService))
                .NotNull()
                .Value;

            var canAddService = this.WhenAnyValue(x => x.SelectedScanner)
                                    .Select(x => x != null);
                 
            ViewSelectedServiceCommand = ReactiveCommand.CreateFromTask<ScanServiceListItemViewModel>(ExecuteViewSelectedService)
                                                        .DisposeWith(Disposables);

            AddScannerCommand = ReactiveCommand.CreateFromTask(ExecuteAddScanner, canAddService)
                                               .DisposeWith(Disposables);
        }

        public ReactiveCommand<Unit, Unit> AddScannerCommand { get; }

        public ReactiveCommand<ScanServiceListItemViewModel, Unit> ViewSelectedServiceCommand { get; }

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

        public ObservableCollection<ScanServiceListItemViewModel> ActiveScanners
        {
            get => _activeScanners;
            set => this.RaiseAndSetIfChanged(ref _activeScanners, value);
        }

        private ServiceDefinitionListItemViewModel _selectedScanner;
        public ServiceDefinitionListItemViewModel SelectedScanner
        {
            get => _selectedScanner;
            set => this.RaiseAndSetIfChanged(ref _selectedScanner, value);
        }

        public ObservableCollection<ServiceDefinitionListItemViewModel> AvailableScanners
        {
            get => _availableScanners;
            set => this.RaiseAndSetIfChanged(ref _availableScanners, value);
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.ContainsKey(HubId_NavParam))
            {
                var hubId = parameters.GetValue<string>(HubId_NavParam);

                _hubInfo = await _hubService.GetCachedHubAsync(hubId);

                var activeScannerViewModels = _hubInfo.ActiveServices.Select(service => new ScanServiceListItemViewModel(service));
                var availableScannerViewModels = _hubInfo.ServiceDefinitions.Select(service => new ServiceDefinitionListItemViewModel(service));

                RunOnUi(() =>
                {
                    ActiveScanners.Clear();
                    AvailableScanners.Clear();

                    ActiveScanners.AddRange(activeScannerViewModels);
                    AvailableScanners.AddRange(availableScannerViewModels);
                });
            }
        }

        private Task ExecuteViewSelectedService(ScanServiceListItemViewModel scanService)
        {
            var navParams = new NavigationParameters
            {
                { nameof(ServiceSettingsViewModel.HubId_NavParam), _hubInfo.Id },
                { nameof(ServiceSettingsViewModel.ServiceId_NavParam), scanService.ServiceInfo.Id }
            };

            return NavigationService.NavigateAsync(nameof(ServiceSettingsPage), navParams);
        }
        private async Task ExecuteAddScanner()
        {
            if (SelectedScanner == null)
            {
                return;
            }

            var activeService = CreateActiveServiceFromType(SelectedScanner.ServiceDefinition);
            _hubInfo.ActiveServices.Add(activeService);

            var result = await _hubService.UpdateHubAsync(_hubInfo);

            if (result)
            {
                AddServiceViewModel(activeService);
            }
        }

        private void AddServiceViewModel(ServiceInfo serviceInfo)
        {
            if (serviceInfo == null)
            {
                return;
            }

            RunOnUi(()=>
            {
                ActiveScanners.Add(new ScanServiceListItemViewModel(serviceInfo));
            });
        }

        private ServiceInfo CreateActiveServiceFromType(ServiceDefinition serviceDefinition)
        {
            var serviceInfo = new ServiceInfo();

            serviceInfo.Id = Guid.NewGuid().ToString();
            serviceInfo.ServiceName = serviceDefinition.Name;
            serviceInfo.ServiceTypeId = serviceDefinition.Id;
            serviceInfo.Properties = serviceDefinition.Properties;

            return serviceInfo;
        }


        private HubInfo _hubInfo;

        private ObservableCollection<ScanServiceListItemViewModel> _activeScanners =
            new ObservableCollection<ScanServiceListItemViewModel>();

        private ObservableCollection<ServiceDefinitionListItemViewModel> _availableScanners =
            new ObservableCollection<ServiceDefinitionListItemViewModel>();
    }
}