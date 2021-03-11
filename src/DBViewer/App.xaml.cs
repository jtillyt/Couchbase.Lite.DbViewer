using Dawn;
using DBViewer.Configuration;
using DBViewer.Services;
using DryIoc;
using Xamarin.Forms;

namespace DBViewer
{
    public partial class App : Application
    {
        public IContainer ServiceContaner { get; }

        public App(IContainer serviceContaner)
        {
            InitializeComponent();


            ServiceContaner = Guard.Argument(serviceContaner, nameof(serviceContaner))
                              .NotNull()
                              .Value;

            LoadServices();

            MainPage = new MainPage(ServiceContaner);
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
            ServiceContaner.Register<IConfigurationService,ConfigurationService>();
        }
    }
}