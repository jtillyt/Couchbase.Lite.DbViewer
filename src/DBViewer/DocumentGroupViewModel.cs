using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DBViewer.Data;
using ReactiveUI;

namespace DBViewer
{
    public class DocumentGroupViewModel : ReactiveObject
    {
        private readonly DataService _dataService;

        private ObservableCollection<DocumentViewModel> _documents = new ObservableCollection<DocumentViewModel>();

        private string _groupName;

        public DocumentGroupViewModel(DataService dataService, string groupName, List<string> documentIds)
        {
            _dataService = dataService;

            GroupName = groupName;

            LoadDocuments(documentIds);
        }

        public string GroupName
        {
            get => _groupName;
            set => this.RaiseAndSetIfChanged(ref _groupName, value, nameof(GroupName));
        }

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