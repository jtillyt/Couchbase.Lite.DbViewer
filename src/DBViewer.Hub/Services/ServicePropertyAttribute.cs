using System;

namespace DbViewer.Hub.Services
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ServicePropertyAttribute : Attribute
    {
        public ServicePropertyAttribute(string key, string name, string defaultValue, string description)
        {
            Key = key;
            DisplayName = name;
            DefaultValue = defaultValue;
            Description = description;
        }

        public string Key { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string DefaultValue { get; set; }
    }
}
