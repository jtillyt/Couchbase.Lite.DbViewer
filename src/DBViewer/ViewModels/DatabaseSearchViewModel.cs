using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using DbViewer.DataStores;
using DbViewer.Extensions;
using DbViewer.Models;
using DbViewer.Views;
using DynamicData;
using Newtonsoft.Json;
using Prism.Navigation;
using ReactiveUI;
using Couchbase.Lite;
using MoonSharp.Interpreter;

namespace DbViewer.ViewModels
{
    public class DatabaseSearchViewModel : NavigationViewModelBase, INavigatedAware
    {
        public DatabaseSearchViewModel(IDatabaseDatastore cacheService, INavigationService navigationService)
            : base(navigationService)
        {
            _cacheService = Guard
                .Argument(cacheService, nameof(cacheService))
                .NotNull()
                .Value;

            SearchCommand = ReactiveCommand.CreateFromTask(ExecuteSearchAsync, outputScheduler: RxApp.MainThreadScheduler);

            _documentCache
               .Connect()
               .RefCount()
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _searchResults)
               .LogManagedThread("Search - After Bind")
               .Subscribe()
               .DisposeWith(Disposables);

            ViewSelectedDocumentCommand = ReactiveCommand.Create<DocumentModel>(ExecuteViewSelectedDocument);

            this.WhenAnyValue(vm => vm.UseAdvancedSearch)
                .Select(val => val ? "JMES Query (ex.. FieldName=='val'&&FieldName2=='val2')" : "Search Text")
                .BindTo(this, x => x.PlaceholderText)
                .DisposeWith(Disposables);
        }

        public ReactiveCommand<DocumentModel, Unit> ViewSelectedDocumentCommand { get; }

        public ReactiveCommand<Unit, Unit> SearchCommand { get; }

        public string SearchTitle
        {
            get => _searchTitle;
            set => this.RaiseAndSetIfChanged(ref _searchTitle, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        public bool UseAdvancedSearch
        {
            get => _useAdvancedSearch;
            set => this.RaiseAndSetIfChanged(ref _useAdvancedSearch, value);
        }

        public string PlaceholderText
        {
            get => _placeHolderText;
            set => this.RaiseAndSetIfChanged(ref _placeHolderText, value);
        }

        public double Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        public ReadOnlyObservableCollection<DocumentModel> SearchResults => _searchResults;

        private CachedDatabaseItemViewModel _currentDatabaseItemViewModel;
        public CachedDatabaseItemViewModel CurrentDatabaseItemViewModel
        {
            get => _currentDatabaseItemViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentDatabaseItemViewModel, value);
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            if (parameters.ContainsKey(nameof(CachedDatabaseItemViewModel)))
            {
                CurrentDatabaseItemViewModel = parameters.GetValue<CachedDatabaseItemViewModel>(nameof(CachedDatabaseItemViewModel));
            }
            if (parameters.ContainsKey(DocumentIdListParam))
            {
                _documentIdsToSearch = parameters.GetValue<IEnumerable<string>>(nameof(DocumentIdListParam));
            }
        }

        private async Task ExecuteSearchAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (CurrentDatabaseItemViewModel == null)
            {
                return;
            }

            var documentIdsWithHits = await SearchDbAsync(SearchText, cancellationToken).ConfigureAwait(false);

            var docModels = documentIdsWithHits.Select(docId => new DocumentModel(CurrentDatabaseItemViewModel.Database, docId))
                                               .OrderBy(vm => vm.DocumentId);

            RunOnUi(() =>
            {
                _documentCache.Edit((editor) =>
                {
                    editor.Clear();
                    editor.AddOrUpdate(docModels);
                });
            });
        }

