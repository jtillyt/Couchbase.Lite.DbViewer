using Dawn;
using DbViewer.Services;
using DbViewer.Shared.Dtos;
using DbViewer.Views;
using DynamicData;
using Prism.Navigation;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DbViewer.ViewModels
{
    public class HubSettingsViewModel : NavigationViewModelBase, INavigatedAware
    {
        public const string HubIdNavParam = nameof(HubIdNavParam);

        private ObservableCollection<ScanServiceListItemViewModel> _activeScanners =
            new ObservableCollection<ScanServiceListItemViewModel>();

        private ObservableCollection<ServiceDefinitionListItemViewModel> _availableScanners =
            new ObservableCollection<ServiceDefinitionListItemViewModel>();

        private readonly ILogger _logger = Log.ForContext<HubSettingsViewModel>();
        private readonly IHubService _hubService;

        private ServiceDefinitionListItemViewModel _selectedScannerType;
        private HubInfo _hubInfo;
        private string _hubName;
        private string _status;

        public HubSettingsViewModel(IHubService hubService, INavigationService navigationService)
            : base(navigationService)
        {
            _hubService = Guard
                .Argument(hubService, nameof(hubService))
                .NotNull()
                .Value;

            var canAddService = this.WhenAnyValue(x => x.SelectedScannerType).Select(x => x != null);

            ViewSelectedServiceCommand = ReactiveCommand.CreateFromTask<ScanServiceListItemViewModel>(
                    ExecuteViewSelectedServiceAsync)
                .DisposeWith(Disposables);

            AddScannerCommand = ReactiveCommand.CreateFromTask(ExecuteAddScannerAsync, canAddService)
                .DisposeWith(Disposables);

            DeleteScannerCommand = ReactiveCommand
                .CreateFromTask<ScanServiceListItemViewModel>(ExecuteDeleteScannerAsync)
                .DisposeWith(Disposables);
        }


        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            try
            {
                if (parameters.ContainsKey(HubIdNavParam))
                {
                    var hubId = parameters.GetValue<string>(HubIdNavParam);

                    _hubInfo = await _hubService.GetCachedHubAsync(hubId, CancellationToken.None).ConfigureAwait(false);

                    var activeScannerViewModels = _hubInfo.ActiveServices
                        .Select(service => new ScanServiceListItemViewModel(service));
                    var availableScannerViewModels = _hubInfo.ServiceDefinitions
                        .Select(
                            service => new ServiceDefinitionListItemViewModel(service));

                    RunOnUi(
                        () =>
                        {
                            ActiveScanners.Clear();
                            AvailableScanners.Clear();

                            ActiveScanners.AddRange(activeScannerViewModels);
                            AvailableScanners.AddRange(availableScannerViewModels);
                        });
                }
                else
                {
                    if (ActiveScanners?.Count > 0)
                    {
                        foreach (var scannerViewModel in ActiveScanners)
                        {
                            scannerViewModel.UpdateFromModel();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, nameof(OnNavigatedTo));
            }
        }

        public ObservableCollection<ScanServiceListItemViewModel> ActiveScanners
        {
            get => _activeScanners;
            set => this.RaiseAndSetIfChanged(ref _activeScanners, value);
        }

        public ReactiveCommand<Unit, Unit> AddScannerCommand { get; }

        public ReactiveCommand<ScanServiceListItemViewModel, Unit> DeleteScannerCommand { get; }

        public ObservableCollection<ServiceDefinitionListItemViewModel> AvailableScanners
        {
            get => _availableScanners;
            set => this.RaiseAndSetIfChanged(ref _availableScanners, value);
        }

        public string HubName
        {
            get => _hubName;
            set => this.RaiseAndSetIfChanged(ref _hubName, value);
        }

        public ServiceDefinitionListItemViewModel SelectedScannerType
        {
            get => _selectedScannerType;
            set => this.RaiseAndSetIfChanged(ref _selectedScannerType, value);
        }

        public string Status
        {
            get => _status;
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        public ReactiveCommand<ScanServiceListItemViewModel, Unit> ViewSelectedServiceCommand { get; }

        private void AddServiceViewModel(ServiceInfo serviceInfo)
        {
            if (serviceInfo == null)
            {
                return;
            }

            RunOnUi(
                () => { ActiveScanners.Add(new ScanServiceListItemViewModel(serviceInfo)); });
        }

        private ServiceInfo CreateActiveServiceFromType(ServiceDefinition serviceDefinition)
        {
            var serviceInfo = new ServiceInfo
            {
                Id = Guid.NewGuid().ToString(),
                ServiceName = serviceDefinition.Name,
                ServiceTypeId = serviceDefinition.Id,
                Properties = serviceDefinition.Properties
            };

            return serviceInfo;
        }

        private async Task ExecuteAddScannerAsync(CancellationToken cancellationToken)
        {
            //TODO: Evaluate if needed with command.canexecute gaurd in place; else message user.
            if (SelectedScannerType == null)
            {
                return;
            }

            var activeService = CreateActiveServiceFromType(SelectedScannerType.ServiceDefinition);
            _hubInfo.ActiveServices.Add(activeService);

            var result = await _hubService.UpdateHubAsync(_hubInfo, cancellationToken).ConfigureAwait(false);

            if (result)
            {
                AddServiceViewModel(activeService);
            }
        }

        private async Task ExecuteDeleteScannerAsync(ScanServiceListItemViewModel scanServiceViewModel,
            CancellationToken cancellationToken)
        {
            _hubInfo.ActiveServices.Remove(scanServiceViewModel.ServiceInfo);

            var result = await _hubService.UpdateHubAsync(_hubInfo, cancellationToken).ConfigureAwait(false);

            if (result)
            {
                RunOnUi(() => { ActiveScanners.Remove(scanServiceViewModel); });
            }
        }

        private Task ExecuteViewSelectedServiceAsync(
            ScanServiceListItemViewModel scanService,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var navParams = new NavigationParameters
            {
                {
                    nameof(ServiceSettingsViewModel.HubIdNavParam),
                    _hubInfo.Id
                },
                {
                    nameof(ServiceSettingsViewModel.ServiceIdNavParam),
                    scanService.ServiceInfo.Id
                }
            };

            return NavigationService.NavigateAsync(nameof(ServiceSettingsPage), navParams);
        }
    }
}