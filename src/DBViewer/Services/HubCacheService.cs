using Akavache;
using DbViewer.Shared.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace DbViewer.Services
{
    public class HubCacheService : IHubCacheService
    {
        private const string Hub_Cache_Key = "Hub_Cache";
        private Dictionary<string, HubInfo> _inMemoryRegistry = new Dictionary<string, HubInfo>();
        private object _synclock = new object();

        public HubCacheService()
        {
            CacheUpdated = new BehaviorSubject<IEnumerable<HubInfo>>(new List<HubInfo>());
        }

        public IObserver<IEnumerable<HubInfo>> CacheUpdated { get; }

        public async Task DeleteHub(string hubId)
        {
            lock (_synclock)
            {
                _inMemoryRegistry.Remove(hubId);
            }

            await SaveAll();
        }

        public async Task<List<HubInfo>> ListAll()
        {
            if (_inMemoryRegistry == null || !_inMemoryRegistry.Any())
            {
                var vals = await BlobCache.LocalMachine
                                          .GetOrCreateObject(Hub_Cache_Key, () => new List<HubInfo>());

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
        public async Task<HubInfo> GetCachedHub(string hubId)
        {
            var all = await ListAll();

            return all.FirstOrDefault(hub => hub.Id == hubId);
        }

        public async Task SaveHub(HubInfo hubInfo)
        {
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

            await SaveAll();
        }


        public async Task SaveAll() => await BlobCache.LocalMachine
                           .InsertObject(Hub_Cache_Key, DictToList());

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
