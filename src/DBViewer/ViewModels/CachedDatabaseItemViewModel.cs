using Dawn;
using DBViewer.Models;
using ReactiveUI;
using System;

namespace DBViewer.ViewModels
{
    public class CachedDatabaseItemViewModel : ViewModelBase
    {
        public CachedDatabaseItemViewModel(CachedDatabase cachedDatabase)
        {
            Database = Guard.Argument(cachedDatabase, nameof(cachedDatabase))
                                        .NotNull()
                                        .Value;

            DisplayName = Database.RemoteDatabaseInfo?
                                        .DisplayDatabaseName;

            DownloadTime = Database.DownloadTime;
        }

        public CachedDatabase Database { get; set; }

        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }

        private DateTimeOffset _downloadTime;
        public DateTimeOffset DownloadTime
        {
            get => _downloadTime;
            set => this.RaiseAndSetIfChanged(ref _downloadTime, value);
        }
    }
}
