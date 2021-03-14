using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBViewer.Hub.Services
{
    public interface IDbScanner
    {
        public IEnumerable<DatabaseInfo> Scan();
    }
}
