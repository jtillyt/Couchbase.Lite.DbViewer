using DbViewer.Shared;
using DBViewer.Models;
using System.IO;
using System.Threading.Tasks;

namespace DBViewer.Services
{
    public interface IDatabaseCacheService
    {
        void Cleanup(CacheRegistry cacheRegistry);
        Task<CacheRegistry> GetRegistry();
        Task SaveFromStream(Stream databaseDownloadStream, DatabaseInfo databaseInfo);
    }
}
