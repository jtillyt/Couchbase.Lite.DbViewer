using System.Collections.Generic;

namespace DbViewer.Models
{
    public class CacheRegistry
    {
        public CacheRegistry()
        {
            DatabaseCollection = new List<CachedDatabase>();
        }

        public List<CachedDatabase> DatabaseCollection { get; set; }
    }
}
