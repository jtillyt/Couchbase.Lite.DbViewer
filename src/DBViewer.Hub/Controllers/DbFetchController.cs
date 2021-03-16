using DbViewer.Shared;
using DBViewer.Hub.Services;
using ImTools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace DBViewer.Hub.Controllers
{
    [ApiController]
    [Route("Databases")]
    public class DbFetchController : ControllerBase
    {
        private readonly ILogger<DbFetchController> _logger;
        private readonly IDbScanner _dbScanner;

        public DbFetchController(ILogger<DbFetchController> logger, IDbScanner dbScanner)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbScanner = dbScanner ?? throw new ArgumentNullException(nameof(dbScanner));
        }

        [HttpGet]
        public IEnumerable<DatabaseInfo> ListAllDbs()
        {
            _logger.LogInformation("Fetching DB Info");
            return _dbScanner.Scan();
        }

        [HttpGet("Name/{displayName}")]
        public Stream GetDatabase([FromRoute] string displayName)
        {
            var listDbs = ListAllDbs();

            var dbInfo = listDbs.FindFirst(db => db.DisplayDatabaseName.Equals(displayName, StringComparison.OrdinalIgnoreCase));

            if (dbInfo == null)
                return null;

            var dbPath = Path.Combine(dbInfo.RemoteRootDirectory, dbInfo.FullDatabaseName);
            var zipPath = dbPath + ".zip";

            if (System.IO.File.Exists(zipPath))
            {
                System.IO.File.Delete(zipPath);
            }

            ZipFile.CreateFromDirectory(dbPath, zipPath);

            return !System.IO.File.Exists(zipPath) ? null : new FileStream(zipPath, FileMode.Open);
        }
    }
}
