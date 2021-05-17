using Dawn;
using DbViewer.Shared;
using DbViewer.Services;
using System;
using System.IO;

namespace DbViewer.Models
{
    public class CachedDatabase
    {
        private object _synclock = new object();

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

        public IDatabaseConnection ActiveConnection { get; private set; }

        public bool Connect()
        {
            lock (_synclock)
            {
                if (ActiveConnection != null && ActiveConnection.IsConnected)
                    return true;

                ActiveConnection = new DatabaseConnection();

                try
                {
                    ActiveConnection.Connect(LocalDatabasePathRoot, RemoteDatabaseInfo.DisplayDatabaseName);
                    return true;
                }
                catch
                {
                    // TODO: <James Thomas: 4/28/83> Add logging 
                }

                return false;
            }
        }

        public bool Disconnect()
        {
            if (ActiveConnection != null)
            {
                try
                {
                    ActiveConnection.Disconnect();
                    ActiveConnection = null;
                    return true;
                }
                catch
                {
                    // TODO: <James Thomas: 4/28/83> Add logging 
                }

                return false;
            }

            return true;
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
        public string LocalDatabasePathFull { get; set; }


        /// <summary>
        /// The full path to the zip file that was downloaded.
        /// </summary>
        public string ArchiveFullPath { get; set; }

        /// <summary>
        /// Tracking whether we've already successfully unzipped
        /// </summary>
        public bool IsUnzipped { get; set; }
    }
}
