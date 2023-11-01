namespace CouchbaseLite.Browser.Data
{
	public class DatabaseDocumentGroup
	{
		public List<DatabaseDocument> Documents { get; set; } = new List<DatabaseDocument>();
		public string GroupName { get; set; }
	}
}
