using System;

namespace DbViewer.Hub.Services
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceTypeAttribute : Attribute
    {
        public ServiceTypeAttribute(string name, string id)
        {
            Name = name;
            Id = id;
        }

        public string Name { get; set; }

        public string Id { get; set; }
    }
}
