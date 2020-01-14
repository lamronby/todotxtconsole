﻿using System;
using System.Collections.Generic;
using System.IO;
using ToDoLib;

namespace ClientConsole
{
    public class CommandContext 
    {
        public TaskList TaskList { get; set; }

        public List<Task> TasksToArchive { get; } = new List<Task>();
        
        public SortType SortType { get; set; }

        public GroupByType GroupByType { get; set; }

        public TaskFilter Filter { get; set; }

        public bool ListOnStart { get; set; }

        public bool ListAfterCommand { get; set; }

        public Dictionary<string, string> OtherConfig { get; set; }

        public int DebugLevel
        {
            get { return (int) Log.LogLevel; }
            set { Log.LogLevel = (LogLevel) value; }
        }

        public override string ToString( )
        {
            return String.Format( "SortType: {0}, GroupByType: {1}",
                this.SortType,
                this.GroupByType );
        }
    }
}
