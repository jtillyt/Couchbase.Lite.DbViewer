using Dawn;
using DBViewer.Configuration;
using System.IO;

namespace DBViewer.Services
{
    public class SshDbFetchService : IDbCopyService
    {
        private readonly ConfigurationRoot _configuration;

        public SshDbFetchService(IConfigurationService configurationService)
        {
            Guard.Argument(configurationService, nameof(configurationService))
                                        .NotNull();

            _configuration = configurationService.GetConfigRoot();
        }

        public bool CopyDbToLocalPath(string localDirectory, string remoteDirectory)
        {
            var dirInfo = new DirectoryInfo(localDirectory);

            var scpClient = new Renci.SshNet.ScpClient(
                _configuration.SshRemoteSettings.SSHHostAddress,
                _configuration.SshRemoteSettings.Username,
                _configuration.SshRemoteSettings.Password);

            scpClient.Download(remoteDirectory, dirInfo);

            return true;
        }
    }
}
