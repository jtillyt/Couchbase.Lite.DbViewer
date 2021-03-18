using Akavache;
using DbViewer.Shared;
using DBViewer.Models;
using ICSharpCode.SharpZipLib.Zip;
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
                    registry.DatabaseCollection.Add(dbItem);
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

                if (UnzipDbStream(dbItem))
                {
                    dbItem.IsUnzipped = true;
                }

                SaveRegistry(registry);
            }, ex => Console.WriteLine(ex), () => Console.WriteLine("Done"));
            // TODO: <James Thomas: 3/14/21> Logging/error handling 
        }

        public bool UnzipDbStream(CachedDatabase cachedDb)
        {
            try
            {
                if (Directory.Exists(cachedDb.LocalDatabasePathFull))
                {
                    Directory.Delete(cachedDb.LocalDatabasePathFull, true);
                }
                Directory.CreateDirectory(cachedDb.LocalDatabasePathFull);
            }
            catch (Exception ex)
            {
                return false;
            }

            try
            {
                var fastZip = new FastZip();
                fastZip.ExtractZip(cachedDb.ArchiveFullPath, cachedDb.LocalDatabasePathFull, null);

                File.Delete(cachedDb.ArchiveFullPath);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public IObservable<CacheRegistry> GetRegistry()
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
