using Dawn;
using DBViewer.Configuration;
using System.IO;

namespace DBViewer.Services
{
    public interface IDbFetchService
    {
        bool FetchDbToLocalPath(string localDirectory);
    }

    public class SshIosSimulatorDbFetchService : IDbFetchService
    {
        private readonly ConfigurationRoot _configuration;
        private Renci.SshNet.SshClient _sshClient;
        private Renci.SshNet.ScpClient _scpClient;

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

        public bool FetchDbToLocalPath(string localDirectory)
        {
            EnsureSshClientConnected();

            var dirInfo = new DirectoryInfo(localDirectory);
            var remotePath = Path.Combine(GetActiveSimulatorPath(),_configuration.SshRemoteSettings.DevicePathToDbDir);

            _scpClient = new Renci.SshNet.ScpClient(_sshClient.ConnectionInfo);
            _scpClient.Download(remotePath, dirInfo);

            return true;
        }

        private void EnsureSshClientConnected()
        {
            if (_sshClient == null)
            {
                Connect(_configuration.SshRemoteSettings.SSHHostAddress, _configuration.SshRemoteSettings.Username, _configuration.SshRemoteSettings.Password);
            }

            if (!_sshClient.IsConnected)
                throw new System.InvalidOperationException("SSH Connection could not be established");
        }
    }
}
