using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Dawn;
using DBViewer.Services;

namespace DBViewer.ViewModels
{
    public class DocumentGroupViewModel : ObservableCollection<DocumentViewModel>
    {
        private readonly IDatabaseService _dataService;

        private ObservableCollection<DocumentViewModel> _documents = new ObservableCollection<DocumentViewModel>();

        public DocumentGroupViewModel(IDatabaseService dataService, string groupName, List<string> documentIds)
        {
            _dataService = Guard.Argument(dataService, nameof(dataService))
                  .NotNull()
                  .Value;

            GroupName = Guard.Argument(groupName, nameof(groupName))
                  .NotNull()
                  .Value;

            LoadDocuments(documentIds);
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

        public void LoadDocuments(List<string> documentIds)
        {
            foreach (var documentId in documentIds)
            {
                var documentViewModel = new DocumentViewModel(this, _dataService, documentId);
                Add(documentViewModel);
            }
        }
    }
}