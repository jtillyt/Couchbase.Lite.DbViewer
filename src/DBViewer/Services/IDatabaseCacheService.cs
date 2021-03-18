using DbViewer.Shared;
using DBViewer.Models;
using System;
using System.IO;

namespace DBViewer.Services
{
    public interface IDatabaseCacheService
    {
        IObservable<CacheRegistry> GetRegistry();
        void SaveFromStream(Stream databaseDownloadStream, DatabaseInfo databaseInfo);
    }
}
