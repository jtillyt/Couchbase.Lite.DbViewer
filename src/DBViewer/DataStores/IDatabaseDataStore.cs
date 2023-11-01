using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DbViewer.Models;
using DbViewer.Shared.Dtos;

namespace DbViewer.DataStores
{
	public interface IDatabaseDatastore
    {
        IObservable<CachedDatabaseRegistry> CacheUpdated { get; }

        void Cleanup(CachedDatabaseRegistry cacheRegistry);
        Task DeleteDatabaseAsync(CachedDatabase database, CancellationToken cancellationToken);
        Task<CachedDatabaseRegistry> GetRegistryAsync(CancellationToken cancellationToken);
        Task SaveFromStreamAsync(Stream databaseDownloadStream, DatabaseInfo databaseInfo, CancellationToken cancellationToken);
        void SaveDatabase(CachedDatabase cachedDatabase, CancellationToken cancellationToken);
    }
}
