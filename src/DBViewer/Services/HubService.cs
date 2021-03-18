using Dawn;
using DbViewer.Shared;
using DBViewer.Api;
using DBViewer.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DBViewer.Services
{
    public class HubService : IHubService
    {
        private IDatabaseCacheService _dbCacheService;
        private IDbHubHttpClient _httpClient;
        private Uri _lastConnectedUri;

        public HubService(IDatabaseCacheService dbCacheService)
        {
            _dbCacheService = Guard.Argument(dbCacheService, nameof(dbCacheService))
                  .NotNull()
                  .Value;
        }

        public DateTimeOffset LastRefreshTime { get; set; }

        public async Task<DownloadResult> DownloadDatabaseAsync(DatabaseInfo databaseInfo)
        {
            var downloadResult = new DownloadResult();
            downloadResult.DatabaseInfo = databaseInfo;

            Stream stream = null;

            try
            {
                var httpResponseMessage = await _httpClient.GetDatabase(databaseInfo.DisplayDatabaseName);
                stream = await httpResponseMessage.Content.ReadAsStreamAsync();
            }
            catch (Exception ex)
            {

            }

            if (stream == null)
                throw new InvalidDataException(nameof(DownloadDatabaseAsync));

            try
            {
                _dbCacheService.SaveFromStream(stream, databaseInfo);
                downloadResult.WasSuccesful = true;
            }
            catch (Exception ex)
            {

            }

            return downloadResult;
        }

        public void EnsureConnection(Uri hubUri)
        {
            if (_httpClient == null || _lastConnectedUri != hubUri)
            {
                try
                {
                    _httpClient = RestService.For<IDbHubHttpClient>(hubUri.ToString());
                    _lastConnectedUri = hubUri;

                    LastRefreshTime = DateTimeOffset.Now;
                }
                catch (Exception ex)
                {

                }
            }
        }

        public Task<IEnumerable<DatabaseInfo>> ListAllAsync()
        {
            return _httpClient.ListAll();
        }
    }
}
