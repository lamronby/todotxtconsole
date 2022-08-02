using System;
using System.IO;
using System.Runtime.CompilerServices;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ClientConsole
{
    public class ConfigService
    {
        private string _configFilePath;
        
        public ToDoConfig ToDoConfig { get; private set; }

        public ConfigService(string configFilePath)
        {
            _configFilePath = configFilePath;
            this.IngestConfigFile(configFilePath);
        }

        public void SetConfig(
            string filePath,
            string archiveFilePath,
            SortType sortType = SortType.Priority,
            GroupByType groupByType = GroupByType.None,
            string filterText = null,
            bool listOnStart = true,
            bool listAfterCommand = false,
            int debugLevel = 0)
        {
            this.ToDoConfig = new ToDoConfig(
                filePath: filePath,
                archiveFilePath: archiveFilePath,
                sortType: sortType,
                groupByType: groupByType,
                filterText: filterText,
                listOnStart: listOnStart,
                listAfterCommand: listAfterCommand,
                debugLevel: debugLevel);
            PersistConfig();
        }

        private void IngestConfigFile(string filePath)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
            this.ToDoConfig = deserializer.Deserialize<ToDoConfig>(File.ReadAllText(filePath));
        }

        private void PersistConfig()
        {
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(this.ToDoConfig);
            File.WriteAllText(_configFilePath, yaml);
        }
    }
}
