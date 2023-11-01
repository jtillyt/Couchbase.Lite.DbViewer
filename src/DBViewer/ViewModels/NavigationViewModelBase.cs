using System.Diagnostics;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Prism.Navigation;
using ReactiveUI;

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

        private async Task ExecuteBackAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await NavigationService.GoBackAsync().ConfigureAwait(false);

            if (!result.Success)
            {
                Debugger.Break();
            }
        }
    }
}
