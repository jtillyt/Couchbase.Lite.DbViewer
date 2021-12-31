using DbViewer.Hub.DbProvider;
using DbViewer.Shared.Dtos;
using System;
using System.Collections.Generic;

namespace DbViewer.Hub.Repos
{
    public interface IDatabaseProviderRepository
    {
        void UpsertProvider(IDbProvider provider);
        List<IDbProvider> GetAllDbScanners(HubInfo hubInfo);
        IDbProvider GetOrCreate(ServiceInfo providerInfo, HubInfo hubInfo);
    }
}
