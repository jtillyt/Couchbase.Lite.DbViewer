using DbViewer.Extensions;
using DbViewer.Models;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace DbViewer.ViewModels
{
    public class DocumentGroupViewModel : ObservableCollectionExtended<DocumentModel>, IDisposable
    {
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private readonly DocumentModel.Comparer _comparer = new DocumentModel.Comparer();

        public DocumentGroupViewModel(IGroup<DocumentModel, string, string> grouping, string key)
        {
            GroupName = key;

            var groupCollection = grouping
                .Cache
                .Connect()
                .RefCount();

            groupCollection.LogManagedThread("Group - Before Bind")
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeOn(RxApp.MainThreadScheduler)
                .Sort(_comparer)
                .Bind(this)
                .LogManagedThread("Group - AfterBind")
                .Subscribe()
                .DisposeWith(_compositeDisposable);
        }

        public string GroupName { get; private set; }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}