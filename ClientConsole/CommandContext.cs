using System;
using ToDoLib;

namespace ClientConsole
{
    public class CommandContext 
    {
        public String FilePath { get; set; }

        public SortType SortType { get; set; }

        public GroupByType GroupByType { get; set; }

        public String ArchiveFilePath { get; set; }

        public TaskFilter Filter { get; set; }

        // Currently unused.
        public int DebugLevel { get; set; }
    }
}
