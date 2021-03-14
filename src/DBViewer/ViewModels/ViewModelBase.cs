using ReactiveUI;
using System;
using Xamarin.Forms;

namespace DBViewer.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        protected void RunOnUi(Action action)
        {
            // TODO: <James Thomas: 3/14/21> Move to Di
            Device.InvokeOnMainThreadAsync(action);
        }
    }
}
