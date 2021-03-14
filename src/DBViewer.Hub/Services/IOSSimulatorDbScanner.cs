using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace DBViewer.Hub.Services
{
    public class IOSSimulatorDbScanner : IDbScanner
    {
        private const string RootConfig_ConfigKey = "IOSSimulatorDbScannerOptions";
        private const string RelativePath_ConfigKey = "DataRelativePath";
        private const string AppBundleId_ConfigKey = "AppBundleId";

        private readonly string AppBundleId;
        private readonly string RelativePathToData;

        private readonly IConfigurationSection _configSection;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IOSSimulatorDbScanner> _logger;

        public IOSSimulatorDbScanner(IConfiguration configuration, ILogger<IOSSimulatorDbScanner> logger)
        {
            _configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));

            _configSection = configuration.GetSection(RootConfig_ConfigKey);

            AppBundleId = _configSection[AppBundleId_ConfigKey];
            RelativePathToData = _configSection[RelativePath_ConfigKey];
        }

        public IEnumerable<DatabaseInfo> Scan()
        {
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
            var argumentString = $"simctl get_app_container booted {AppBundleId} data";

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
    }
}