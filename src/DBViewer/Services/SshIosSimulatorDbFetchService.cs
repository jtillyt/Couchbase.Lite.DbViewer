using Dawn;
using DBViewer.Configuration;
using System.IO;

namespace DBViewer.Services
{
   

    public class SshIosSimulatorDbCopyService : IDbCopyService
    {
        private readonly ConfigurationRoot _configuration;
        private Renci.SshNet.SshClient _sshClient;

        public SshIosSimulatorDbCopyService(IConfigurationService configurationService)
        {
            Guard.Argument(configurationService, nameof(configurationService))
                                        .NotNull();

            _configuration = configurationService.GetConfigRoot();
        }

       

        public bool CopyDbToLocalPath(string localDirectory, string remoteDirectory)
        {

            var dirInfo = new DirectoryInfo(localDirectory);

            var scpClient = new Renci.SshNet.ScpClient(_sshClient.ConnectionInfo);
            scpClient.Download(remoteDirectory, dirInfo);

            return true;
        }

        //TODO: This works but needs tweaks so we are assured we get the current sim
        //from which we can then use a relative path configured by the user
        private bool Connect(string hostAddress, string username, string password)
        {
            if (_sshClient == null)
                _sshClient = new Renci.SshNet.SshClient(hostAddress, username, password);

            _sshClient.Connect();

            if (_sshClient.IsConnected)
                return false;

            return true;
        }

        //private string GetActiveSimulatorPath()
        //{
        //    EnsureSshClientConnected();

        //    var cmdText = $"xcrun simctl get_app_container booted {_configuration.SshRemoteSettings.AppId}";
        //    var results = _sshClient.RunCommand(cmdText);

        //    return results.CommandText;
        //}

        //private void EnsureSshClientConnected()
        //{
        //    if (_sshClient == null)
        //    {
        //        Connect(_configuration.SshRemoteSettings.SSHHostAddress, _configuration.SshRemoteSettings.Username, _configuration.SshRemoteSettings.Password);
        //    }

        //    if (!_sshClient.IsConnected)
        //        throw new System.InvalidOperationException("SSH Connection could not be established");
        //}
    }
}
