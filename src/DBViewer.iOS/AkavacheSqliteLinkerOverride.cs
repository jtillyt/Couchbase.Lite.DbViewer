using Akavache.Sqlite3;
using System;

namespace DbViewer.iOS
{
    [Preserve]
    public static class LinkerPreserve
    {
        static LinkerPreserve()
        {
            var persistentName = typeof(SQLitePersistentBlobCache).FullName;
            var encryptedName = typeof(SQLiteEncryptedBlobCache).FullName;
        }
    }


    public class PreserveAttribute : Attribute
    {
    }
}