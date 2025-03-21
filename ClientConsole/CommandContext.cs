using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToDoLib;
using Serilog;
using Serilog.Events;
using System;

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

        public bool DisplayBeforeThresholdDate { get; set; }

        public Dictionary<string, string> OtherConfig { get; set; }

        public override string ToString( )
        {
            var otherConfig = this.OtherConfig == null ? "" : string.Join(", ", this.OtherConfig.Select(kv => $"{kv.Key}: {kv.Value}"));
            
            return $"Filter: {this.Filter}, GroupByType: {this.GroupByType}, ListAfterCommand? {this.ListAfterCommand}, ListOnStart? {this.ListOnStart}, SortType: {this.SortType}, DisplayBeforeThresholdDate: {this.DisplayBeforeThresholdDate}. OtherConfig: {otherConfig}";
        }
    }
}
