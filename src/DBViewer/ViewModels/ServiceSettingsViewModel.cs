using Dawn;
using DbViewer.Services;
using DbViewer.Shared.Dtos;
using DynamicData;
using Prism.Navigation;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;

namespace DbViewer.ViewModels
{
    public class ServiceSettingsViewModel : NavigationViewModelBase, INavigatedAware
    {
        public const string ServiceIdNavParam = nameof(ServiceIdNavParam);
        public const string HubIdNavParam = nameof(HubIdNavParam);

        private readonly ILogger _logger = Log.ForContext<ServiceSettingsViewModel>();
        private readonly IHubService _hubService;

        private string _serviceName;

        public ServiceSettingsViewModel(IHubService hubService, INavigationService navigationService)
               : base(navigationService)
        {
            _hubService = Guard
                .Argument(hubService, nameof(hubService))
                .NotNull()
                .Value;

            SaveServiceCommand = ReactiveCommand.CreateFromTask(ExecuteSaveServiceAsync);
        }

        public ReactiveCommand<Unit, Unit> SaveServiceCommand { get; }

        public ObservableCollection<ServicePropertyViewModel> ServiceProperties
        {
            get => _serviceProperties;
            set => this.RaiseAndSetIfChanged(ref _serviceProperties, value);
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            try
            {
                if (parameters.ContainsKey(ServiceIdNavParam) && parameters.ContainsKey(HubIdNavParam))
                {
                    var serviceId = parameters.GetValue<string>(ServiceIdNavParam);
                    var hubId = parameters.GetValue<string>(HubIdNavParam);

                    _hubInfo = await _hubService.GetCachedHubAsync(hubId, CancellationToken.None).ConfigureAwait(false);

                    _serviceInfo = _hubInfo.ActiveServices.FirstOrDefault(service => string.Equals(service.Id, serviceId));
                    var servicePropVms = _serviceInfo.Properties.Select(prop => new ServicePropertyViewModel(prop));

                    if (_serviceInfo != null)
                    {
                        RunOnUi(
                        () =>
                        {
                            ServiceName = _serviceInfo.ServiceName;
                            ServiceProperties.AddRange(servicePropVms);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, nameof(OnNavigatedTo));
            }
        }

        public string ServiceName { get => _serviceName; set => this.RaiseAndSetIfChanged(ref _serviceName, value); }

        private async Task ExecuteSaveServiceAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var serviceProperties = ServiceProperties.Select(propVm => propVm.ServiceProperty);

            _serviceInfo.Properties = serviceProperties.ToList();

            var wasSuccessful = await _hubService.UpdateHubAsync(_hubInfo, cancellationToken).ConfigureAwait(false);

            if (wasSuccessful)
            {
                cancellationToken.ThrowIfCancellationRequested();

                RunOnUi(() =>
                {
                    NavigationService.GoBackAsync().ConfigureAwait(false);
                });
            }
            else
            {
                // TODO: <James Thomas: 6/2/21> Handle and notify user 
            }
        }

        private ServiceInfo _serviceInfo;
        private HubInfo _hubInfo;

        private ObservableCollection<ServicePropertyViewModel> _serviceProperties =
            new ObservableCollection<ServicePropertyViewModel>();
    }
}