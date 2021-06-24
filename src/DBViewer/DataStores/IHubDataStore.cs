using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DbViewer.Shared.Dtos;

namespace DbViewer.DataStores
{
    public interface IHubDatastore
    {
        Task<List<HubInfo>> ListAllAsync(CancellationToken cancellationToken);

        Task SaveAllAsync(CancellationToken cancellationToken);

        Task SaveHubAsync(HubInfo hub, CancellationToken cancellationToken);

        Task DeleteHubAsync(string hubId, CancellationToken cancellationToken);

        Task<HubInfo> GetCachedHubAsync(string hubId, CancellationToken cancellationToken);
    }
}
