using System.Collections.Generic;

namespace DbViewer.Shared.Configuration
{
    public class HubInfo
    {
        public string Id { get; set; }

        public string HubName { get; set; }

        public string HostAddress { get; set; }

        public List<ServiceInfo> ActiveServices { get; set; } = new List<ServiceInfo>();

        public List<ServiceDefinition> ServiceDefinitions { get; set; } = new List<ServiceDefinition>();
    }
}
