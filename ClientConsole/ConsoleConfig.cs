using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using CommonExtensions;
using ToDoLib;

namespace ClientConsole
{
    public class ConsoleConfig
    {
        private static readonly ConsoleConfig _config = new ConsoleConfig();

        /// <summary>
        /// Set method is exposed so that the instance can be swapped out for Unit Testing.
        /// NOTE: Cannot set Instance after it has been initialized, and calling Get will 
        /// automatically initialize the Instance.
        /// </summary>
        public static ConsoleConfig Instance
        {
            get { return _config; }
        }


        /// <summary>
        /// This will load settings from the App.config.
        /// </summary>
        private ConsoleConfig()
        {
            FilePath = ConfigurationManager.AppSettings["FilePath"];

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["CurrentGrouping"]))
                GroupByType = DotNetExtensions.ParseEnum<GroupByType>(ConfigurationManager.AppSettings["CurrentGrouping"], GroupByType.Project);

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["CurrentSort"]))
                SortType = DotNetExtensions.ParseEnum<SortType>(ConfigurationManager.AppSettings["CurrentSort"], SortType.None);

            ArchiveFilePath = ConfigurationManager.AppSettings["ArchiveFilePath"];

            FilterText = ConfigurationManager.AppSettings["FilterText"];

            Log.LogLevel = (ConfigurationManager.AppSettings["DebugLoggingOn"] == "true") ? 
                LogLevel.Debug : 
                LogLevel.Error;

        }

        #region Properties

        public String FilePath { get; set; }

        public SortType SortType { get; set; }

        public GroupByType GroupByType { get; set; }

        public String ArchiveFilePath { get; set; }

        public String FilterText { get; set; }

        #endregion

    }
}
