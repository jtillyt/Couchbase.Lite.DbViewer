using DBViewer.Services;
using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace DBViewer.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

            ApplicationView.PreferredLaunchViewSize = new Size(600,1080);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            var app = new DBViewer.App();
            LoadApplication(app);
        }
    }
}
