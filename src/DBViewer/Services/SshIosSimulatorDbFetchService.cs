using Dawn;
using DBViewer.Configuration;

namespace DBViewer.Services
{
    public interface IDbFetchService
    {
        void CopyRemoteDbToLocalPath();
    }

    public class SshIosSimulatorDbFetchService : IDbFetchService
    {
        private readonly ConfigurationRoot _configuration;
        private Renci.SshNet.SshClient _sshClient;

        public SshIosSimulatorDbFetchService(IConfigurationService configurationService)
        {
             Guard.Argument(configurationService, nameof(configurationService))
                                         .NotNull();

            _configuration = configurationService.GetConfigRoot();
        }

        public bool Connect(string hostAddress, string username, string password)
        {
            if (_sshClient == null)
                _sshClient = new Renci.SshNet.SshClient(hostAddress, username, password);

            _sshClient.Connect();

            if (_sshClient.IsConnected)
                return false;

            return true;
        }

        public string GetActiveSimulatorPath()
        {
            EnsureSshClientConnected();

            var cmdText = $"xcrun simctl get_app_container booted {_configuration.SshRemoteSettings.AppId}";
            var results = _sshClient.RunCommand(cmdText);

            return results.CommandText;
        }

        public void CopyRemoteDbToLocalPath()
        {
            
        }

        private void EnsureSshClientConnected()
        {
            if (_sshClient == null)
            {
                Connect(_configuration.SshRemoteSettings.SSHHostAddress, _configuration.SshRemoteSettings.Username, _configuration.SshRemoteSettings.Password);
            }
        }
    }
}
