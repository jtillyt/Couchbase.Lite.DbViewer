using System.Collections.Generic;
using System.Threading.Tasks;
using DbViewer.Shared.Configuration;

namespace DbViewer.Services
{
    public interface IHubCacheService
    {
        Task<List<HubInfo>> ListAll();
        
        Task SaveAll();

        Task SaveHub(HubInfo hub);

        Task DeleteHub(string hubId);

        Task<HubInfo> GetCachedHub(string hubId);
    }
}
