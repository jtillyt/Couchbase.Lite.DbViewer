using Newtonsoft.Json;
using System.IO;

namespace DBViewer.Configuration
{
    public interface IConfigurationService
    {
        /// <summary>
        /// Gets the latest in memory representation of the configuration. This may be different than the settings loaded on start.
        /// </summary>
        ConfigurationRoot Configuration { get; }

        /// <summary>
        /// Gets or resets the current configuration with the configuration set before running.
        /// </summary>
        /// <returns></returns>
        ConfigurationRoot GetConfigRoot();
    }

    public class ConfigurationService : IConfigurationService
    {
        private ConfigurationRoot _configuration;

        /// <inheritdoc/>
        public ConfigurationRoot Configuration
        {
            get
            {
                if (_configuration == null)
                    _configuration = GetConfigRoot();

                return _configuration;
            }
        }

        /// <inheritdoc/>
        public ConfigurationRoot GetConfigRoot()
        {
            ConfigurationRoot config;
            var rootNamespace = typeof(App).Assembly.GetName();
            var embeddedConfigurationStream = this.GetType().Assembly.GetManifestResourceStream($"{rootNamespace}.appsettings.json");

            if (embeddedConfigurationStream == null)
            {
                throw new FileNotFoundException(nameof(embeddedConfigurationStream));
            }

            using (var streamReader = new StreamReader(embeddedConfigurationStream))
            {
                var json = streamReader.ReadToEnd();
                config = JsonConvert.DeserializeObject<ConfigurationRoot>(json);

                _configuration = config;
            }

            return config;
        }
    }
}
