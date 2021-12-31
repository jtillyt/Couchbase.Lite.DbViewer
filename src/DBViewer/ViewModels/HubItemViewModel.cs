using Dawn;
using DbViewer.Shared.Dtos;
using ReactiveUI;

namespace DbViewer.ViewModels
{
    public class HubItemViewModel : ReactiveObject
    {
        public HubItemViewModel(HubInfo hubInfo)
        {
            Guard.Argument(hubInfo, nameof(hubInfo))
                .NotNull();

            DisplayName = hubInfo.HubName;
            HostAddress = hubInfo.HostAddress;
            HubId = hubInfo.Id;
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