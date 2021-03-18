using DbViewer.Shared;
using DBViewer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBViewer.Services
{
    public interface IHubService
    {
        void EnsureConnection(Uri hubUri);
        Task<IEnumerable<DatabaseInfo>> ListAllAsync();
        Task<DownloadResult> DownloadDatabaseAsync(DatabaseInfo databaseInfo);
    }
}