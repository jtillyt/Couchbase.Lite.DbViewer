﻿// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using Foundation;

namespace DbViewer.Hub.macOs
{
    [Register("ViewController")]
    partial class ViewController
    {
        [Outlet]
        AppKit.NSButton SettingsButton { get; set; }

        [Outlet]
        AppKit.NSTextFieldCell titleText { get; set; }

        [Action ("SettingsButtonClick:")]
        partial void SettingsButtonClick (Foundation.NSObject sender);
		
        void ReleaseDesignerOutlets ()
        {
            if (titleText != null) {
                titleText.Dispose ();
                titleText = null;
            }

            if (SettingsButton != null) {
                SettingsButton.Dispose ();
                SettingsButton = null;
            }
        }
    }
}