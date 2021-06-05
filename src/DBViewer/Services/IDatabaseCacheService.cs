using DbViewer.Shared;
using DbViewer.Models;
using System.IO;
using System.Threading.Tasks;

namespace DbViewer.Services
{
    public interface IDatabaseCacheService
    {
        void Cleanup(CachedDatabaseRegistry cacheRegistry);
        Task<CachedDatabaseRegistry> GetRegistry();
        Task SaveFromStream(Stream databaseDownloadStream, DatabaseInfo databaseInfo);
    }
}
