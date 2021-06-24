using ReactiveUI;
using System;
using Xamarin.Forms;

namespace DbViewer.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        public System.Reactive.Disposables.CompositeDisposable Disposables { get; } = new System.Reactive.Disposables.CompositeDisposable();


        protected void RunOnUi(Action action)
        {
            // TODO: <James Thomas: 3/14/21> Move to Di
            Device.InvokeOnMainThreadAsync(action);
        }
    }
}
