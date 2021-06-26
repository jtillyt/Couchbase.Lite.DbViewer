using DbViewer.Hub.Services;
using DbViewer.Shared.Configuration;
using DbViewer.Shared.Dtos;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Linq;
using Xunit;

namespace DbViewer.Hub.Tests.Configuration
{
    public class ConfigurationTests
    {
        [Fact]
        public void Save_WhenConfigurationHasServices_ThenSerializedWithoutException()
        {
            var hub = BuildSimpleHub();

            var service1 = BuildSimpleService(1);
            service1.Properties.Add(new ServicePropertyInfo() { Key = "Key1", Value = "Key1ValueService1" });
            service1.Properties.Add(new ServicePropertyInfo() { Key = "Key2", Value = "Key2ValueService1" });

            var service2 = BuildSimpleService(2);
            service2.Properties.Add(new ServicePropertyInfo() { Key = "Key1", Value = "Key1ValueService2" });
            service2.Properties.Add(new ServicePropertyInfo() { Key = "Key2", Value = "Key2ValueService2" });

            hub.ActiveServices.Add(service1);
            hub.ActiveServices.Add(service2);

            var hubJson = JsonConvert.SerializeObject(hub);

            var receivedHub = JsonConvert.DeserializeObject<HubInfo>(hubJson);

            receivedHub.Should()
                       .NotBeNull();

            receivedHub.HubName
                       .Should()
                       .BeEquivalentTo(hub.HubName);

            receivedHub.Id
                       .Should()
                       .BeEquivalentTo(hub.Id);

            receivedHub.ActiveServices
                       .Should()
                       .NotBeNullOrEmpty();

            receivedHub.ActiveServices
                       .Count
                       .Should()
                       .Be(hub.ActiveServices.Count);
        }

        [Fact]
        public void ServiceScanner_WhenScanningAssemblyWithService_ServiceDefinitionReturned()
        {
           var assem = GetType().Assembly;
           
            var serviceTypes = ServiceScanner.GetServicesForAssembly(assem);

            serviceTypes.Should()
                       .NotBeNullOrEmpty();

            var serviceType = serviceTypes.FirstOrDefault();

            serviceType.Id
                       .Should()
                       .NotBeNullOrWhiteSpace();

            serviceType.Name
                       .Should()
                       .NotBeNullOrWhiteSpace();

            serviceType.Properties
                       .Should()
                       .NotBeNullOrEmpty();
        }

        public static HubInfo BuildSimpleHub(uint mockId = 1)
        {
            var configuration = new HubInfo();

            configuration.Id = Guid.NewGuid().ToString();
            configuration.HubName = $"Test Hub {mockId}";

            return configuration;
        }

        public static ServiceInfo BuildSimpleService(uint mockId = 1)
        {
            var service = new ServiceInfo();

            service.Id = Guid.NewGuid().ToString();
            service.ServiceName = $"Test Service {mockId}";

            return service;
        }
    }
}
