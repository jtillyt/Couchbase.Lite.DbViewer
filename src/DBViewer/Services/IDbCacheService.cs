using DbViewer.Shared;
using DBViewer.Models;
using System.IO;

namespace DBViewer.Services
{
    public interface IDbCacheService
    {
        void SaveFromStream(Stream databaseDownloadStream, DatabaseInfo databaseInfo);
    }
}
