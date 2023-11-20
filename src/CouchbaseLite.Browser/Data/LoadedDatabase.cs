namespace CouchbaseLite.Browser.Data
{
	public class LoadedDatabase
	{
		public IReadOnlyList<DatabaseDocumentGroup> DatabaseDocumentGroups { get; set; }

		public IReadOnlyList<DatabaseDocument> DatabaseDocuments { get; set; }

		public string ErrorMessage {get; set; }

		public int ResultCount = 0;
	}
}
