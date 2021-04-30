using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Dawn;
using DBViewer.Models;
using DBViewer.Services;

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

            foreach(var searchString in searchStrings)
            {
                if (documentId.ToLowerInvariant().Contains(searchString.ToLowerInvariant()))
                    return true;
            }

            return false;
        }
    }
}