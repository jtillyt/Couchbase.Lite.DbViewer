using System.Collections.Generic;

namespace DbViewer.Models
{
    public class CachedDatabaseRegistry
    {
        public CachedDatabaseRegistry()
        {
            DatabaseCollection = new List<CachedDatabase>();
        }

        public List<CachedDatabase> DatabaseCollection { get; set; }
    }
}