using DbViewer.Hub.Services;

namespace DbViewer.Shared.Configuration
{
    [ServiceType("Test-Service", "test-service-type-id")]
    public class TestService
    {
        public TestService()
        {

        }

        [ServiceProperty("test-property-one","Test Property One", "Default Value","Description 1")]
        public string TestServicePropertyOne { get; set; }

        [ServiceProperty("test-property-two","Test Property Two", "Another Default Value", "Description 2")]
        public string TestServicePropertyTwo { get; set; }
    }
}
