using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace ClientConsole
{
    public class ConfigService : IConfigService
    {
        private Dictionary<string, string> _todoConfig;

        public ConfigService()
        {
            _todoConfig = new Dictionary<string, string>();
            var filePath = @"C:\src\thirdparty\Todotxtconsole\ClientConsole\ClientConsoleConfig.yaml";

            this.IngestConfigFile(filePath);
        }

        public string GetValue(string key)
        {
            string value;
            if ( !_todoConfig.TryGetValue(key, out value))
                return null;

            return value;
        }

        public void SetValue(string key, string value)
        {
            _todoConfig[key] = value;
            // TODO Persist
        }

        private void IngestConfigFile( string filePath )
        {
            // Load the stream
            var yaml = new YamlStream();
            yaml.Load( File.OpenText( filePath ) );

            // Examine the stream
            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

            foreach ( var entry in mapping.Children )
                _todoConfig.Add( ((YamlScalarNode)entry.Key).Value, ((YamlScalarNode)entry.Value).Value );
        }
    }
}
