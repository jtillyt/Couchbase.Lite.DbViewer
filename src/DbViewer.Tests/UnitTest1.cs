using Couchbase.Lite;
using Couchbase.Lite.Query;
using DBViewer.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace DbViewer.Tests
{
    public class UnitTest1
    {
        private string _dbName = @"DbName";
        private string _dbDir = @"C:\temp\Dbs";

        [Fact]
        public void Test1()
        {
            var dataService = new DataService();
            var result = dataService.Connect(_dbDir, _dbName);
        }
    }
}