using System.Collections.Generic;

namespace DbViewer.Shared.Dtos
{
    public class ServiceDefinition
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string FullyQualifiedAssemblyTypeName { get; set; }

        public List<ServicePropertyInfo> Properties { get; set; } = new List<ServicePropertyInfo>();
    }
}
