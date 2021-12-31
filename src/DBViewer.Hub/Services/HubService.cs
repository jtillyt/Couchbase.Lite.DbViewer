using Dawn;
using DbViewer.Hub.Couchbase;
using DbViewer.Hub.DbProvider;
using DbViewer.Hub.Repos;
using DbViewer.Shared.Couchbase;
using DbViewer.Shared.Dtos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DbViewer.Hub.Services
{
    public class HubService : IHubService
    {
        private const string DataFolder = "Data";
        private const string HubInfoFilePath = "HubInfo.json";

        private readonly ILogger<HubService> _logger;
        private readonly IDatabaseConnection _databaseConnection;
        private readonly IDatabaseProviderRepository _databaseProviderRepository;

        private HubInfo _hubInfo;

        public HubService(ILogger<HubService> logger, IDatabaseProviderRepository databaseProviderRepository, IDatabaseConnection databaseConnection)
        {
            _logger = Guard.Argument(logger, nameof(logger))
                           .NotNull()
                           .Value;

            _databaseConnection = Guard.Argument(databaseConnection, nameof(databaseConnection))
                                       .NotNull()
                                       .Value;

            _databaseProviderRepository = Guard.Argument(databaseProviderRepository, nameof(databaseProviderRepository))
                                               .NotNull()
                                               .Value;
        }

        public HubInfo GetLatestHub()
        {
            _logger.LogDebug("Fetching latest hub info.");

            if (_hubInfo != null)
            {
                return _hubInfo;
            }

            var path = GetPath();

            if (!File.Exists(path))
            {
                _logger.LogDebug("Hub info does not exist. Creating.. ");

                var newHub = CreateNew();
                SaveLatestHub(newHub);

                _logger.LogDebug("Hub info created.");
                return newHub;
            }

            _logger.LogDebug("Loading hub info from disk. ");

            var hubInfoJsonContexts = File.ReadAllText(path);

            _hubInfo = JsonConvert.DeserializeObject<HubInfo>(hubInfoJsonContexts);

            return _hubInfo;
        }

        public void SaveLatestHub(HubInfo hubInfo)
        {
            _logger.LogDebug("Saving latest hub info...");

            var path = GetPath(true);

            var hubInfoJsonContexts = JsonConvert.SerializeObject(hubInfo);

            File.WriteAllText(path, hubInfoJsonContexts);

            _logger.LogDebug("Hub info saved.");

            _hubInfo = hubInfo;
        }


        public DocumentInfo SaveDocumentToDatabase(DocumentInfo documentInfo)
        {
            var dbProvider = _databaseProviderRepository.GetOrCreate(documentInfo.DatabaseInfo?.ProviderInfo, GetLatestHub());
            var rootDatabasePath = dbProvider.GetCurrentDatabaseRootPath(documentInfo.DatabaseInfo);

            var isConnected = _databaseConnection.Connect(rootDatabasePath, documentInfo.DatabaseInfo.DisplayDatabaseName);

            // TODO: <James Thomas: 6/27/21> Switch to full response object instead of null
            if (!isConnected)
            {
                return null;
            }

            var document = _databaseConnection.GetDocumentById(documentInfo.DocumentId);

            var dictionary = CbUtils.ParseTo<Dictionary<string, object>>(documentInfo.DataAsJson);

            var mutableDoc = document.ToMutable();
            mutableDoc.SetData(dictionary);

            _databaseConnection.SaveDocument(mutableDoc);

            mutableDoc.Dispose();

            _databaseConnection.Disconnect();

            return GetDocumentById(documentInfo.DatabaseInfo, documentInfo.DocumentId);
        }

        public DocumentInfo GetDocumentById(DatabaseInfo databaseInfo, string documentId)
        {
            var dbProvider = _databaseProviderRepository.GetOrCreate(databaseInfo?.ProviderInfo, GetLatestHub());
            var rootDatabasePath = dbProvider.GetCurrentDatabaseRootPath(databaseInfo);

            var isConnected = _databaseConnection.Connect(rootDatabasePath, databaseInfo.DisplayDatabaseName);

            // TODO: <James Thomas: 6/27/21> Switch to full response object instead of null
            if (!isConnected)
            {
                return null;
            }

            var document = _databaseConnection.GetDocumentById(documentId);

            var updatedJson = JsonConvert.SerializeObject(document, Formatting.Indented);

            var documentInfo = new DocumentInfo(databaseInfo, documentId, document.RevisionID, updatedJson);

            document.Dispose();

            _databaseConnection.Disconnect();

            return documentInfo;
        }

        public bool DeleteDocument(DatabaseInfo databaseInfo, string documentId)
        {
            _logger.LogDebug($"Deleting document with id '{documentId}'...");


            var dbProvider = _databaseProviderRepository.GetOrCreate(databaseInfo?.ProviderInfo, GetLatestHub());
            var rootDatabasePath = dbProvider.GetCurrentDatabaseRootPath(databaseInfo);

            var isConnected = _databaseConnection.Connect(rootDatabasePath, databaseInfo.DisplayDatabaseName);

            // TODO: <James Thomas: 6/27/21> Switch to full response object instead of null
            if (!isConnected)
            {
                return false;
            }

            _databaseConnection.DeleteDocumentById(documentId);

            _databaseConnection.Disconnect();

            _logger.LogDebug($"Deleted document with id '{documentId}'.");

            return true;
        }

        private static string GetPath(bool ensure = false)
        {
            var filePath = Path.Combine(DataFolder, HubInfoFilePath);

            if (!Directory.Exists(DataFolder))
            {
                Directory.CreateDirectory(DataFolder);
            }

            return filePath;
        }

        private HubInfo CreateNew()
        {
            var serviceTypes = ServiceScanner.GetServicesForAssembly(GetType().Assembly);
            var staticDirectoryScanner = serviceTypes.FirstOrDefault(st => st.FullyQualifiedAssemblyTypeName == typeof(StaticDirectoryDbProvider).AssemblyQualifiedName);

            var hubInfo = new HubInfo
            {
                HubName = $"{Environment.MachineName} - Hub",
                Id = Guid.NewGuid().ToString(),
                ServiceDefinitions = serviceTypes,
                ActiveServices = staticDirectoryScanner == null ? new List<ServiceInfo>() : new List<ServiceInfo>() { CreateServiceFromDefinition(staticDirectoryScanner) }
            };

            return hubInfo;
        }

        private ServiceInfo CreateServiceFromDefinition(ServiceDefinition definition)
        {
            var serviceInfo = new ServiceInfo
            {
                Id = Guid.NewGuid().ToString(),
                ServiceName = definition.Name,
                ServiceTypeId = definition.Id
            };

            foreach (var prop in definition.Properties)
            {
                serviceInfo.Properties.Add(prop);
            }

            return serviceInfo;
        }


    }
}
