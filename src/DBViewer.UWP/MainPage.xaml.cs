using DBViewer.Services;
using DryIoc;

namespace DBViewer.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();
            var app = new DBViewer.App(LoadPlatformServiceContainer());
            LoadApplication(app);
        }

        private IContainer LoadPlatformServiceContainer()
        {
            var container = new Container();

            container.Register<IDbCopyService, SshDbFetchService>(Reuse.Singleton);

            return container;
        }
    }
}
