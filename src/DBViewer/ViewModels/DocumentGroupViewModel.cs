using DbViewer.Extensions;
using DbViewer.Models;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System;
using System.Collections.Generic;
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

        public class DocumentGroupViewModelComparer : IComparer<DocumentGroupViewModel>
        {
            public int Compare(DocumentGroupViewModel x, DocumentGroupViewModel y)
            {
                if (x == y) return 0;
                if (x?.GroupName == null || y?.GroupName == null) return 1;

                return string.Compare(x.GroupName, y.GroupName, true);
            }
        }
    }
}