        private Task<List<string>> SearchDbAsync(string searchText, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                UpdateProgress(0);

                var documentIdsWithHits = new List<string>();

                var database = CurrentDatabaseItemViewModel.Database;
                var isConnected = database.ConnectToRemote();

                if (!isConnected)
                {
                    return documentIdsWithHits;
                }

                var connection = database.ActiveConnection;

                var documentIds = connection.ListAllDocumentIds();
                var totalDocCount = Math.Min(documentIds.Count, MaxResults);

                //try
                //{
                //    using (var document = connection.GetDocumentById(documentIds[0]))
                //    {
                //        var cleanedDocument = document.CleanAttachments();
                //        var copileResult = UseScript(searchText, cleanedDocument);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Debug.WriteLine(ex);
                //}

                //return new List<string>();

                var count = 0;
                for (int i = 0; i < documentIds.Count; i++)
                {
                    var documentId = documentIds[i];

                    cancellationToken.ThrowIfCancellationRequested();

                    using (var document = connection.GetDocumentById(documentId))
                    {

                        try
                        {
                            //var copileResult = UseScript(searchText, document);
                        }
                        catch (Exception ex)
                        {
                        }

                        var documentText = string.Empty;

                        try
                        {
                            var cleanedDocument = document.CleanAttachments();
                            documentText = JsonConvert.SerializeObject(cleanedDocument);

                        }
                        catch (Exception ex)
                        {

                        }

                        try
                        {
                            if (IsMatch(documentText, searchText))
                            {
                                documentIdsWithHits.Add(documentId);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Search cancelled due to exception");

                            UpdateProgress(0);

                            break;
                        }
                        finally
                        {
                            count++;
                            UpdateProgress(count / totalDocCount);
                        }
                    }

                    if (documentIdsWithHits.Count >= MaxResults)
                    {
                        break;
                    }
                }

                return documentIdsWithHits;
            });
        }

        private bool UseScript(string query, Document document)
        {
            UserData.RegisterType<ScriptDocument>();

            var cleanedQuery = query;

            if (!query.StartsWith("return "))
            {
                cleanedQuery = "return " + cleanedQuery;
            }

            Script script = new Script();
            DynValue obj = UserData.Create(new ScriptDocument(document));

            script.Globals["obj"] = obj;

            DynValue res = script.DoString(cleanedQuery);

            return res.Boolean;
        }

        private bool IsMatch(string documentText, string searchText)
        {
            return UseAdvancedSearch ? ContainsAdvanced(documentText, searchText) : ContainsSimple(documentText, searchText);
        }

        private bool ContainsSimple(string documentText, string searchText)
        {
            var searchTextCorrected = searchText.ToLower();
            return documentText.ToLower().Contains(searchTextCorrected);
        }

        private bool ContainsAdvanced(string documentText, string searchText)
        {
            try
            {
                ////https://jmespath.org/specification.html#id16
                ////https://github.com/jdevillard/JmesPath.Net
                //var jmes = new JmesPath();

                //var ast = jmes.Parse(searchText);

                //var result = jmes.Transform(documentText, searchText);

                //if (result == null)
                //{
                //    return false;
                //}

                //if (bool.TryParse(result, out bool boolValue))
                //{
                //    return boolValue;
                //}

                return true;
                //return !string.IsNullOrWhiteSpace(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error with advanced search: {ex}");
                throw;
            }
        }



        private void ExecuteViewSelectedDocument(DocumentModel document)
        {
            var navParams = new NavigationParameters
            {
                { nameof(DocumentModel), document }
            };

            RunOnUi(() =>
            {
                NavigationService.NavigateAsync(nameof(DocumentViewerPage), navParams);
            });
        }

        private void UpdateProgress(double progress)
        {
            RunOnUi(() =>
            {
                Progress = progress;
            });
        }

        internal const string DocumentIdListParam = nameof(DocumentIdListParam);
        private const int MaxResults = 500;

        private readonly IDatabaseDatastore _cacheService;
        private bool _useAdvancedSearch;

        private IEnumerable<string> _documentIdsToSearch;
        private string _searchTitle;
        private string _searchText = "";
        private string _placeHolderText = "Search Text";
        private double _progress = 0.0;

        private readonly ReadOnlyObservableCollection<DocumentModel> _searchResults;

        private readonly ISourceCache<DocumentModel, string> _documentCache =
         new SourceCache<DocumentModel, string>(x => x.DocumentId);
    }
}