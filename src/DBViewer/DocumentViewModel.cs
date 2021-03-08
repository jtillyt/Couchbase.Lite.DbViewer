using Couchbase.Lite;
using DBViewer.Data;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace DBViewer
{
    public class DocumentViewModel : ReactiveObject
    {
        private readonly DocumentGroupViewModel _parentViewModel;
        private readonly DataService _dataService;

        public DocumentViewModel(DocumentGroupViewModel documentGroupViewModel, DataService dataService, string documentId)
        {
            _parentViewModel = documentGroupViewModel;
            _dataService = dataService;

            DocumentId = documentId;
        }

        public Document Document { get; }

        private string _documentId;

        public string DocumentId
        {
            get { return _documentId; }
            set
            {
                this.RaiseAndSetIfChanged(ref _documentId, value, nameof(DocumentId));
            }
        }

        /// <summary>
        /// We should load the document and pass back so that tree nodes can be created
        /// </summary>
        /// <returns></returns>
        internal string GetJson()
        {
            var document = _dataService.GetDocumentById(DocumentId);
            return JsonConvert.SerializeObject(document, Formatting.Indented);
        }
    }
}
