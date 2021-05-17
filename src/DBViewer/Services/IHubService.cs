using DbViewer.Shared;
using DbViewer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DbViewer.Services
{
    public interface IHubService
    {
        void EnsureConnection(Uri hubUri);
        Task<IEnumerable<DatabaseInfo>> ListAllAsync();
        Task<DownloadResult> DownloadDatabaseAsync(DatabaseInfo databaseInfo);
    }
}