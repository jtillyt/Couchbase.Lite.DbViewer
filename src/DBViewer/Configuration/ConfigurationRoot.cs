using System;
using System.Collections.Generic;
using System.Text;

namespace DbViewer.Configuration
{
    public class ConfigurationRoot
    {
        public SshRemoteSettings SshRemoteSettings {get;set; }
    }

    public class SshRemoteSettings
    {
        /// <summary>
        /// The host address (DNS resolvable name or IP) to connect to
        /// </summary>
        public string SSHHostAddress { get; set; }

        /// <summary>
        /// SSH username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// SSH password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The app id ex: com.myapp.app
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// The path from the root of the simulators app directory
        /// </summary>
        public string DevicePathToDbDir {get;set; }
    }
}
