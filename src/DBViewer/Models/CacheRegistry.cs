using System.Collections.Generic;

namespace DBViewer.Models
{
    public class CacheRegistry
    {
        public List<CachedDatabase> DatabaseCollection {get;set; } = new List<CachedDatabase>();
    }
}
