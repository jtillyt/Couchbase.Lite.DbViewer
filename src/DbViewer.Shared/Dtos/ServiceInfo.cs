using System.Collections.Generic;

namespace DbViewer.Shared.Dtos
{
    public class ServiceInfo
    {
        public string Id { get; set; }

        public string ServiceTypeId { get; set; }

        public string ServiceName { get; set; }

        public List<ServicePropertyInfo> Properties { get; set; } = new List<ServicePropertyInfo>();


    }
}
