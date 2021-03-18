using Couchbase.Lite;
using System.Collections.Generic;

namespace DBViewer.Services
{
    public interface IDatabaseService
    {
        bool Connect(string dbDirectory, string dbName);
        List<string> ListAllDocumentIds();
        Document GetDocumentById(string id);
        bool Disconnect();
    }
}
