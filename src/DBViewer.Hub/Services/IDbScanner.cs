using DbViewer.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbViewer.Hub.Services
{
    public interface IDbScanner
    {
        public IEnumerable<DatabaseInfo> Scan();
    }
}
