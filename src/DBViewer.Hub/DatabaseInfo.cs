using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBViewer.Hub
{
    public class DatabaseInfo
    {
        public string RemoteRootDirectory { get;set; }
        public string DisplayDatabaseName { get;set; }
        
        public string FullDatabaseName { get; set; }
    }
}
