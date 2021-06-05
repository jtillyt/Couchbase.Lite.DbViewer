using Dawn;
using DbViewer.Api;
using DbViewer.Models;
using DbViewer.Shared;
using DbViewer.Shared.Configuration;
using Refit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DbViewer.Services
{
    public class HubService : IHubService
    {
        private Dictionary<Uri, IDbHubHttpClient> _hubClients = new Dictionary<Uri, IDbHubHttpClient>();

        private IDatabaseCacheService _dbCacheService;
        private IHubCacheService _hubCacheService;

        public HubService(IDatabaseCacheService dbCacheService, IHubCacheService hubCacheService)
        {
            _dbCacheService = Guard.Argument(dbCacheService, nameof(dbCacheService))
                                   .NotNull()
                                   .Value;

            _hubCacheService = Guard.Argument(hubCacheService, nameof(hubCacheService))
                                    .NotNull()
                                    .Value;
        }

        public DateTimeOffset LastRefreshTime { get; set; }

        public async Task<DownloadResult> DownloadDatabaseAsync(Uri hubUri, DatabaseInfo databaseInfo)
        {
            var connection = GetConnection(hubUri);

            var downloadResult = new DownloadResult();
            downloadResult.DatabaseInfo = databaseInfo;

            Stream stream = null;

            try
            {
                var httpResponseMessage = await connection.GetDatabase(databaseInfo.DisplayDatabaseName);
                stream = await httpResponseMessage.Content.ReadAsStreamAsync();
            }
            catch (Exception ex)
            {

            }

            if (stream == null)
                throw new InvalidDataException(nameof(DownloadDatabaseAsync));

            try
            {
                databaseInfo.RequestAddress = hubUri;
                await _dbCacheService.SaveFromStream(stream, databaseInfo);
                downloadResult.WasSuccesful = true;
            }
            catch (Exception ex)
            {

            }

            return downloadResult;
        }

        public IDbHubHttpClient GetConnection(Uri hubUri)
        {
            IDbHubHttpClient client = null;

            if (!_hubClients.TryGetValue(hubUri, out client))
            {
                try
                {
                    client = RestService.For<IDbHubHttpClient>(hubUri.ToString(), new RefitSettings
                    {
                        ContentSerializer = new NewtonsoftJsonContentSerializer()
                    });

                    _hubClients.Add(hubUri, client);

                    LastRefreshTime = DateTimeOffset.Now;
                }
                catch (Exception ex)
                {

                }
            }

            return client;
        }


        public Task<IEnumerable<DatabaseInfo>> ListAllHubDatabasesAsync(Uri hubUri) => GetConnection(hubUri).ListAll();

        public Task<HubInfo> GetCachedHubAsync(string hubId) => _hubCacheService.GetCachedHub(hubId);

        public Task<List<HubInfo>> ListAllKnownHubsAsync() => _hubCacheService.ListAll();

        public async Task<HubInfo> TryAddHubAsync(Uri hubUri)
        {
            var connection = GetConnection(hubUri);

            HubInfo hubInfo = null;

            try
            {
                hubInfo = await connection.GetHubInfo();
                hubInfo.HostAddress = hubUri.ToString();

                await _hubCacheService.SaveHub(hubInfo);
            }
            catch (Exception ex)
            {

            }

            return hubInfo;
        }

        public async Task<bool> UpdateHubAsync(HubInfo hubInfo)
        {
            var uri = new Uri(hubInfo.HostAddress);
            var connection = GetConnection(uri);

            try
            {
                await connection.UpdateHub(hubInfo);
            }
            catch (Exception ex)
            {
                return false;
            }

            try
            {
                await _hubCacheService.SaveHub(hubInfo);
                return true;
            }
            catch(Exception ex)
            {

            }

            return false;
        }
    }
}
