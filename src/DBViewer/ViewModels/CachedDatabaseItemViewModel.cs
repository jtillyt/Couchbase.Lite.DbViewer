using Dawn;
using DbViewer.Models;
using ReactiveUI;
using System;

namespace DbViewer.ViewModels
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

            var dateTime = Database.DownloadTime.DateTime;
            DownloadTime = $"{dateTime.ToShortDateString()} {dateTime.ToShortTimeString()}";
        }

        public CachedDatabase Database { get; set; }

        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }

        private string _downloadTime;
        public string DownloadTime
        {
            get => _downloadTime;
            set => this.RaiseAndSetIfChanged(ref _downloadTime, value);
        }
    }
}
