using System.Collections.Generic;

namespace DbViewer.Models
{
    public class CacheRegistry
    {
        public List<CachedDatabase> DatabaseCollection {get;set; } = new List<CachedDatabase>();
    }
}
