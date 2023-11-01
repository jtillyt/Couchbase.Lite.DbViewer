namespace CouchbaseLite.Browser.Data
{
	public class LoadedDatabase
	{
		public IReadOnlyList<DatabaseDocument> DatabaseDocuments { get; set; }

		public string ErrorMessage {get; set; }
	}
}
