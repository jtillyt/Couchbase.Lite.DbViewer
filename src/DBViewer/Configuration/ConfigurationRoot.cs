using System;
using System.Collections.Generic;
using System.Text;

namespace DBViewer.Configuration
{
    public class ConfigurationRoot
    {
        public SshRemoteSettings SshRemoteSettings {get;set; }
    }

    public class SshRemoteSettings
    {
        public string SSHHostAddress { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string AppId { get; set; }
    }
}
