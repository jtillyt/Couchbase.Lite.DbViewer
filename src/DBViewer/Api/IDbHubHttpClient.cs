using DbViewer.Shared;
using Refit;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DbViewer.Api
{
    public interface IDbHubHttpClient
    {
        [Get("/Databases")]
        Task<IEnumerable<DatabaseInfo>> ListAll();
        
        [Get("/Databases/Name/{displayName}")]
        Task<HttpResponseMessage> GetDatabase(string displayName);
    }
}
