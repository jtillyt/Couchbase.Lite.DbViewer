using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Couchbase.Lite;
using Couchbase.Lite.Query;
using Dawn;

namespace DBViewer.Data
{
    public class DataService
    {
        private Database _database;

        public bool Connect(string dbDirectory, string dbName)
        {
            Guard.Argument(dbDirectory, nameof(dbDirectory))
                 .NotNull()
                 .NotEmpty();

            if (!Directory.Exists(dbDirectory))
            {
                throw new DirectoryNotFoundException(dbDirectory);
            }

            var dbConfig = new DatabaseConfiguration();
            dbConfig.Directory = dbDirectory;

            _database = new Database(dbName, dbConfig);

            return true;
        }

        public List<string> ListAllDocumentIds()
        {
            var documentIds = GetAllDocumentsIds(_database);

            if (documentIds == null)
                return new List<string>();

            return documentIds.ToList();
        }

        private void TraverseDoc(Document doc, Action<object> objAction)
        {
            Console.WriteLine($"__--__ Document {doc.Id} __--__");

            foreach (var kvp in doc)
            {
                TraverseObject(kvp.Value, objAction);
            }
        }

        private void TraverseObject(object obj, Action<object> objAction)
        {
            if (obj is ArrayObject objArray)
            {
                foreach (var child in objArray)
                {
                    TraverseObject(child, objAction);
                }
            }
            else
            {
                objAction(obj);
            }
        }

        private IEnumerable<string> GetAllDocumentsIds(Database db)
        {
            return QueryBuilder
                .Select((ISelectResult)SelectResult.Expression(Meta.ID),
                    (ISelectResult)SelectResult.Property("Type")).From(DataSource.Database(db)).Execute()
                .Select(i => i.GetString("id"))
                .Where(docId => docId != null).ToList();
        }

        public Document GetDocumentById(string id)
        {
            return _database.GetDocument(id);
        }
    }
}