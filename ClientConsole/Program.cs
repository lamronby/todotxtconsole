using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ClientConsole.Commands;
using ClientConsole.Views;
using CommandLine;
using Mono.Options;
using ToDoLib;

namespace ClientConsole
{
    class Program
    {
        private class Options
        {
            [Option('f', "config_file", Required = true, HelpText = "Path to a config file.")]
            public string ConfigFile { get; set; }
        }
        #region todo reference

        /*
          Actions:
            DONE
            add "THING I NEED TO DO +project @context"
            a "THING I NEED TO DO +project @context"
              Adds THING I NEED TO DO to your todo.txt file on its own line.
              Project and context notation optional.
              Quotes optional.

            NOT SUPPORTED
            addm "FIRST THING I NEED TO DO +project1 @context
            SECOND THING I NEED TO DO +project2 @context"
              Adds FIRST THING I NEED TO DO to your todo.txt on its own line and
              Adds SECOND THING I NEED TO DO to you todo.txt on its own line.
              Project and context notation optional.
              Quotes optional.

            NOT SUPPORTED
            addto DEST "TEXT TO ADD"
              Adds a line of text to any file located in the todo.txt directory.
              For example, addto inbox.txt "decide about vacation"

            NOT SUPPORTED
            append ITEM# "TEXT TO APPEND"
            app ITEM# "TEXT TO APPEND"
              Adds TEXT TO APPEND to the end of the task on line ITEM#.
              Quotes optional.

            UNNEEDED
            archive
              Moves all done tasks from todo.txt to done.txt and removes blank lines.

            UNNEEDED
            command [ACTIONS]
              Runs the remaining arguments using only todo.sh builtins.
              Will not call any .todo.actions.d scripts.

            DONE
            del ITEM# [TERM]
            rm ITEM# [TERM]
              Deletes the task on line ITEM# in todo.txt.
              If TERM specified, deletes only TERM from the task.

            NOT SUPPORTED
            depri ITEM#[, ITEM#, ITEM#, ...]
            dp ITEM#[, ITEM#, ITEM#, ...]
              Deprioritizes (removes the priority) from the task(s)
              on line ITEM# in todo.txt.

            DONE
            do ITEM#[, ITEM#, ITEM#, ...]
              Marks task(s) on line ITEM# as done in todo.txt.

            help
              Display this help message.

            DONE
            list [TERM...]
            ls [TERM...]
              Displays all tasks that contain TERM(s) sorted by priority with line
              numbers.  If no TERM specified, lists entire todo.txt.

            NOT SUPPORTED
            listall [TERM...]
            lsa [TERM...]
              Displays all the lines in todo.txt AND done.txt that contain TERM(s)
              sorted by priority with line  numbers.  If no TERM specified, lists
              entire todo.txt AND done.txt concatenated and sorted.

            NOT SUPPORTED
            listcon
            lsc
              Lists all the task contexts that start with the @ sign in todo.txt.

            UNNEEDED?
            listfile SRC [TERM...]
            lf SRC [TERM...]
              Displays all the lines in SRC file located in the todo.txt directory,
              sorted by priority with line  numbers.  If TERM specified, lists
              all lines that contain TERM in SRC file.

            NOT SUPPORTED
            listpri [PRIORITY] [TERM...]
            lsp [PRIORITY] [TERM...]
              Displays all tasks prioritized PRIORITY.
              If no PRIORITY specified, lists all prioritized tasks.
              If TERM specified, lists only prioritized tasks that contain TERM.

            NOT SUPPORTED
            listproj
            lsprj
              Lists all the projects that start with the + sign in todo.txt.

            NOT SUPPORTED
            move ITEM# DEST [SRC]
            mv ITEM# DEST [SRC]
              Moves a line from source text file (SRC) to destination text file (DEST).
              Both source and destination file must be located in the directory defined
              in the configuration directory.  When SRC is not defined
              it's by default todo.txt.

            NOT SUPPORTED
            prepend ITEM# "TEXT TO PREPEND"
            prep ITEM# "TEXT TO PREPEND"
              Adds TEXT TO PREPEND to the beginning of the task on line ITEM#.
              Quotes optional.

            DONE
            pri ITEM# PRIORITY
            p ITEM# PRIORITY
              Adds PRIORITY to task on line ITEM#.  If the task is already
              prioritized, replaces current priority with new PRIORITY.
              PRIORITY must be an uppercase letter between A and Z.

            DONE
            replace ITEM# "UPDATED TODO"
              Replaces task on line ITEM# with UPDATED TODO.

            report
              Adds the number of open tasks and done tasks to report.txt.

            NEW
            filter [TERM...] 
              Filter 'ls' listings. Filter with no arguments clears any 
              existing filters.

          Options:
            -@
                Hide context names in list output. Use twice to show context
                names (default).
            -+
                Hide project names in list output. Use twice to show project
                names (default).
            -c
                Color mode
            -d CONFIG_FILE
                Use a configuration file other than the default ~/.todo/config
            -f
                Forces actions without confirmation or interactive input
            -h
                Display a short help message
            -p
                Plain mode turns off colors
            -P
                Hide priority labels in list output. Use twice to show
                priority labels (default).
            -a
                Don't auto-archive tasks automatically on completion
            -A
                Auto-archive tasks automatically on completion
            -n
                Don't preserve line numbers; automatically remove blank lines
                on task deletion
            -N
                Preserve line numbers
            -t
                Prepend the current date to a task automatically
                when it's added.
            -T
                Do not prepend the current date to a task automatically
                when it's added.
            -v
                Verbose mode turns on confirmation messages
            -vv
                Extra verbose mode prints some debugging information
            -V
                Displays version, license and credits
            -x
                Disables TODOTXT_FINAL_FILTER


          Environment variables:
            TODOTXT_AUTO_ARCHIVE            is same as option -a (0)/-A (1)
            TODOTXT_CFG_FILE=CONFIG_FILE    is same as option -d CONFIG_FILE
            TODOTXT_FORCE=1                 is same as option -f
            TODOTXT_PRESERVE_LINE_NUMBERS   is same as option -n (0)/-N (1)
            TODOTXT_PLAIN                   is same as option -p (1)/-c (0)
            TODOTXT_DATE_ON_ADD             is same as option -t (1)/-T (0)
            TODOTXT_VERBOSE=1               is same as option -v
            TODOTXT_DEFAULT_ACTION=""       run this when called with no arguments
            TODOTXT_SORT_COMMAND="sort ..." customize list output
            TODOTXT_FINAL_FILTER="sed ..."  customize list after color, P@+ hiding
        */

