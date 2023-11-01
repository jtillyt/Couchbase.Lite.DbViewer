using System;
using System.IO;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using DbViewer.DataStores;
using DbViewer.Extensions;
using DbViewer.Models;
using DbViewer.Services;
using Newtonsoft.Json;
using Prism.Navigation;
using ReactiveUI;
using Xamarin.Essentials;

namespace DbViewer.ViewModels
{
	public class CachedDatabaseEditViewModel : NavigationViewModelBase, INavigatedAware
	{
		private readonly IHubService _hubService;
		private readonly IDatabaseDatastore _databaseCacheService;

		public CachedDatabaseEditViewModel(INavigationService navigationService, IDatabaseDatastore databaseCacheService, IHubService hubService)
			: base(navigationService)
		{
			_hubService = Guard.Argument(hubService, nameof(hubService))
				  .NotNull()
				  .Value;

			_databaseCacheService = Guard.Argument(databaseCacheService, nameof(databaseCacheService))
				  .NotNull()
				  .Value;

			ExportCommand = ReactiveCommand.CreateFromTask(ExecuteExportAsync);

			CopyLocalPathCommand = ReactiveCommand.Create(ExecuteCopyLocalPath);
		}

	

		public ReactiveCommand<Unit, Unit> SaveCommand { get; }

		public ReactiveCommand<Unit, Unit> CopyLocalPathCommand { get; }

		public ReactiveCommand<Unit, Unit> ExportCommand { get; }

		public CachedDatabase Database { get; set; }

		public string DisplayName
		{
			get => _displayName;
			set => this.RaiseAndSetIfChanged(ref _displayName, value);
		}

		public string DownloadTime
		{
			get => _downloadTime;
			set => this.RaiseAndSetIfChanged(ref _downloadTime, value);
		}

		public string HubAddress
		{
			get => _hubAddress;
			set => this.RaiseAndSetIfChanged(ref _hubAddress, value);
		}

		private string _localDbPath;
		public string LocalDbPath
		{
			get => _localDbPath;
			set => this.RaiseAndSetIfChanged(ref _localDbPath, value);
		}

		private string _exportPath;
		public string ExportPath
		{
			get => _exportPath;
			set => this.RaiseAndSetIfChanged(ref _exportPath, value);
		}

		public void OnNavigatedFrom(INavigationParameters parameters)
		{

		}

		public void OnNavigatedTo(INavigationParameters parameters)
		{
			var cachedVm = parameters.GetValue<CachedDatabaseItemViewModel>(nameof(CachedDatabaseItemViewModel));

			OnDatabaseUpdated(cachedVm.Database);
		}

		private void OnDatabaseUpdated(CachedDatabase cachedDatabase)
		{
			_cachedDatabase = cachedDatabase;

			HubAddress = GetHubAddressString(cachedDatabase);

			var dateTime = cachedDatabase.DownloadTime.DateTime;
			DownloadTime = GetDownloadTimeString(dateTime);

			DisplayName = cachedDatabase.UserDefinedDisplayName ?? cachedDatabase.RemoteDatabaseInfo?
																				 .DisplayDatabaseName;

			LocalDbPath = cachedDatabase.LocalDatabasePathFull;
		}

		private Task ExecuteExportAsync(CancellationToken cancellationToken)
		{
			return Task.Run(() =>
			{
				var dirPath = Path.Combine(_cachedDatabase.LocalDatabasePathRoot, Guid.NewGuid().ToString());

				RunOnUi(()=>{ ExportPath = dirPath; });

				try
				{
					_cachedDatabase.ConnectToRemote();

					var connection = _cachedDatabase.ActiveConnection;

					Directory.CreateDirectory(dirPath);

					foreach (var documentId in connection.ListAllDocumentIds(false))
					{
						using (var document = connection.GetDocumentById(documentId))
						{

							var documentText = string.Empty;

							try
							{
								var id = document.Id.Replace("::", "_")+".json";
								var docPath = Path.Combine(dirPath, id);
								var cleanedDocument = document.CleanAttachments();

								documentText = JsonConvert.SerializeObject(cleanedDocument);
								File.WriteAllText(docPath, documentText);

							}
							catch (Exception ex)
							{

							}
						}
					}
				}
				finally
				{
					_cachedDatabase.DisconnectFromRemote();
				}

			});
		}

		private async Task ExecuteSaveAsync(CancellationToken cancellationToken)
		{
			if (_cachedDatabase == null)
			{
				//Do we throw?  Alert?
				return;
			}

			_cachedDatabase.UserDefinedDisplayName = DisplayName;

			_databaseCacheService.SaveDatabase(_cachedDatabase, cancellationToken);
		}

		private static string GetDownloadTimeString(DateTime dateTime)
		{
			return $"{dateTime.ToShortDateString()} {dateTime.ToShortTimeString()}";
		}

		private static string GetHubAddressString(CachedDatabase cachedDatabase)
		{
			return cachedDatabase?.RemoteDatabaseInfo?.RequestAddress?.Host ?? "Unknown";
		}

		private void ExecuteCopyLocalPath()
		{
			Clipboard.SetTextAsync(LocalDbPath);
		}

		private string _displayName;
		private string _downloadTime;
		private string _hubAddress;

		private CachedDatabase _cachedDatabase;
	}
}
