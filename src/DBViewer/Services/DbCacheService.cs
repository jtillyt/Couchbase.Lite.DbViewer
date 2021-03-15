using Akavache;
using DbViewer.Shared;
using DBViewer.Models;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using Xamarin.Essentials;

namespace DBViewer.Services
{
    public class DbCacheService : IDbCacheService
    {
        private const string Registry_Key = nameof(Registry_Key);

        public DbCacheService()
        {
            CacheUpdated = new BehaviorSubject<CacheRegistry>(new CacheRegistry());
        }

        public IObserver<CacheRegistry> CacheUpdated { get; }

        public void SaveFromStream(Stream databaseDownloadStream, DatabaseInfo databaseInfo)
        {
            GetRegistry().Subscribe(registry =>
            {
                CachedDatabase dbItem = registry.DatabaseCollection.FirstOrDefault(db => db.RemoteDatabaseInfo == databaseInfo);

                if (dbItem == null)
                {
                    dbItem = new CachedDatabase(FileSystem.AppDataDirectory, databaseInfo, DateTimeOffset.Now);
                }

                dbItem.IsUnzipped = false;

                if (File.Exists(dbItem.ArchiveFullPath))
                {
                    File.Delete(dbItem.ArchiveFullPath);
                }

                using (var fileStream = new FileStream(dbItem.ArchiveFullPath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    databaseDownloadStream.CopyTo(fileStream);
                    fileStream.Close();
                }

                if (Directory.Exists(dbItem.LocalDatabasePathFull))
                {
                    Directory.Delete(dbItem.LocalDatabasePathFull, true);
                }

                SaveRegistry(registry);
            }, ex => Console.WriteLine(ex), () => Console.WriteLine("Done"));             // TODO: <James Thomas: 3/14/21> Logging/error handling 
        }

        private IObservable<CacheRegistry> GetRegistry()
        {
            return BlobCache.LocalMachine.GetOrCreateObject(Registry_Key, () => new CacheRegistry());
        }

        private void SaveRegistry(CacheRegistry registry)
        {
            BlobCache.LocalMachine.InsertObject(Registry_Key, registry).Subscribe(_ =>
            {
                CacheUpdated.OnNext(registry);
            });
        }
    }
}