        #endregion

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: ClientConsole [-hfasg]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        /*
          Options:
            f|ConfigFile
                Specify the path of the config  file.
        */
        static void Main(string[] args)
        {
            string configFilePath = null;
            
            //ParserResult<Options> result = 
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    configFilePath = o.ConfigFile;
                    if (string.IsNullOrEmpty(configFilePath))
                    {
                        var baseDir = Assembly.GetExecutingAssembly().Location;
                        configFilePath = baseDir + "ClientConsoleConfig.yaml";
                    }
                })
                .WithNotParsed(e =>
                {
                    foreach (var error in e)
                    {
                        if (error.Tag == ErrorType.HelpRequestedError ||
                            error.Tag == ErrorType.HelpVerbRequestedError ||
                            error.Tag == ErrorType.VersionRequestedError)
                            continue;

                        Console.Error.WriteLine(error.ToString());
                    }
                    Environment.Exit(1);
                });
            

            var configService = new ConfigService(configFilePath);

            if ( string.IsNullOrEmpty( configService.ToDoConfig.FilePath ) )
            {
                Console.WriteLine("Cannot continue, file path  not configured.");
                return;
            }

            if ( string.IsNullOrEmpty( configService.ToDoConfig.ArchiveFilePath ) )
            {
                // Don't use an archive file, just archive within the main file 
                Console.WriteLine($"Archive file not specified, will archive into {configService.ToDoConfig.FilePath}");
                configService.SetConfig(
                    filePath: configService.ToDoConfig.FilePath,
                    archiveFilePath: configService.ToDoConfig.FilePath,
                    sortType: configService.ToDoConfig.SortType,
                    groupByType: configService.ToDoConfig.GroupByType,
                    filterText: configService.ToDoConfig.FilterText,
                    listOnStart: configService.ToDoConfig.ListOnStart,
                    listAfterCommand: configService.ToDoConfig.ListAfterCommand);
            }

            // TODO Create file if it doesn't exist.
            if (!File.Exists(configService.ToDoConfig.FilePath))
            {
                throw new ArgumentException($"Todo file path does not exist: {configService.ToDoConfig.FilePath}");
            }
            
            if (!File.Exists(configService.ToDoConfig.ArchiveFilePath))
            {
                throw new ArgumentException($"Archive file path does not exist: {configService.ToDoConfig.ArchiveFilePath}");
            }
            var taskList = LoadTasks(configService.ToDoConfig.FilePath);

            var recurFilePath = configService.GetValue("recur_file_path");
            if (!File.Exists(recurFilePath))
            {
                throw new ArgumentException($"Recur file path was specified but does not exist: {recurFilePath}");
            }
            var recurTaskList = LoadRecurringTasks(recurFilePath);

            var commands = LoadCommands();

            IToDoController controller = new ToDoController(configService, taskList, commands, new TaskListView());

            Console.WriteLine($"Logfile: {Log.LogFile}");
            
            controller.Run();
        }

        private static TaskList LoadTasks(string filePath)
        {
            TaskList taskList = null;

            try
            {
                taskList = new TaskList(filePath);
            }
            catch (Exception ex)
            {
                var msg = "An error occurred while opening " + filePath;
                Log.Error(msg, ex);
            }

            return taskList;
        }

        private static RecurTaskList LoadRecurringTasks(string filePath)
        {
            RecurTaskList taskList = null;

            try
            {
                taskList = new RecurTaskList(filePath);
            }
            catch (Exception ex)
            {
                var msg = "An error occurred while opening " + filePath;
                Log.Error(msg, ex);
            }

            return taskList;
        }

        private static IDictionary<string, ITodoCommand> LoadCommands()
        {
            var commands = new Dictionary<string, ITodoCommand>();

            // Load all default commands.
            var assy = Assembly.GetExecutingAssembly();

            var assyTypes = assy.GetTypes().Where(t => t.GetInterface("ITodoCommand") != null);

            foreach (var t in assyTypes)
            {
                var cmd = (ITodoCommand)Activator.CreateInstance(t);
                foreach (var key in cmd.GetKeys())
                {
                    commands.Add(key, cmd);
                    Log.Debug("Adding command {0}", key);
                }
            }

            return commands;
        }

    }
}
