using System;
using System.IO;
using Dawn;
using DbViewer.Shared.Dtos;
using Newtonsoft.Json;
using Serilog;
using Xamarin.Essentials;

namespace DbViewer.Models
{
	public class CachedDatabase
	{
		private readonly object _synclock = new object();

		public CachedDatabase()
		{

		}

		public CachedDatabase(DatabaseInfo remoteDatabaseInfo, DateTimeOffset downloadTime)
		{
			RemoteDatabaseInfo = Guard.Argument(remoteDatabaseInfo, nameof(remoteDatabaseInfo))
				  .NotNull()
				  .Value;

			DownloadTime = downloadTime;
		}

		[JsonIgnore]
		public IDatabaseConnection ActiveConnection { get; private set; }

		public bool ConnectToRemote()
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
				catch (Exception ex)
				{
					__logger.Error(ex, "Error occured while attempting to connect to DB");
				}

				return false;
			}
		}

		public bool DisconnectFromRemote()
		{
			if (ActiveConnection != null)
			{
				try
				{
					ActiveConnection.Disconnect();
					ActiveConnection = null;
					return true;
				}
				catch (Exception ex)
				{
					__logger.Error(ex, "Error occured while attempting to disconnect from DB");
				}

				return false;
			}

			return true;
		}

		public DatabaseInfo RemoteDatabaseInfo { get; set; }

		public DateTimeOffset DownloadTime { get; set; }

		public string UserDefinedDisplayName { get; set; }

		/// <summary>
		/// The path to the directory that contains the database folder. Couchbase uses this with the subdirectory being the 'name'
		/// </summary>
		[JsonIgnore]
		public string LocalDatabasePathRoot => Path.Combine(FileSystem.AppDataDirectory, RemoteDatabaseInfo?.HubId);

		/// <summary>
		/// The full path to the directoy that contains the unzipped database contents.
		/// </summary>
		[JsonIgnore]
		public string LocalDatabasePathFull => Path.Combine(LocalDatabasePathRoot, RemoteDatabaseInfo?.FullDatabaseName);


		/// <summary>
		/// The full path to the zip file that was downloaded.
		/// </summary>
		[JsonIgnore]
		public string ArchiveFullPath => $"{LocalDatabasePathFull}.zip";

		private static readonly ILogger __logger = Log.Logger.ForContext<CachedDatabase>();
	}
}
