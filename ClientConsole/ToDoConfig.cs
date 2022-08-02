using System;

namespace ClientConsole
{
    public class ToDoConfig
    {
        public string FilePath { get; set;  }

        public string ArchiveFilePath { get; set; }

        public string RecurFilePath { get; set;  }

        public SortType SortType { get; set;  }

        public GroupByType GroupByType { get; set;  }

        public string FilterText { get; set; }

        public bool ListOnStart { get; set; }

        public bool ListAfterCommand { get; set; }

        public bool DisplayBeforeThresholdDate { get; set; }

        public int DebugLevel { get; set; }

        public ToDoConfig() { }
        
        public ToDoConfig(
            string filePath,
            string archiveFilePath,
            SortType sortType = SortType.Priority,
            GroupByType groupByType = GroupByType.None,
            string filterText = null,
            bool listOnStart = true,
            bool listAfterCommand = false,
            bool displayBeforeThresholdDate = false,
            int debugLevel = 0)
        {
            FilePath = filePath;
            ArchiveFilePath = archiveFilePath;
            SortType = sortType;
            GroupByType = groupByType;
            FilterText = filterText;
            ListOnStart = listOnStart;
            ListAfterCommand = listAfterCommand;
            DisplayBeforeThresholdDate = displayBeforeThresholdDate;
            DebugLevel = debugLevel;
        }
    }
}