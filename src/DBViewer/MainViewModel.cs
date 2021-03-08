using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using DBViewer.Data;
using Newtonsoft.Json;
using ReactiveUI;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DBViewer
{
    public class MainViewModel : ReactiveObject
    {
        private readonly DataService _dataService;

        private string _databaseDirectory;

        private string _databaseName;

        private string _jsonText;

        private ObservableCollection<DocumentGroupViewModel> _rootNodes =
            new ObservableCollection<DocumentGroupViewModel>();

        private string _selectedDocumentId;

        public MainViewModel()
        {
            _dataService = new DataService();

            LoadDatabase = ReactiveCommand.Create(Load);
            DocumentSelected = ReactiveCommand.Create(OnDocumentSelected);

            ThrownExceptions.Subscribe(OnException);

            DatabaseDirectory = FileSystem.AppDataDirectory;
            DatabaseName = "DbName";
        }

        public ObservableCollection<DocumentGroupViewModel> RootNodes
        {
            get => _rootNodes;
            set => this.RaiseAndSetIfChanged(ref _rootNodes, value, nameof(RootNodes));
        }

        public ReactiveCommand<Unit, Unit> LoadDatabase { get; }
        public ReactiveCommand<Unit, Unit> DocumentSelected { get; }

        public string DatabaseDirectory
        {
            get => _databaseDirectory;
            set => this.RaiseAndSetIfChanged(ref _databaseDirectory, value, nameof(DatabaseDirectory));
        }

        public string DatabaseName
        {
            get => _databaseName;
            set => this.RaiseAndSetIfChanged(ref _databaseName, value, nameof(DatabaseName));
        }

        public string JsonText
        {
            get => _jsonText;
            set => this.RaiseAndSetIfChanged(ref _jsonText, value, nameof(JsonText));
        }

        public string SelectedDocumentId
        {
            get => _selectedDocumentId;
            set => this.RaiseAndSetIfChanged(ref _selectedDocumentId, value, nameof(SelectedDocumentId));
        }

        private void OnException(Exception obj)
        {
        }

        public void Load()
        {
            var result = _dataService.Connect(DatabaseDirectory, DatabaseName);

            if (!result)
                return;

            var documentIds = _dataService.ListAllDocumentIds();

            var groupedDocuments = documentIds.GroupBy(key => { return key.Substring(0, key.IndexOf("::")); });

            foreach (var group in groupedDocuments)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    var groupVm = new DocumentGroupViewModel(_dataService, group.Key, group.ToList());
                    RootNodes.Add(groupVm);
                });
            }
        }

        private void OnDocumentSelected()
        {
            var document = _dataService.GetDocumentById(SelectedDocumentId);

            var json = JsonConvert.SerializeObject(document, Formatting.Indented);

            JsonText = json;
        }
    }
}