using DbViewer.Shared;
using Refit;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DBViewer.Api
{
    public interface IDbHubHttpClient
    {
        [Get("/Databases/ListAllDbs")]
        Task<IEnumerable<DatabaseInfo>> ListAll();
        
        [Get("/Databases/Name/{displayName}")]
        Task<Stream> GetDatabase(string displayName);
    }
}
