using System.Reflection.Metadata.Ecma335;

namespace CouchbaseLite.Browser.Data
{
	public class DatabaseService
	{
		private Dictionary<Guid, DatabaseConnection> _connections = new Dictionary<Guid, DatabaseConnection>();
		public LoadedDatabase LoadDb(DatabaseViewerProperties properties)
		{
			DatabaseConnection connection = null;

			if (!_connections.TryGetValue(properties.ViewerId, out connection))
			{
				connection = new DatabaseConnection();

				try
				{
					connection.Connect(properties.DatabasePath, "HHNext");
					_connections.Add(properties.ViewerId, connection);
				}
				catch (Exception ex)
				{
					return new LoadedDatabase(){ ErrorMessage = "Error loading database:" +  ex.Message};
				}
			}

			var documentIds = connection.ListAllDocumentIds();

			var documentList = new List<DatabaseDocument>();

			foreach ( var documentId in documentIds ) {

				documentList.Add(new DatabaseDocument(){ DisplayName = documentId });
			}

			return new LoadedDatabase() { DatabaseDocuments = documentList };
		}
	}
}