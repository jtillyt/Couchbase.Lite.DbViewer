using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace DbViewer.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

            ApplicationView.PreferredLaunchViewSize = new Size(600,1080);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            var app = new DbViewer.App();
            LoadApplication(app);
        }
    }
}
