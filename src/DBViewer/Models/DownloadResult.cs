using DbViewer.Shared;

namespace DBViewer.Models
{
    public class DownloadResult
    {
        public DatabaseInfo DatabaseInfo { get; set; }
        public bool WasSuccesful { get; set; }
    }
}
