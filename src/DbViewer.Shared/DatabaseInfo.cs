using System.Collections.Generic;

namespace DbViewer.Shared
{
    public class DatabaseInfo
    {
        public string RemoteRootDirectory { get; set; }

        public string DisplayDatabaseName { get; set; }

        public string FullDatabaseName { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is DatabaseInfo dbInfo))
            {
                return false;
            }

            return dbInfo.DisplayDatabaseName == DisplayDatabaseName;
        }

        public override int GetHashCode()
        {
            int hashCode = -2073412333;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(RemoteRootDirectory);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DisplayDatabaseName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FullDatabaseName);
            return hashCode;
        }
    }
}
