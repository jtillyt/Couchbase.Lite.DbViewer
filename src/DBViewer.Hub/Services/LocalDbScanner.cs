﻿using Dawn;
using DbViewer.Shared;
using DbViewer.Shared.Dtos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DbViewer.Hub.Services
{
    [ServiceType(ServiceConstants.LocalDatabaseScannerServiceTypeName, ServiceConstants.LocalDatabaseScannerServiceTypeId)]
    public class LocalDbScanner : IDbScanner, IService
    {
        private const string LocalPath_ConfigKey = "LocalDbDirectory";

        private readonly ILogger<LocalDbScanner> _logger;
        private ServiceInfo _serviceInfo;

        public LocalDbScanner(ILogger<LocalDbScanner> logger)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public void InitiateService(ServiceInfo serviceInfo)
        {
            _serviceInfo = Guard.Argument(serviceInfo)
                                .NotNull()
                                .Value;

            _logger.LogInformation($"Initiating service {serviceInfo.ServiceName} of type {serviceInfo.ServiceTypeId}");

            LocalDirectory = _serviceInfo.Properties.FirstOrDefault(prop => prop.Key == LocalPath_ConfigKey)?.Value;
        }

        [ServiceProperty(LocalPath_ConfigKey, "Local Scan Directory", "TestDatabase", "This is the path to root directory that contains Couchbase Lite database directories.")]
        public string LocalDirectory { get; set; }

        public IEnumerable<DatabaseInfo> Scan()
        {
            var list = new List<DatabaseInfo>();

            _logger.LogInformation($"DBRoot dir: {LocalDirectory}");

            if (!Directory.Exists(LocalDirectory))
            {
                _logger.LogWarning($"{LocalDirectory} does not exist");
                return list;
            }

            string[] directories = null;

            try
            {
                directories = Directory.GetDirectories(LocalDirectory);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading directory: {LocalDirectory}", ex);
            }

            if (!(directories?.Length > 0))
            {
                _logger.LogWarning($"{LocalDirectory} does not contain files or an error occured while reading");
                return list;
            }

            _logger.LogDebug($"Loading {directories.Length} DB directories from:{LocalDirectory}");

            foreach (var dir in Directory.GetDirectories(LocalDirectory))
            {
                var dbInfo = new DatabaseInfo()

                {
                    DisplayDatabaseName = Path.GetFileNameWithoutExtension(dir),
                    FullDatabaseName = Path.GetFileName(dir),
                    RemoteRootDirectory = LocalDirectory
                };

                list.Add(dbInfo);

                _logger.LogDebug($"Added database {dbInfo.DisplayDatabaseName} from directory {LocalDirectory}");
            }

            return list;
        }
    }
}
