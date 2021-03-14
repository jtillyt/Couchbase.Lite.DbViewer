using DbViewer.Shared;
using DBViewer.Api;
using Refit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DBViewer.Services
{
    public class HubService : IHubService
    {
        private IDbHubHttpClient _httpClient;
        private Uri _lastConnectedUri;

        public HubService()
        {
        }

        public DateTimeOffset LastRefreshTime { get; set; }

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

        public Task<IEnumerable<DatabaseInfo>> ListAll()
        {
            return _httpClient.ListAll();
        }
    }
}
