using DbViewer.Shared;
using DbViewer.Shared.Configuration;
using Refit;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DbViewer.Api
{
    public interface IDbHubHttpClient
    {
        [Get("/Hubs")]
        Task<HubInfo> GetHubInfo();

        [Get("/Databases")]
        Task<IEnumerable<DatabaseInfo>> ListAll();
        
        [Get("/Databases/Name/{displayName}")]
        Task<HttpResponseMessage> GetDatabase(string displayName);

        [Put("/Hubs/Update/{hubInfo}")]
        Task UpdateHub(HubInfo hubInfo);
    }
}
