using DBViewer.Configuration;
using DBViewer.Services;
using DryIoc;
using Xamarin.Forms;

namespace DBViewer
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var serviceContainer = LoadServices();

            MainPage = new MainPage(serviceContainer);
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

        private IContainer LoadServices()
        {
            var iocContaner = new Container();

            iocContaner.Register<IConfigurationService,ConfigurationService>();
            iocContaner.Register<IDbCopyService, SshDbFetchService>();

            return iocContaner;
        }
    }
}