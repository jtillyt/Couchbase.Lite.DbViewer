using Dawn;
using DbViewer.Shared.Configuration;
using ReactiveUI;

namespace DbViewer.ViewModels
{
    public class HubItemViewModel : ReactiveObject
    {
        private readonly HubInfo _hubInfo;

        public HubItemViewModel(HubInfo hubInfo)
        {
            _hubInfo = Guard.Argument(hubInfo, nameof(hubInfo))
                              .NotNull()
                              .Value;


            DisplayName = _hubInfo.HubName;
            HostAddress = _hubInfo.HostAddress;
            HubId = _hubInfo.Id;
        }

        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }

        private string _hostAddress;
        public string HostAddress
        {
            get => _hostAddress;
            set => this.RaiseAndSetIfChanged(ref _hostAddress, value);
        }

        private string _hubId;
        public string HubId
        {
            get => _hubId;
            set => this.RaiseAndSetIfChanged(ref _hubId, value);
        }
    }
}
