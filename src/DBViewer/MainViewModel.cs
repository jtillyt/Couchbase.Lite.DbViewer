using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Dawn;
using DBViewer.Data;
using DBViewer.Services;
using Newtonsoft.Json;
using ReactiveUI;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DBViewer
{
    public class MainViewModel : ReactiveObject
    {
        private const string LastDbNameKey = "LastDbName";

        private readonly DataService _dataService;
        private readonly IDbFetchService _dbFetchService;

        private string _databaseDirectory;

        private string _databaseName;

        private string _jsonText;

        private ObservableCollection<DocumentGroupViewModel> _rootNodes =
            new ObservableCollection<DocumentGroupViewModel>();

        private string _selectedDocumentId;

        public MainViewModel(IDbFetchService dbFetchService)
        {
            _dataService = new DataService();
            _dbFetchService = Guard.Argument(dbFetchService, nameof(dbFetchService))
                  .NotNull()
                  .Value;

            LoadDatabase = ReactiveCommand.Create(ExecuteLoad);
            DocumentSelected = ReactiveCommand.Create(ExecuteDocumentSelected);
            FetchDatabases = ReactiveCommand.Create(ExecuteFetchDatabases);

            ThrownExceptions.Subscribe(OnException);

            DatabaseDirectory = FileSystem.AppDataDirectory;
            DatabaseName = Preferences.Get(LastDbNameKey, "DbName");
        }

        public ObservableCollection<DocumentGroupViewModel> RootNodes
        {
            get => _rootNodes;
            set => this.RaiseAndSetIfChanged(ref _rootNodes, value, nameof(RootNodes));
        }

        public ReactiveCommand<Unit, Unit> LoadDatabase { get; }

        public ReactiveCommand<Unit, Unit> FetchDatabases { get; }

        public ReactiveCommand<Unit, Unit> DocumentSelected { get; }

        public string DatabaseDirectory
        {
            get => _databaseDirectory;
            set => this.RaiseAndSetIfChanged(ref _databaseDirectory, value, nameof(DatabaseDirectory));
        }


        public string DatabaseName
        {
            get
            {
                return _databaseName;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref _databaseName, value, nameof(DatabaseName));
            }
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

        public void ExecuteLoad()
        {
            var result = _dataService.Connect(DatabaseDirectory, DatabaseName);

            if (!result)
                return;


            Preferences.Set(LastDbNameKey, DatabaseName);

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

        private void ExecuteDocumentSelected()
        {
            var document = _dataService.GetDocumentById(SelectedDocumentId);

            var json = JsonConvert.SerializeObject(document, Formatting.Indented);

            JsonText = json;
        }

        private void ExecuteFetchDatabases()
        {
            _dbFetchService.FetchDbToLocalPath(DatabaseDirectory);
        }
    }
}