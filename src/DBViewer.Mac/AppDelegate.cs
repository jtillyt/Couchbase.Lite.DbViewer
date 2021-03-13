using AppKit;
using DryIoc;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace DBViewer.Mac
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        public AppDelegate()
        {
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

            var rect = new CoreGraphics.CGRect(200, 200, 800, 600);
            MainWindow = new NSWindow(rect, style, NSBackingStore.Buffered, false);
            MainWindow.Title = "DB Viewer";
        }

        public override NSWindow MainWindow {get; }

        public override void DidFinishLaunching(NSNotification notification)
        {
            Forms.Init();

            LoadApplication(new App());
            // Insert code here to initialize your application
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }
}