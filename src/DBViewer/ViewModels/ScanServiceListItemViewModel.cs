using Dawn;
using DbViewer.Shared;
using DbViewer.Shared.Dtos;
using ReactiveUI;

namespace DbViewer.ViewModels
{
    public class ScanServiceListItemViewModel : ReactiveObject
    {
        public ScanServiceListItemViewModel(ServiceInfo serviceInfo)
        {
            ServiceInfo = Guard.Argument(serviceInfo, nameof(serviceInfo))
                              .NotNull()
                              .Value;

            DisplayName = ServiceInfo.ServiceName;

            //TODO: This is fragile but needed in the short term
            if (ServiceInfo.ServiceTypeId == ServiceConstants.LocalDatabaseScannerServiceTypeId)
            {
                Description = serviceInfo.Properties[0].Value;
            }
        }

        public ServiceInfo ServiceInfo { get; }

        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }
    }
}
