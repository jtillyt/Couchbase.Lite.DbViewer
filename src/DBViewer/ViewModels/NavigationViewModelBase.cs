using Prism.Navigation;
using ReactiveUI;
using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;

namespace DbViewer.ViewModels
{
    public class NavigationViewModelBase : ViewModelBase
    {
        public NavigationViewModelBase(INavigationService navigationService)
        {
            NavigationService = navigationService;

            BackCommand = ReactiveCommand.CreateFromTask(ExecuteBackAsync);
        }

        public ReactiveCommand<Unit, Unit> BackCommand { get; private set; }

        protected INavigationService NavigationService { get; }

        private async Task ExecuteBackAsync()
        {
            var result = await NavigationService.GoBackAsync();

            if (!result.Success)
            {
                Debugger.Break();
            }
        }
    }
}
