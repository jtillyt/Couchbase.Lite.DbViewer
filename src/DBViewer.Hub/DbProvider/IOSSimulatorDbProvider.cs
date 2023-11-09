using Dawn;
using DbViewer.Hub.Services;
using DbViewer.Shared;
using DbViewer.Shared.Dtos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DbViewer.Hub.DbProvider
{
    [ServiceType(ServiceConstants.iOSDatabaseLocatorServiceTypeName, ServiceConstants.iOSDatabaseLocatorServiceTypeId)]
    public class IOSSimulatorDbProvider : IDbProvider, IService
    {
        private const string RelativePath_ConfigKey = "DataRelativePath";
        private const string AppBundleId_ConfigKey = "AppBundleId";
        private const string SimulatorId_ConfigKey = "SimulatorId";

        private ServiceInfo _serviceInfo;
        private HubInfo _hubInfo;

        private readonly ILogger<IOSSimulatorDbProvider> _logger;

        public IOSSimulatorDbProvider(ILogger<IOSSimulatorDbProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Id = Guid.NewGuid().ToString();
        }

        [ServiceProperty(AppBundleId_ConfigKey, "App Bundle Id", "com.your.app.bundle",
            "This is the BundleId found in the app's Info.plist file")]
        public string AppBundleId { get; set; }


        [ServiceProperty(RelativePath_ConfigKey, "Relative Path to Database", "Documents/DataStore",
            "This is the relative path to where the databases are in relation to the root of the application.")]
        public string RelativePathToData { get; set; }


        [ServiceProperty(SimulatorId_ConfigKey, "Simulator Id", "Some-Guid-Id",
            "This is the simulator id that you would like to target for scanning. These can be found in XCode. Use the string 'booted' for target the active simulator")]
        public string SimulatorId { get; set; }

        public string Id { get; }

        public IEnumerable<DatabaseInfo> Scan()
        {
            if (!OperatingSystem.IsMacOS())
            {
                _logger.LogWarning("Can't scan. This scanner only works on Mac.");
                return Enumerable.Empty<DatabaseInfo>();
            }

            var list = new List<DatabaseInfo>();
            var appDataPath = GetCurrentSimulatorDataPath();
            var rootDbPath = Path.Combine(appDataPath, RelativePathToData);

            _logger.LogInformation($"DBRoot dir: {rootDbPath}");

            if (!Directory.Exists(rootDbPath))
                return list;

            foreach (var dir in Directory.GetDirectories(rootDbPath))
            {
                list.Add(new DatabaseInfo()
                {
                    DisplayDatabaseName = Path.GetFileNameWithoutExtension(dir),
                    FullDatabaseName = Path.GetFileName(dir),
                    ProviderInfo = _serviceInfo,
                    HubId = _hubInfo.Id
                });
            }

            return list;
        }

        public string GetCurrentDatabaseRootPath(DatabaseInfo databaseInfo)
        {
            var appDataPath = GetCurrentSimulatorDataPath();
            var rootDbPath = Path.Combine(appDataPath, RelativePathToData);

            return rootDbPath;
        }

        private string GetCurrentSimulatorDataPath()
        {
            var psi = BuildCurrentSimCommandCommand();
            var process = new Process
            {
                StartInfo = psi
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return result.Trim();
        }

        private ProcessStartInfo BuildCurrentSimCommandCommand()
        {
            var argumentString = $"simctl get_app_container {SimulatorId} {AppBundleId} data";

            _logger.LogInformation($"Fetching sim path with args: {argumentString}");

            var psi = new ProcessStartInfo
            {
                FileName = "/usr/bin/xcrun",
                Arguments = argumentString,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            return psi;
        }

        public void InitiateService(ServiceInfo serviceInfo, HubInfo hubInfo)
        {
            _serviceInfo = Guard.Argument(serviceInfo)
                .NotNull()
                .Value;

            _hubInfo = Guard.Argument(hubInfo)
                .NotNull()
                .Value;

            _logger.LogInformation(
                $"Initiating service {_serviceInfo.ServiceName} of type {_serviceInfo.ServiceTypeId}");

            AppBundleId = _serviceInfo.Properties.FirstOrDefault(prop => prop.Key == AppBundleId_ConfigKey)?.Value;
            SimulatorId = _serviceInfo.Properties.FirstOrDefault(prop => prop.Key == SimulatorId_ConfigKey)?.Value;
            RelativePathToData = _serviceInfo.Properties.FirstOrDefault(prop => prop.Key == RelativePath_ConfigKey)
                ?.Value;
        }
    }
}