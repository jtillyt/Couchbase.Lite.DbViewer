using Dawn;
using DBViewer.Configuration;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Diagnostics;
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
            var client = new SftpClient(
                _configuration.SshRemoteSettings.SSHHostAddress,
                _configuration.SshRemoteSettings.Username,
                _configuration.SshRemoteSettings.Password);

            client.ErrorOccurred += ScpClient_ErrorOccurred;
            client.HostKeyReceived += ScpClient_HostKeyReceived;

            client.Connect();

            if (!client.IsConnected)
                return false;

            try
            {
                var rootItem = client.Get(remoteDirectory);
                RecurseAndCopy(client, rootItem, remoteDirectory.TrimEnd('/'), localDirectory);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return true;
        }

        private void RecurseAndCopy(SftpClient client, SftpFile dirItem, string remoteDir, string localDir)
        {
            foreach (var subItem in client.ListDirectory(dirItem.FullName))
            {
                var relativeRemotePath = subItem.FullName.Substring(remoteDir.Length + 1);//One past zero index + '/'
                var relativeLocalPath = Path.Combine(localDir, relativeRemotePath);

                if (relativeRemotePath.EndsWith(".") || relativeRemotePath.EndsWith(".."))
                    continue;

                if (subItem.IsDirectory)
                {
                    if (!Directory.Exists(relativeLocalPath))
                        Directory.CreateDirectory(relativeLocalPath);

                    RecurseAndCopy(client, subItem, remoteDir, localDir);

                }
                else if (subItem.IsRegularFile)
                {
                    using (var fs = new FileStream(relativeLocalPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        try
                        {
                            client.DownloadFile(subItem.FullName, fs);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }
            }
        }

        private void ScpClient_HostKeyReceived(object sender, Renci.SshNet.Common.HostKeyEventArgs e)
        {
            Console.WriteLine(e);
        }

        private void ScpClient_ErrorOccurred(object sender, Renci.SshNet.Common.ExceptionEventArgs ex)
        {
            Console.WriteLine(ex.Exception);
        }

        // TODO: <James Thomas: 3/10/2021> Stream to observable 
        private void ScpClient_Downloading(object sender, Renci.SshNet.Common.ScpDownloadEventArgs e)
        {
            Console.WriteLine(e);
        }
    }
}
