using DbViewer.Shared.Dtos;

namespace DbViewer.Hub.Services
{
    public interface IService
    {
        public void InitiateService(ServiceInfo serviceInfo, HubInfo hubInfo);
    }
}
