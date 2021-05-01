using DbViewer.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;

namespace DBViewer.Hub.Services
{
    public class LocalDbScanner : IDbScanner
    {
        private const string RootConfig_ConfigKey = "LocalDbScannerOptions";
        private const string LocalPath_ConfigKey = "LocalDbDirectory";

        private readonly IConfigurationSection _configSection;
        private readonly ILogger<LocalDbScanner> _logger;
        private readonly string _path;

        public LocalDbScanner(IConfiguration configuration, ILogger<LocalDbScanner> logger)
        {
            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));

            _configSection = configuration.GetSection(RootConfig_ConfigKey);

            _path = _configSection[LocalPath_ConfigKey];
        }

        public IEnumerable<DatabaseInfo> Scan()
        {
            var list = new List<DatabaseInfo>();

            _logger.LogInformation($"DBRoot dir: {_path}");

            if (!Directory.Exists(_path))
                return list;

            foreach (var dir in Directory.GetDirectories(_path))
            {
               list.Add(new DatabaseInfo()
               {
                    DisplayDatabaseName = Path.GetFileNameWithoutExtension(dir),
                    FullDatabaseName = Path.GetFileName(dir),
                    RemoteRootDirectory = _path
               }); 
            }

            return list;
        }
    }
}
