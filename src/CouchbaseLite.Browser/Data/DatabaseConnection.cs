using Couchbase.Lite.Query;
using Couchbase.Lite;
using System;
using Dawn;

namespace CouchbaseLite.Browser.Data
{
	public class DatabaseConnection 
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

			var dbConfig = new DatabaseConfiguration
			{
				Directory = dbDirectory
			};

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

		public List<string> FindDocumentIds()
		{
			var documentIds = GetAllDocumentsIds(_database);

			if (documentIds == null)
				return new List<string>();

			return documentIds.ToList();
		}

		private IEnumerable<string> GetAllDocumentsIds(Database db)
		{
			if (db == null)
			{
				return Enumerable.Empty<string>();
			}

			var docIds = QueryBuilder
				.Select(SelectResult.Expression(Meta.ID),
					SelectResult.Property("Type")).From(DataSource.Database(db)).Execute()
				.Select(i => i.GetString("id"))
				.Where(docId => docId != null).ToList();

			return docIds;
		}

		public Database Database => _database;

		public IDataSourceAs ActiveSource => DataSource.Database(_database);

		public Document GetDocumentById(string id)
		{
			return _database.GetDocument(id);
		}

		public void SaveDocument(MutableDocument document)
		{
			_database.Save(document);
		}

		public void Compact()
		{
			_database.PerformMaintenance(MaintenanceType.Compact);
		}

		public void DeleteDocumentById(string id)
		{
			var document = GetDocumentById(id);

			if (document != null)
			{
				_database.Delete(document);
			}
		}
	}
}
