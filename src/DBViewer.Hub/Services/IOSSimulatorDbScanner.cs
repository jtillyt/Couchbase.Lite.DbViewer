using Dawn;
using DbViewer.Shared;
using DbViewer.Shared.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DbViewer.Hub.Services
{
    [ServiceType("iOS Simulator Scanner", "ios-simulator-scanner")]
    public class IOSSimulatorDbScanner : IDbScanner, IService
    {
        private const string RelativePath_ConfigKey = "DataRelativePath";
        private const string AppBundleId_ConfigKey = "AppBundleId";
        private const string SimulatorId_ConfigKey = "SimulatorId";

        private ServiceInfo _serviceInfo;

        private readonly ILogger<IOSSimulatorDbScanner> _logger;

        public IOSSimulatorDbScanner(ILogger<IOSSimulatorDbScanner> logger)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        [ServiceProperty(AppBundleId_ConfigKey, "App Bundle Id", "com.your.app.bundle", "This is the BundleId found in the app's Info.plist file")]
        public string AppBundleId { get; set; }


        [ServiceProperty(RelativePath_ConfigKey, "Relative Path to Database", "Documents/DataStore", "This is the relative path to where the databases are in relation to the root of the application.")]
        public string RelativePathToData { get; set; }


        [ServiceProperty(SimulatorId_ConfigKey, "Simulator Id", "Some-Guid-Id", "This is the simulator id that you would like to target for scanning. These can be found in XCode. Use the string 'booted' for target the active simulator")]
        public string SimulatorId { get; set; }


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
                    RemoteRootDirectory = rootDbPath
                });
            }

            return list;
        }

        private string GetCurrentSimulatorDataPath()
        {
            var psi = BuildCurrentSimCommandCommand();
            var process = new Process();
            process.StartInfo = psi;
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

        public void InitiateService(ServiceInfo serviceInfo)
        {
            _serviceInfo = Guard.Argument(serviceInfo)
                                .NotNull()
                                .Value;

            _logger.LogInformation($"Initiating service {_serviceInfo.ServiceName} of type {_serviceInfo.ServiceTypeId}");

            AppBundleId = _serviceInfo.Properties.FirstOrDefault(prop => prop.Key == AppBundleId_ConfigKey)?.Value;
            SimulatorId = _serviceInfo.Properties.FirstOrDefault(prop => prop.Key == SimulatorId_ConfigKey)?.Value;
            RelativePathToData = _serviceInfo.Properties.FirstOrDefault(prop => prop.Key == RelativePath_ConfigKey)?.Value;
        }
    }
}