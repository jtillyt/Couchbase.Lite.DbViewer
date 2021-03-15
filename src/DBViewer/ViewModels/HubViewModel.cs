using Dawn;
using DbViewer.Shared;
using DBViewer.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DBViewer.ViewModels
{
    public class HubViewModel : ViewModelBase
    {
        private const string LastHubAddressKey = "LastHubAddress";
        private readonly IHubService _hubService;

        public HubViewModel(IHubService hubService)
        {
            _hubService = Guard.Argument(hubService, nameof(hubService))
                               .NotNull()
                               .Value;

            ListAllDatabasesCommand = ReactiveCommand.CreateFromTask(ExecuteListAllDatabases);
            DownloadCheckedCommand = ReactiveCommand.CreateFromTask(ExecuteDownloadChecked);

            // TODO: <James Thomas: 3/14/21> Move to DI 
            HubAddress = Preferences.Get(LastHubAddressKey, "");
        }

        public ReactiveCommand<Unit, Unit> ListAllDatabasesCommand { get; }
        public ReactiveCommand<Unit, Unit> DownloadCheckedCommand { get; }

        public string HubAddress
        {
            get => _hubAddress;
            set => this.RaiseAndSetIfChanged(ref _hubAddress, value);
        }

        public ObservableCollection<RemoteDatabaseViewModel> RemoteDatabases
        {
            get => _remoteDatabases;
            set => this.RaiseAndSetIfChanged(ref _remoteDatabases, value);
        }


        private async Task ExecuteListAllDatabases()
        {
            if (string.IsNullOrEmpty(HubAddress))
                return;

            var hubUri = new Uri(HubAddress);

            _hubService.EnsureConnection(hubUri);
            Preferences.Set(LastHubAddressKey, HubAddress);

            IEnumerable<DatabaseInfo> dbList = null;

            try
            {
                dbList = await _hubService.ListAll();
            }
            catch (Exception ex)
            {
                //Wire up logging
            }

            if (dbList == null)
                return;

            var viewModels = dbList.Select(db => new RemoteDatabaseViewModel(db));

            RunOnUi(() =>
            {
                RemoteDatabases.Clear();

                foreach (var vm in viewModels)
                {
                    RemoteDatabases.Add(vm);
                }
            });
        }

        private async Task ExecuteDownloadChecked()
        {
            foreach(var vm in RemoteDatabases)
            {
                if (vm.ShouldDownload)
                {
                    _hubService.DownloadDatabase(vm.DatabaseInfo);
                }
            }
        }

        private string _hubAddress;
        private ObservableCollection<RemoteDatabaseViewModel> _remoteDatabases = new ObservableCollection<RemoteDatabaseViewModel>();
    }
}
