using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using DbViewer.DataStores;
using DbViewer.Models;
using DbViewer.Services;
using ReactiveUI;

namespace DbViewer.ViewModels
{
	public class CachedDatabaseItemViewModel : ViewModelBase
	{
		private readonly IHubService _hubService;
		private readonly IDatabaseDatastore _databaseCacheService;

		public CachedDatabaseItemViewModel(CachedDatabase cachedDatabase, IDatabaseDatastore databaseCacheService, IHubService hubService)
		{
			Database = Guard.Argument(cachedDatabase, nameof(cachedDatabase))
										.NotNull()
										.Value;

			_hubService = Guard.Argument(hubService, nameof(hubService))
				  .NotNull()
				  .Value;

			_databaseCacheService = Guard.Argument(databaseCacheService, nameof(databaseCacheService))
				  .NotNull()
				  .Value;

			DisplayName = Database.UserDefinedDisplayName ?? Database.RemoteDatabaseInfo?
																	 .DisplayDatabaseName;

			HubAddress = GetHubAddressString(Database);

			var dateTime = Database.DownloadTime.DateTime;
			DownloadTime = GetDownloadTimeString(dateTime);

			GetLatestCommand = ReactiveCommand.CreateFromTask(ExecuteGetLatestAsync);
			DeleteCommand = ReactiveCommand.CreateFromTask(ExecuteDeleteAsync);
		}

		public ReactiveCommand<Unit, Unit> GetLatestCommand { get; }
		public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

		public CachedDatabase Database { get; set; }

		private string _displayName;
		public string DisplayName
		{
			get => _displayName;
			set => this.RaiseAndSetIfChanged(ref _displayName, value);
		}

		private string _downloadTime;
		public string DownloadTime
		{
			get => _downloadTime;
			set => this.RaiseAndSetIfChanged(ref _downloadTime, value);
		}

		private string _hubAddress;
		public string HubAddress
		{
			get => _hubAddress;
			set => this.RaiseAndSetIfChanged(ref _hubAddress, value);
		}

		public async Task ExecuteGetLatestAsync(CancellationToken cancellationToken)
		{
			var remoteInfo = Database.RemoteDatabaseInfo;

			var result = await _hubService.DownloadDatabaseAsync(remoteInfo.RequestAddress, remoteInfo, cancellationToken)
										  .ConfigureAwait(false);

			if (result.WasSuccesful)
			{
				RunOnUi(() =>
				{
					DownloadTime = GetDownloadTimeString(DateTime.Now);
				});
			}
		}

		private Task ExecuteDeleteAsync(CancellationToken cancellationToken) => _databaseCacheService.DeleteDatabaseAsync(Database, cancellationToken);


		private static string GetDownloadTimeString(DateTime dateTime)
		{
			return $"{dateTime.ToShortDateString()} {dateTime.ToShortTimeString()}";
		}

		private static string GetHubAddressString(CachedDatabase cachedDatabase)
		{
			return cachedDatabase?.RemoteDatabaseInfo?.RequestAddress?.Host ?? "Unknown";
		}
	}
}
