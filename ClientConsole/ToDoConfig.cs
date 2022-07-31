using System;

namespace ClientConsole
{
    public class ToDoConfig
    {
        public string FilePath { get; }

        public string ArchiveFilePath { get; }

        public SortType SortType { get; }

        public GroupByType GroupByType { get; }

        public string FilterText { get; }

        public bool ListOnStart { get; }

        public bool ListAfterCommand { get; }

        public int DebugLevel { get; }

        public ToDoConfig(
            string filePath,
            string archiveFilePath,
            SortType sortType = SortType.Priority,
            GroupByType groupByType = GroupByType.None,
            string filterText = null,
            bool listOnStart = true,
            bool listAfterCommand = false,
            int debugLevel = 0)
        {
            FilePath = filePath;
            ArchiveFilePath = archiveFilePath;
            SortType = sortType;
            GroupByType = groupByType;
            FilterText = filterText;
            ListOnStart = listOnStart;
            ListAfterCommand = listAfterCommand;
            DebugLevel = debugLevel;
        }
    }
}