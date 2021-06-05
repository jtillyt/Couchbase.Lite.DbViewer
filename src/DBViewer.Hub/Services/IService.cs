using DbViewer.Shared.Configuration;

namespace DbViewer.Hub.Services
{
    public interface IService
    {
        public void InitiateService(ServiceInfo serviceInfo);
    }
}
