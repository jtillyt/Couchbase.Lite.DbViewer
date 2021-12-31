using DbViewer.Shared.Dtos;
using System.Collections.Generic;

namespace DbViewer.Hub.DbProvider
{
    public interface IDbProvider
    {
        public string Id { get; }

        string GetCurrentDatabaseRootPath(DatabaseInfo databaseInfo);

        public IEnumerable<DatabaseInfo> Scan();
    }
}
