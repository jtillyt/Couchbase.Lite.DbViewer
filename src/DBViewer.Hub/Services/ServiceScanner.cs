﻿using Dawn;
using DbViewer.Shared.Dtos;
using System.Collections.Generic;
using System.Reflection;

namespace DbViewer.Hub.Services
{
    public class ServiceScanner
    {
        public static List<ServiceDefinition> GetServicesForAssembly(Assembly assembly)
        {
            Guard.Argument(assembly)
                 .NotNull();

            var serviceTypes = new List<ServiceDefinition>();

            foreach (var type in assembly.GetTypes())
            {
                var serviceTypeAtt = type.GetCustomAttribute<ServiceTypeAttribute>();

                if (serviceTypeAtt == null)
                    continue;

                var serviceType = new ServiceDefinition();

                serviceType.FullyQualifiedAssemblyTypeName = type.AssemblyQualifiedName;
                serviceType.Id = serviceTypeAtt.Id;
                serviceType.Name = serviceTypeAtt.Name;

                foreach (var prop in type.GetProperties())
                {
                    var propAtt = prop.GetCustomAttribute<ServicePropertyAttribute>();

                    if (propAtt == null)
                        continue;

                    var serviceProp = new ServicePropertyInfo
                    {
                        Key = propAtt.Key,
                        Value = propAtt.DefaultValue,
                        DisplayName = propAtt.DisplayName,
                        Description = propAtt.Description
                    };

                    serviceType.Properties.Add(serviceProp);
                }

                serviceTypes.Add(serviceType);
            }

            return serviceTypes;
        }
    }
}
