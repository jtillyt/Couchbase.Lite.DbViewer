using AppKit;
using Foundation;
using ReactiveUI;

namespace DbViewer.Hub.macOs
{
    public class StatusBarController : NSObject
    {
        readonly NSStatusBar _statusBar;
        readonly NSStatusItem _statusItem;
        NSStatusBarButton _button;
        NSPopover _popOver;
        EventMonitor _eventMonitor;
        NSWindow _aboutWindow;
        NSStoryboard _storyboard;
        // NSWindowController _windowController;

        public StatusBarController()
        {
            _statusBar = new NSStatusBar();
            _statusItem = _statusBar.CreateStatusItem(NSStatusItemLength.Variable);
            _popOver = new NSPopover();
            // ViewController.QuitButtonClicked += HandleQuitButtonClicked;
            // ViewController.AboutMenuItemClicked += HandleAboutMenuItemClicked;
			_storyboard = NSStoryboard.FromName("Main", null);
			// _windowController = _storyboard.InstantiateControllerWithIdentifier("AboutWindow") as NSWindowController;
		}

        ~StatusBarController()
        {
            // ViewController.QuitButtonClicked -= HandleQuitButtonClicked;
            // ViewController.AboutMenuItemClicked -= HandleAboutMenuItemClicked;
        }

        /// <summary>
        /// Initialise a NSStatusItem instance with an image, popover and event handling.
        /// </summary>
        /// <param name="imageFileName">Image file name.</param>
        /// <param name="popOver">Pop over.</param>
        public void InitStatusBarItem(string imageFileName, NSPopover popOver)
        {
			_button = _statusItem.Button;
            NSImage image = new NSImage(imageFileName)
			{
				Template = true
			};
			_button.Image = image;
			_button.Action = new ObjCRuntime.Selector("toggle:");
			_button.Target = this;

            _popOver = popOver;

			_eventMonitor = new EventMonitor(NSEventMask.LeftMouseDown | NSEventMask.RightMouseDown, MouseEventHandler);
			_eventMonitor.Start();
		}

		[Export ("toggle:")]
		void Toggle(NSObject sender)
		{
            if (_popOver.Shown)
                Close(sender);
            else Show(sender);
		}

        /// <summary>
        /// Shows the popover
        /// </summary>
        /// <param name="sender">Sender.</param>
		public void Show(NSObject sender)
		{
		    _button = _statusItem.Button;
		    _popOver.Show(_button.Bounds, _button, NSRectEdge.MaxYEdge);
		    _eventMonitor.Start();
		}

        /// <summary>
        /// Hides the popover
        /// </summary>
        /// <param name="sender">Sender.</param>
		public void Close(NSObject sender)
		{
		    _popOver.PerformClose(sender);
		    _eventMonitor.Stop();
		}

		void MouseEventHandler(NSEvent @event)
		{
		    if (_popOver.Shown)
		        Close(@event);
		}

        void HandleQuitButtonClicked(object sender, System.EventArgs e)
        {
            Close(sender as NSObject);
            var alert = new NSAlert()
            {
                MessageText = "Are you sure you want to Quit Ambar?"
            };
            alert.AddButton("Quit");
			alert.AddButton("Cancel");
			var retValue = alert.RunModal();
			if(retValue == 1000)
                NSApplication.SharedApplication.Terminate(sender as NSObject);
		}

        void HandleAboutMenuItemClicked(object sender, System.EventArgs e)
        {
            Close(sender as NSObject);
			//
   //          _aboutWindow = _windowController.Window;
   //          _aboutWindow.Title = "";
   //          _aboutWindow.TitlebarAppearsTransparent = true;
			// _aboutWindow.MovableByWindowBackground = true;
   //
   //          _windowController.ShowWindow(sender as NSObject);
        }
    }
    
    public class EventMonitor
    {
        NSObject _monitor;
        NSEventMask _mask;
        GlobalEventHandler _handler;

        public EventMonitor()
        {
            
        }

        public EventMonitor(NSEventMask mask, GlobalEventHandler handler)
        {
            _mask = mask;
            _handler = handler;
        }

        //Destructor
        ~ EventMonitor()
        {
            Stop(); 
        }

        /// <summary>
        /// Start monitoring events of a given mask.
        /// </summary>
        public void Start()
        {
            _monitor = NSEvent.AddGlobalMonitorForEventsMatchingMask(_mask, _handler) as NSObject;
        }

        /// <summary>
        /// Stop monitoring event and release the resources.
        /// </summary>
        public void Stop()
        {
            if (_monitor != null)
            {
                NSEvent.RemoveMonitor(_monitor);
                _monitor = null;
            }
        }
    }
}