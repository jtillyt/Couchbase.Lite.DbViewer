using Akavache;
using DbViewer.Configuration;
using DbViewer.Services;
using DbViewer.ViewModels;
using DbViewer.Views;
using DryIoc;
using Prism.Ioc;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DbViewer
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

            BlobCache.ApplicationName = AppInfo.Name;
            BlobCache.EnsureInitialized();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IConfigurationService,ConfigurationService>();
            containerRegistry.RegisterSingleton<IHubService,HubService>();
            containerRegistry.RegisterSingleton<IDatabaseCacheService, DatabaseCacheService>();
            containerRegistry.RegisterSingleton<IHubCacheService, HubCacheService>();

            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<CachedDatabaseListPage, CachedDatabaseListViewModel>();
            containerRegistry.RegisterForNavigation<DatabaseBrowserPage, DatabaseBrowserViewModel>();
            containerRegistry.RegisterForNavigation<DatabaseSearchPage, DatabaseSearchViewModel>();
            containerRegistry.RegisterForNavigation<DocumentViewerPage, DocumentViewerViewModel>();
            containerRegistry.RegisterForNavigation<HubListPage, HubListViewModel>();
            containerRegistry.RegisterForNavigation<HubDetailPage, HubDetailViewModel>();
            containerRegistry.RegisterForNavigation<HubSettingsPage, HubSettingsViewModel>();
            containerRegistry.RegisterForNavigation<ServiceSettingsPage, ServiceSettingsViewModel>();
        }
    }
}