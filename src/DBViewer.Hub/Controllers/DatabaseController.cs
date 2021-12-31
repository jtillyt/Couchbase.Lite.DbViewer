using Dawn;
using DbViewer.Hub.Repos;
using DbViewer.Hub.Services;
using DbViewer.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace DbViewer.Hub.Controllers
{
    [ApiController]
    [Route("Databases")]
    public class DatabaseController : ControllerBase
    {
        private readonly ILogger<DatabaseController> _logger;
        private readonly IHubService _hubService;
        private readonly IDatabaseProviderRepository _databaseProviderRepository;

        public DatabaseController(ILogger<DatabaseController> logger, IHubService hubService, IDatabaseProviderRepository databaseProviderRepository)
        {
            _logger = Guard.Argument(logger, nameof(logger))
                  .NotNull()
                  .Value;

            _hubService = Guard.Argument(hubService, nameof(hubService))
                  .NotNull()
                  .Value;

            _databaseProviderRepository = Guard.Argument(databaseProviderRepository, nameof(databaseProviderRepository))
                                               .NotNull()
                                               .Value;
        }

        [HttpGet]
        public IEnumerable<DatabaseInfo> ListAllDbs()
        {
            _logger.LogInformation("Fetching DB Info");

            var hubInfo = _hubService.GetLatestHub();
            var scanners = _databaseProviderRepository.GetAllDbScanners(hubInfo);

            if (scanners == null)
                return Enumerable.Empty<DatabaseInfo>();

            var dbs = new List<DatabaseInfo>();

            foreach(var scanner in scanners)
            {
               dbs.AddRange(scanner.Scan());
            }

            return dbs;
        }

        [HttpGet("Name/{displayName}")]
        public Stream GetDatabase([FromRoute] string displayName)
        {
            var listDbs = ListAllDbs();

            var dbInfo = listDbs.FirstOrDefault(db => db.DisplayDatabaseName.Equals(displayName, StringComparison.OrdinalIgnoreCase));

            if (dbInfo == null)
                return null;

            var hubInfo = _hubService.GetLatestHub();

            var dbProvider = _databaseProviderRepository.GetOrCreate(dbInfo.ProviderInfo, hubInfo);
            var rootDatabasePath = dbProvider.GetCurrentDatabaseRootPath(dbInfo);

            var dbPath = Path.Combine(rootDatabasePath, dbInfo.FullDatabaseName);

            var zipPath = dbPath + ".zip";

            if (System.IO.File.Exists(zipPath))
            {
                System.IO.File.Delete(zipPath);
            }

            ZipFile.CreateFromDirectory(dbPath, zipPath);

            return !System.IO.File.Exists(zipPath) ? null : new FileStream(zipPath, FileMode.Open);
        }

        [HttpPut("Document/{documentInfo}")]
        public  DocumentInfo SaveDocument(DocumentInfo documentInfo)
        {
            var updateDocumentInfo = _hubService.SaveDocumentToDatabase(documentInfo);

            return updateDocumentInfo;
        }

        [HttpDelete("Document/{documentRequest}")]
        public bool DeleteDocument(DocumentRequest documentRequest)
        {
            var deleteResult = _hubService.DeleteDocument(documentRequest.DatabaseInfo, documentRequest.DocumentId);

            return deleteResult;
        }

        [HttpGet("Document")]
        public  DocumentInfo GetDocument([FromBody] DocumentRequest documentRequest)
        {
            var documentInfo = _hubService.GetDocumentById(documentRequest.DatabaseInfo, documentRequest.DocumentId);

            return documentInfo;
        }
    }
}
