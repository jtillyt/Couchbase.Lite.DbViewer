using DBViewer.Configuration;
using DBViewer.Services;
using DBViewer.ViewModels;
using DBViewer.Views;
using DryIoc;
using Prism.Ioc;
using Xamarin.Forms;

namespace DBViewer
{
    public partial class App
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void OnStart()
        {
        }
        
        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        protected override async void OnInitialized()
        {
            var result = await NavigationService.NavigateAsync(nameof(HubView));

            if (!result.Success)
            {
                System.Diagnostics.Debugger.Break();
            }
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IConfigurationService,ConfigurationService>();
            containerRegistry.Register<IHubService,HubService>();
            containerRegistry.Register<IDbCacheService, DbCacheService>();

            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<HubView, HubViewModel>();
        }
    }
}