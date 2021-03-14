using DbViewer.Shared;
using DBViewer.Services;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace DBViewer.ViewModels
{
    public class RemoteDatabaseViewModel : ViewModelBase
    {
        private readonly IHubService _hubService;

        public RemoteDatabaseViewModel(IHubService hubService, DatabaseInfo dbInfo)
        {
            _hubService = hubService ?? throw new ArgumentNullException(nameof(hubService));

            DisplayName = dbInfo.DisplayDatabaseName;

            DownloadCommand = ReactiveCommand.CreateFromTask(ExecuteDownload);
        }

        public ReactiveCommand<Unit, Unit> DownloadCommand { get; }

        private string _displayName;
        public string DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }

        private async Task<Unit> ExecuteDownload()
        {
            return Unit.Default;
        }
    }
}
