using DBViewer.Configuration;
using DBViewer.Services;
using Xamarin.Forms;

namespace DBViewer
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            LoadServices();

            MainPage = new MainPage();
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

        private void LoadServices()
        {
            DependencyService.Register<IConfigurationService,ConfigurationService>();
            DependencyService.Register<IDbFetchService, SshIosSimulatorDbFetchService>();
        }
    }
}