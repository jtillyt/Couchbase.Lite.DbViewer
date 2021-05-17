using DbViewer.Shared;
using DbViewer.Models;
using System.IO;
using System.Threading.Tasks;

namespace DbViewer.Services
{
    public interface IDatabaseCacheService
    {
        void Cleanup(CacheRegistry cacheRegistry);
        Task<CacheRegistry> GetRegistry();
        Task SaveFromStream(Stream databaseDownloadStream, DatabaseInfo databaseInfo);
    }
}
