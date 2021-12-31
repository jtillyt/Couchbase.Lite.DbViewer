using System;
using System.Collections.Generic;

namespace DbViewer.Shared.Dtos
{
    public class DatabaseInfo
    {
        public string DisplayDatabaseName { get; set; }

        public string FullDatabaseName { get; set; }

        public ServiceInfo ProviderInfo { get; set; }

        public string HubId { get; set; }

        [Obsolete("We should be using the data from the hub id")]
        public Uri RequestAddress { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is DatabaseInfo dbInfo))
            {
                return false;
            }

            return dbInfo.DisplayDatabaseName == DisplayDatabaseName &&
                   dbInfo.FullDatabaseName == FullDatabaseName &&
                   dbInfo.ProviderInfo.Id == ProviderInfo.Id &&
                   dbInfo.HubId == dbInfo.HubId;
        }

        public override int GetHashCode()
        {
            int hashCode = -2073412333;
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(HubId);
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(ProviderInfo.Id);
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(DisplayDatabaseName);
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(FullDatabaseName);
            return hashCode;
        }
    }
}
