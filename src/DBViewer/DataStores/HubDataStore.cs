using Akavache;
using DbViewer.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace DbViewer.DataStores
{
    public class HubDatastore : IHubDatastore
    {
        private const string HubCacheKey = "HubCache";

        private readonly Dictionary<string, HubInfo> _inMemoryRegistry = new Dictionary<string, HubInfo>();
        private readonly object _synclock = new object();

        public HubDatastore()
        {
            CacheUpdated = new BehaviorSubject<IEnumerable<HubInfo>>(new List<HubInfo>());
        }

        public IObserver<IEnumerable<HubInfo>> CacheUpdated { get; }

        public async Task DeleteHubAsync(string hubId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_synclock)
            {
                _inMemoryRegistry.Remove(hubId);
            }

            await SaveAllAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<HubInfo>> ListAllAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_inMemoryRegistry == null || !_inMemoryRegistry.Any())
            {
                var vals = await BlobCache.LocalMachine
                    .GetOrCreateObject(HubCacheKey, () => new List<HubInfo>());

                foreach (var val in vals)
                {
                    lock (_synclock)
                    {
                        if (!_inMemoryRegistry.ContainsKey(val.Id))
                        {
                            _inMemoryRegistry.Add(val.Id, val);
                        }
                    }
                }
            }

            return DictToList();
        }

        public async Task<HubInfo> GetCachedHubAsync(string hubId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var all = await ListAllAsync(cancellationToken).ConfigureAwait(false);

            return all.FirstOrDefault(hub => hub.Id == hubId);
        }

        public Task SaveHubAsync(HubInfo hubInfo, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_synclock)
            {
                if (_inMemoryRegistry.ContainsKey(hubInfo.Id))
                {
                    _inMemoryRegistry[hubInfo.Id] = hubInfo;
                }
                else
                {
                    _inMemoryRegistry.Add(hubInfo.Id, hubInfo);
                }
            }

            return SaveAllAsync(cancellationToken);
        }


        public async Task SaveAllAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await BlobCache.LocalMachine
                .InsertObject(HubCacheKey, DictToList());
        }

        private List<HubInfo> DictToList()
        {
            List<HubInfo> hubs;

            lock (_synclock)
            {
                if (_inMemoryRegistry?.Values == null)
                {
                    return new List<HubInfo>();
                }

                hubs = _inMemoryRegistry.Values.ToList();
            }

            return hubs;
        }
    }
}