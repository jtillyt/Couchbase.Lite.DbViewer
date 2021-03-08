using DBViewer.Data;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DBViewer
{
    public class DocumentGroupViewModel : ReactiveObject
    {
        private readonly DataService _dataService;

        public DocumentGroupViewModel(DataService dataService, string groupName, List<string> documentIds)
        {
            _dataService = dataService;

            GroupName = groupName;

            LoadDocuments(documentIds);
        }

        private string _groupName;

        public string GroupName
        {
            get { return _groupName; }
            set { this.RaiseAndSetIfChanged(ref _groupName, value, nameof(GroupName)); }
        }

        private ObservableCollection<DocumentViewModel> _documents = new ObservableCollection<DocumentViewModel>();

        public ObservableCollection<DocumentViewModel> Documents
        {
            get => _documents;
            set => this.RaiseAndSetIfChanged(ref _documents, value, nameof(Documents));
        }

        public void LoadDocuments(List<string> documentIds)
        {
            foreach (var documentId in documentIds)
            {
                var documentViewModel = new DocumentViewModel(this, _dataService, documentId);
                Documents.Add(documentViewModel);
            }
        }
    }
}