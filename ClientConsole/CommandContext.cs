using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientConsole
{
    public class CommandContext 
    {
        public String FilePath { get; set; }

        public SortType SortType { get; set; }

        public GroupByType GroupByType { get; set; }

        public String ArchiveFilePath { get; set; }

        // Currently unused.
        public int DebugLevel { get; set; }

    }
}
