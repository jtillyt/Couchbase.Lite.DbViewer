using Dawn;
using DbViewer.Shared.Configuration;
using ReactiveUI;

namespace DbViewer.ViewModels
{
    public class ServiceDefinitionListItemViewModel : ReactiveObject
    {
        public ServiceDefinitionListItemViewModel(ServiceDefinition serviceDefinition)
        {
            ServiceDefinition = Guard.Argument(serviceDefinition, nameof(serviceDefinition))
                              .NotNull()
                              .Value;


            DisplayName = ServiceDefinition.Name;
        }


        public ServiceDefinition ServiceDefinition { get; }

        public string DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }

        private string _displayName;
    }
}
