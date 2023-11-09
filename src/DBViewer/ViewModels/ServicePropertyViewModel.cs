using Dawn;
using DbViewer.Shared.Dtos;
using ReactiveUI;

namespace DbViewer.ViewModels
{
    public class ServicePropertyViewModel : ReactiveObject
    {
        public ServicePropertyViewModel(ServicePropertyInfo servicePropertyInfo)
        {
            ServiceProperty = Guard.Argument(servicePropertyInfo, nameof(servicePropertyInfo))
                .NotNull()
                .Value;

            DisplayName = ServiceProperty.DisplayName;
            Value = ServiceProperty.Value;
            Description = ServiceProperty.Description;
        }

        public ServicePropertyInfo ServiceProperty { get; }

        private string _displayName;

        public string DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }

        private string _value;

        public string Value
        {
            get => _value;
            set
            {
                this.RaiseAndSetIfChanged(ref _value, value);

                if (!string.Equals(ServiceProperty.Value, value))
                {
                    ServiceProperty.Value = value;
                }
            }
        }

        private string _description;

        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }
    }
}