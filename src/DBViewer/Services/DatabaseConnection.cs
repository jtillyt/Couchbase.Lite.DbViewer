using Couchbase.Lite;
using Couchbase.Lite.Query;
using Dawn;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DBViewer.Services
{
    public class DatabaseConnection : IDatabaseConnection
    {
        private Database _database;

        public bool IsConnected => _database != null;

        public bool Connect(string dbDirectory, string dbName)
        {
            Guard.Argument(dbDirectory, nameof(dbDirectory))
                 .NotNull()
                 .NotEmpty();

            if (!Directory.Exists(dbDirectory))
            {
                return false;
            }

            var dbConfig = new DatabaseConfiguration();
            dbConfig.Directory = dbDirectory;

            _database = new Database(dbName, dbConfig);

            return true;
        }

        public bool Disconnect()
        {
            if (_database?.Config == null)
                return true;

            _database.Close();
            _database = null;

            return true;
        }

        public List<string> ListAllDocumentIds()
        {
            var documentIds = GetAllDocumentsIds(_database);

            if (documentIds == null)
                return new List<string>();

            return documentIds.ToList();
        }

        private IEnumerable<string> GetAllDocumentsIds(Database db)
        {
           if (_database == null)
            {
                return Enumerable.Empty<string>();
            }

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