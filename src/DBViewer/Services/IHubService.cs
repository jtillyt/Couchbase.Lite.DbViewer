using DbViewer.Shared;
using DbViewer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DbViewer.Shared.Configuration;
using DbViewer.Api;

namespace DbViewer.Services
{
    public interface IHubService
    {
        IDbHubHttpClient GetConnection(Uri hubUri);

        Task<HubInfo> TryAddHubAsync(Uri hubUri);

        Task<HubInfo> GetCachedHubAsync(string hubId);

        Task<IEnumerable<DatabaseInfo>> ListAllHubDatabasesAsync(Uri hubUri);

        Task<DownloadResult> DownloadDatabaseAsync(Uri hubUri, DatabaseInfo databaseInfo);

        Task<List<HubInfo>> ListAllKnownHubsAsync();

        Task<bool> UpdateHubAsync(HubInfo hubInfo);
    }
}