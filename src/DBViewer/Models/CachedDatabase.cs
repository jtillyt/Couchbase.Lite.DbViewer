using Dawn;
using DbViewer.Shared;
using System;
using System.IO;

namespace DBViewer.Models
{
    public class CachedDatabase
    {
        public CachedDatabase()
        {
        }

        public CachedDatabase(string localDatabasePathRoot, DatabaseInfo remoteDatabaseInfo, DateTimeOffset downloadTime)
        {
            LocalDatabasePathRoot = Guard.Argument(localDatabasePathRoot, nameof(localDatabasePathRoot))
                  .NotNull()
                  .Value;

            RemoteDatabaseInfo = Guard.Argument(remoteDatabaseInfo, nameof(remoteDatabaseInfo))
                  .NotNull()
                  .Value;

            DownloadTime = downloadTime;

            LocalDatabasePathFull = Path.Combine(LocalDatabasePathRoot, RemoteDatabaseInfo.FullDatabaseName);
            ArchiveFullPath = LocalDatabasePathFull + ".zip";
        }

        public DatabaseInfo RemoteDatabaseInfo { get; set; }
        public DateTimeOffset DownloadTime { get; set; }

        /// <summary>
        /// The path to the directory that contains the database folder. Couchbase uses this with the subdirectory being the 'name'
        /// </summary>
        public string LocalDatabasePathRoot { get; set; }

        /// <summary>
        /// The full path to the directoy that contains the unzipped database contents.
        /// </summary>
        public string LocalDatabasePathFull {get;set; }


        /// <summary>
        /// The full path to the zip file that was downloaded.
        /// </summary>
        public string ArchiveFullPath { get; set; }

        /// <summary>
        /// Tracking whether we've already successfully unzipped
        /// </summary>
        public bool IsUnzipped {get;set; }
    }
}
