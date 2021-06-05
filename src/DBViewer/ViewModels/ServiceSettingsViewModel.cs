using Dawn;
using DbViewer.Services;
using DbViewer.Shared.Configuration;
using DynamicData;
using Prism.Navigation;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace DbViewer.ViewModels
{
    public class ServiceSettingsViewModel : NavigationViewModelBase, INavigatedAware
    {
        public const string ServiceId_NavParam = nameof(ServiceId_NavParam);
        public const string HubId_NavParam = nameof(HubId_NavParam);

        private readonly IHubService _hubService;

        public ServiceSettingsViewModel(IHubService hubService, INavigationService navigationService)
            : base(navigationService)
        {
            _hubService = Guard
                .Argument(hubService, nameof(hubService))
                .NotNull()
                .Value;

            SaveServiceCommand = ReactiveCommand.CreateFromTask(ExecuteSaveService);
        }

        public ReactiveCommand<Unit, Unit> SaveServiceCommand { get; }

        private string _serviceName;
        public string ServiceName
        {
            get => _serviceName;
            set => this.RaiseAndSetIfChanged(ref _serviceName, value);
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public ObservableCollection<ServicePropertyViewModel> ServiceProperties
        {
            get => _serviceProperties;
            set => this.RaiseAndSetIfChanged(ref _serviceProperties, value);
        }

        public async void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.ContainsKey(ServiceId_NavParam) && parameters.ContainsKey(HubId_NavParam))
            {
                var serviceId = parameters.GetValue<string>(ServiceId_NavParam);
                var hubId = parameters.GetValue<string>(HubId_NavParam);

                _hubInfo = await _hubService.GetCachedHubAsync(hubId);

                _serviceInfo = _hubInfo.ActiveServices.FirstOrDefault(service => service.Id == serviceId);
                var servicePropVms = _serviceInfo.Properties.Select(prop => new ServicePropertyViewModel(prop));

                if (_serviceInfo != null)
                {
                    RunOnUi(() =>
                    {
                        ServiceName = _serviceInfo.ServiceName;
                        ServiceProperties.AddRange(servicePropVms);
                    });
                }
            }
        }

  

        private async Task ExecuteSaveService()
        {
            var serviceProperties = ServiceProperties.Select(propVm => propVm.ServiceProperty);

            _serviceInfo.Properties = serviceProperties.ToList();

            var wasSuccessful = await _hubService.UpdateHubAsync(_hubInfo);

            if (wasSuccessful)
            {
                _ = await NavigationService.GoBackAsync();
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