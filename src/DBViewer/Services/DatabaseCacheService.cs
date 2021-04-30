using Akavache;
using DbViewer.Shared;
using DBViewer.Models;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace DBViewer.Services
{
    public class DatabaseCacheService : IDatabaseCacheService
    {
        private const string Registry_Key = nameof(Registry_Key);

        public DatabaseCacheService() { CacheUpdated = new BehaviorSubject<CacheRegistry>(new CacheRegistry()); }

        public IObserver<CacheRegistry> CacheUpdated { get; }

        public async Task SaveFromStream(Stream databaseDownloadStream, DatabaseInfo databaseInfo)
        {
            var registry = await GetRegistry();

            CachedDatabase dbItem = registry.DatabaseCollection
                                            .FirstOrDefault(
                                    db => db.RemoteDatabaseInfo.DisplayDatabaseName == databaseInfo.DisplayDatabaseName);

            if(dbItem == null || !Directory.Exists(dbItem.LocalDatabasePathRoot))
            {
                dbItem = new CachedDatabase(FileSystem.AppDataDirectory, databaseInfo, DateTimeOffset.Now);
                registry.DatabaseCollection
                        .Add(dbItem);
            }
            else
            {
                dbItem.ActiveConnection?.Disconnect();
            }

            dbItem.IsUnzipped = false;

            if(File.Exists(dbItem.ArchiveFullPath))
            {
                File.Delete(dbItem.ArchiveFullPath);
            }

            using(var fileStream = new FileStream(dbItem.ArchiveFullPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                databaseDownloadStream.CopyTo(fileStream);
                fileStream.Close();
            }

            if(UnzipDbStream(dbItem))
            {
                dbItem.IsUnzipped = true;
            }

            SaveRegistry(registry);
        }

        public bool UnzipDbStream(CachedDatabase cachedDb)
        {
            try
            {
                if(Directory.Exists(cachedDb.LocalDatabasePathFull))
                {
                    Directory.Delete(cachedDb.LocalDatabasePathFull, true);
                }

                Directory.CreateDirectory(cachedDb.LocalDatabasePathFull);
            }
            catch(Exception ex)
            {
                return false;
            }

            try
            {
                var fastZip = new FastZip();
                fastZip.ExtractZip(cachedDb.ArchiveFullPath, cachedDb.LocalDatabasePathFull, null);

                File.Delete(cachedDb.ArchiveFullPath);
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
        }

        public async Task<CacheRegistry> GetRegistry()
        {
            if(_inMemoryRegistry == null)
            {
                _inMemoryRegistry = await BlobCache.LocalMachine
                                                   .GetOrCreateObject(Registry_Key, () => new CacheRegistry());
                Cleanup(_inMemoryRegistry);
            }

            return _inMemoryRegistry;
        }

        public void Cleanup(CacheRegistry cacheRegistry)
        {
            var dbsWithBadPaths = new List<CachedDatabase>();
            foreach(var item in cacheRegistry.DatabaseCollection)
            {
                if(!Directory.Exists(item.LocalDatabasePathFull))
                {
                    dbsWithBadPaths.Add(item);
                }
            }

            foreach(var badDb in dbsWithBadPaths)
            {
                cacheRegistry.DatabaseCollection
                             .Remove(badDb);
            }

            SaveRegistry(cacheRegistry);
        }

        private void SaveRegistry(CacheRegistry registry)
        {
            BlobCache.LocalMachine
                     .InsertObject(Registry_Key, registry)
                     .Subscribe(
            _ =>
            {
                CacheUpdated.OnNext(registry);
            });
        }

        private CacheRegistry _inMemoryRegistry = null;
    }
}
