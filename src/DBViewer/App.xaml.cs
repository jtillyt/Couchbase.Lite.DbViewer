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

            Application.Current.UserAppTheme = OSAppTheme.Light;
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
            var result = await NavigationService.NavigateAsync(nameof(CachedDatabaseListPage));

            if (!result.Success)
            {
                System.Diagnostics.Debugger.Break();
            }
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IConfigurationService,ConfigurationService>();
            containerRegistry.RegisterSingleton<IHubService,HubService>();
            containerRegistry.RegisterSingleton<IDatabaseCacheService, DatabaseCacheService>();
            containerRegistry.RegisterSingleton<IDatabaseService, DatabaseService>();

            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<CachedDatabaseListPage, CachedDatabaseListViewModel>();
            containerRegistry.RegisterForNavigation<DatabaseBrowserPage, DatabaseBrowserViewModel>();
            containerRegistry.RegisterForNavigation<DocumentViewerPage, DocumentViewerViewModel>();
            containerRegistry.RegisterForNavigation<HubPage, HubViewModel>();
        }
    }
}