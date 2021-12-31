using Couchbase.Lite.DI;
using Dawn;
using DbViewer.Hub.DbProvider;
using DbViewer.Hub.Services;
using DbViewer.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DbViewer.Hub.Repos
{
    public class DatabaseProviderRepository : IDatabaseProviderRepository
    {
        public DatabaseProviderRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = Guard.Argument(serviceProvider, nameof(serviceProvider))
                                    .NotNull()
                                    .Value;
        }

        private List<IDbProvider> _providers = new List<IDbProvider>();

        public IDbProvider GetOrCreate(ServiceInfo providerInfo, HubInfo hubInfo)
        {
            var existing = _providers.FirstOrDefault(provider => provider.Id == providerInfo.Id);

            if (existing != null)
            {
                UpdateProvider(existing, providerInfo, hubInfo);
                return existing;
            }

            var serviceDefinition = hubInfo.ServiceDefinitions.FirstOrDefault(st => st.Id == providerInfo.ServiceTypeId);
            var serviceType = Type.GetType(serviceDefinition.FullyQualifiedAssemblyTypeName);
            var service = _serviceProvider.GetService(serviceType);

            if (service is IDbProvider dbProvider)
            {
                UpdateProvider(dbProvider, providerInfo, hubInfo);
                return dbProvider;
            }

            throw new Exception("Could not find db provider");
        }

        public List<IDbProvider> GetAllDbScanners(HubInfo hubInfo)
        {
            var dbProviders = new List<IDbProvider>();

            var serviceTypes = hubInfo.ServiceDefinitions;

            foreach (var serviceInfo in hubInfo.ActiveServices)
            {
                var matchingType = serviceTypes.FirstOrDefault(st => st.Id == serviceInfo.ServiceTypeId);

                if (matchingType == null)
                {
                    continue;
                }

                var serviceType = Type.GetType(matchingType.FullyQualifiedAssemblyTypeName);

                var service = _serviceProvider.GetService(serviceType);

                if (service is IDbProvider dbProvider)
                {
                    dbProviders.Add(dbProvider);
                    UpdateProvider(dbProvider, serviceInfo, hubInfo);
                }
            }

            return dbProviders;
        }

        public IDbProvider GetById(string providerId)
        {
            if (string.IsNullOrWhiteSpace(providerId))
            {
                return null;
            }

            var existing = _providers.FirstOrDefault(provider => provider.Id == providerId);

            return existing;
        }

        public void UpsertProvider(IDbProvider provider)
        {
            var existing = GetById(provider.Id);

            if (existing != null)
            {
                _providers.Remove(provider);
            }

            _providers.Add(provider);
        }

        private void UpdateProvider(IDbProvider dbProvider, ServiceInfo serviceInfo, HubInfo hubInfo)
        {
            UpsertProvider(dbProvider);

            if (dbProvider is IService hubService)
            {
                hubService.InitiateService(serviceInfo, hubInfo);
            }
        }

        private readonly IServiceProvider _serviceProvider;
    }
}
