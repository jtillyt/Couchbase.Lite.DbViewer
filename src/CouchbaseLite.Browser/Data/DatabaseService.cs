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
					return new LoadedDatabase() { ErrorMessage = "Error loading database:" + ex.Message };
				}
			}

			var documentIds = connection.ListAllDocumentIds().OrderBy(x => x);

			var documentIdGroups = documentIds.GroupBy(x =>
			{
				var split = x.Split("::");
				return split.Length > 1 ? split[0] : "Ungrouped";

			});
			var documentGroups = new List<DatabaseDocumentGroup>();
			var documentList = new List<DatabaseDocument>();

			foreach (var idGroup in documentIdGroups)
			{
				var documentGroup = new DatabaseDocumentGroup();

				documentGroup.GroupName = idGroup.Key;

				foreach (var documentId in idGroup)
				{
					var doc = new DatabaseDocument() { DisplayName = documentId };

					documentGroup.Documents.Add(doc);
					documentList.Add(doc);
				}

				documentGroups.Add(documentGroup);
			}

			return new LoadedDatabase()
			{
				DatabaseDocuments = documentList,
				DatabaseDocumentGroups = documentGroups
			};
		}
	}
}