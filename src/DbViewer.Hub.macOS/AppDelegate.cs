using AppKit;
using Foundation;

namespace DbViewer.Hub.macOs
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        NSPopover popOver = new NSPopover();

        public AppDelegate()
        {
            /*caret*/
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            // Insert code here to initialize your application
            
            var storyboard = NSStoryboard.FromName("Main", null);
            var controller = storyboard.InstantiateControllerWithIdentifier("PopupController") as ViewController;

            popOver.ContentViewController = controller;

            StatusBarController statusBar = new StatusBarController();
            statusBar.InitStatusBarItem("lite-view.png", popOver);
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }
}