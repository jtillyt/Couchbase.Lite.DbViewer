using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using Couchbase.Lite;
using Dawn;
using DBViewer.Models;
using DBViewer.Services;
using DynamicData;
using DynamicData.Binding;

namespace DBViewer.ViewModels
{
    public class DocumentGroupViewModel : ObservableCollection<DocumentViewModel>
    {
        private readonly CachedDatabase _database;

        public DocumentGroupViewModel(CachedDatabase database, string groupName, List<string> documentIds, string[] searchStrings = null)
        {
            _database = Guard.Argument(database, nameof(database))
                  .NotNull()
                  .Value;

            GroupName = Guard.Argument(groupName, nameof(groupName))
                  .NotNull()
                  .Value;

            LoadDocuments(documentIds, searchStrings);
        }

        private string _groupName = "";

        public string GroupName
        {
            get { return _groupName; }
            set
            {
                if (_groupName != value)
                {
                    _groupName = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(GroupName)));
                }
            }
        }

        public void LoadDocuments(List<string> documentIds, string[] searchStrings)
        {
            foreach (var documentId in documentIds)
            {
                if (ShouldShowDocument(documentId, searchStrings))
                {
                    var documentViewModel = new DocumentViewModel(this, _database, documentId);
                    Add(documentViewModel);
                }
            }
        }

        private static bool ShouldShowDocument(string documentId, string[] searchStrings)
        { 
            if (searchStrings == null || searchStrings[0] == string.Empty)
                return true;

            return searchStrings.Any(searchString => documentId.ToLowerInvariant().Contains(searchString.ToLowerInvariant()));
        }
    }

    public class DocumentGroupModel : ObservableCollectionExtended<DocumentModel>, IDisposable
    {
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        public DocumentGroupModel(IGroup<DocumentModel, string, string> grouping, string key)
        {
            GroupName = key;
            grouping
                .Cache
                .Connect()
                .Bind(this)
                .Subscribe()
                .DisposeWith(_compositeDisposable);
        }

        public string GroupName { get; set; }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}