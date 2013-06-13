using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ClientConsole.Commands;
using CommonExtensions;
using Mono.Options;
using ToDoLib;

namespace ClientConsole
{
	public class ClientConsole
    {

        #region Private

        private const string InputPattern = @"^(?<command>\w+)(\s+(?<raw>.*))?$";

		private TaskList _taskList;
		private IDictionary<string, ITodoCommand> _commands = new Dictionary<string, ITodoCommand>();
		private readonly Regex _inputPattern = new Regex(InputPattern);

        #endregion

        #region Properties

	    public CommandContext Context { get; set; }

        #endregion

        private void LoadTasks(string filePath)
		{
			try
			{
				_taskList = new TaskList(filePath);
			}
			catch (Exception ex)
			{
				var msg = "An error occurred while opening " + filePath;
				Log.Error(msg, ex);
			}
		}

		private IEnumerable<Task> Sort(IEnumerable<Task> tasks)
		{
			Log.Debug("Sorting {0} tasks by {1}", tasks.Count().ToString(), ConsoleConfig.Instance.SortType.ToString());

            switch (ConsoleConfig.Instance.SortType)
			{
				// nb, we sub-sort by completed for most sorts by prepending either a or z
				case SortType.Completed:
					return tasks.OrderBy(t => t.Completed);
				case SortType.Context:
					return tasks.OrderBy(t =>
					{
						var s = t.Completed ? "z" : "a";
						if (t.Contexts != null && t.Contexts.Count > 0)
							s += t.Contexts.Min().Substring(1);
						else
							s += "zzz";
						return s;
					});
				case SortType.Alphabetical:
					return tasks.OrderBy(t => (t.Completed ? "z" : "a") + t.Body);
				case SortType.DueDate:
					return tasks.OrderBy(t => (t.Completed ? "z" : "a") + (string.IsNullOrEmpty(t.DueDate) ? "zzz" : t.DueDate));
				case SortType.Priority:
					return tasks.OrderBy(t => (t.Completed ? "z" : "a") + (string.IsNullOrEmpty(t.Priority) ? "zzz" : t.Priority));
				case SortType.Project:
					return tasks.OrderBy(t =>
					{
						var s = t.Completed ? "z" : "a";
						if (t.Projects != null && t.Projects.Count > 0)
							s += t.Projects.Min().Substring(1);
						else
							s += "zzz";
						return s;
					});
				case SortType.None:
				default:
					return tasks;
			}
		}

		private void PrintTasks()
		{
			if (this.Context.GroupByType == GroupByType.Project)
			{
				Console.WriteLine("===== Projects =====");
				var projects = _taskList.Projects;

				projects.Sort();

				foreach (var project in projects)
				{
					Console.WriteLine("\n--- {0} ---", project);

					var tasks = _taskList.Tasks.Where(t => t.Projects.Contains(project)).ToList();
					var sortedTasks = tasks.OrderBy(t => (t.Completed ? "z" : "a") + (string.IsNullOrEmpty(t.Priority) ? "zzz" : t.Priority));
					foreach (var task in sortedTasks)
					{
						PrintTask(task);
					}
				}

				var tasksNoProject = _taskList.Tasks.Where(t => t.Projects.Count == 0).ToList();
				if (tasksNoProject.Count > 0)
				{
					Console.WriteLine("\n--- none ---");

					var sortedTasks = tasksNoProject.OrderBy(t => (t.Completed ? "z" : "a") + (string.IsNullOrEmpty(t.Priority) ? "zzz" : t.Priority));
					foreach (var task in sortedTasks)
					{
						PrintTask(task);
					}
				}
			}
            else if (this.Context.GroupByType == GroupByType.Context)
			{
                Console.WriteLine("===== Contexts =====");
                var contexts = _taskList.Contexts;

                contexts.Sort();

                foreach (var context in contexts)
                {
                    Console.WriteLine("\n--- {0} ---", context);

                    var tasks = _taskList.Tasks.Where(t => t.Contexts.Contains(context)).ToList();
                    var sortedTasks = tasks.OrderBy(t => (t.Completed ? "z" : "a") + (string.IsNullOrEmpty(t.Priority) ? "zzz" : t.Priority));
                    foreach (var task in sortedTasks)
                    {
                        PrintTask(task);
                    }
                }

                var tasksNoContext = _taskList.Tasks.Where(t => t.Contexts.Count == 0).ToList();
                if (tasksNoContext.Count > 0)
                {
                    Console.WriteLine("\n--- none ---");

                    var sortedTasks = tasksNoContext.OrderBy(t => (t.Completed ? "z" : "a") + (string.IsNullOrEmpty(t.Priority) ? "zzz" : t.Priority));
                    foreach (var task in sortedTasks)
                    {
                        PrintTask(task);
                    }
                }
            }
			else
			{
				foreach (var task in Sort(_taskList.Tasks))
				{
					PrintTask(task);
				}
			}
			Console.ResetColor();
		}

		private void PrintTask(Task task)
		{
			switch (task.Priority)
			{
				case "(A)":
					Console.ForegroundColor = ConsoleColor.Yellow;
					break;
				case "(B)":
					Console.ForegroundColor = ConsoleColor.Green;
					break;
				case "(C)":
					Console.ForegroundColor = ConsoleColor.Cyan;
					break;
				case "(D)":
					Console.ForegroundColor = ConsoleColor.White;
					break;
			}

			string taskStr;
            switch (this.Context.GroupByType)
			{
				case GroupByType.Context:
					taskStr = string.Format("{0,-4}{1}{2}{3} {4}",
						task.Id,
						task.Completed ? "x " + task.CompletedDate + " " : "",
						task.Priority == null ? "" : task.Priority + " ",
						task.Body, string.Join(" ", task.Projects));
					break;
				case GroupByType.Priority:
					taskStr = string.Format("{0,-4}{1}{2} {3} {4}",
						task.Id,
						task.Completed ? "x " + task.CompletedDate + " " : "",
						task.Body, string.Join(" ", task.Projects), string.Join(" ", task.Contexts));
					break;
				case GroupByType.Project:
					taskStr = string.Format("{0,-4}{1}{2} {3} {4}",
						task.Id,
						task.Completed ? "x " + task.CompletedDate + " " : "",
						task.Priority == null ? "" : task.Priority + " ",
						task.Body, string.Join(" ", task.Contexts));
					break;
				case GroupByType.None:
				default:
					taskStr = task.ToString();		// Print as-is.
					break;
			}
			Console.WriteLine(taskStr);
			Console.ResetColor();
		}

		private void RunConsole()
		{
			PrintTasks();

			while (true)
			{
				Console.Write("todo=> ");
				var line = Console.ReadLine();

				if (line.Trim().ToLower() == "q")
					break;

				var matches = _inputPattern.Match(line);
				var cmd = matches.Groups["command"].Value.Trim();
				var raw = matches.Groups["raw"].Value.Trim();

				if (String.IsNullOrEmpty(cmd))
					Console.WriteLine("Show help");
				else
				{
					if (ParseCommand(cmd, raw))
						PrintTasks();
				}
			}
		}


		private bool ParseCommand(string command, string raw)
		{
			bool success = false;

			if (_commands.ContainsKey(command.ToLower()))
			{
				var cmd = _commands[command.ToLower()];
				cmd.Execute(raw, this.Context);
				success = true;
			}
			else
			{
				Console.WriteLine("Command not recognized: {0}", command);
				success = false;
			}

			return success;
		}

		private void LoadCommands()
		{
			// Load all default commands.
			var assy = Assembly.GetExecutingAssembly();

			var assyTypes = assy.GetTypes().Where(t => t.GetInterface("ITodoCommand") != null);

			foreach (var t in assyTypes)
			{
				var cmd = (ITodoCommand)Activator.CreateInstance(t, new object[] { _taskList });
				foreach (var key in cmd.GetKeys())
				{
					_commands.Add(key, cmd);
					Log.Debug("Adding command {0}", key);
				}
			}

			// TODO Load all plugin commands.
		}

        #region todo reference

        /*
          Actions:
            SUPPORTED
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
            f|TodoFile
                Specify the todo file.
            a|ArchiveFile
                Specify the archive file.
            s|SortBy
                Specify a sort. Default is ?, valid values are none, alphabetical,
			    completed, context, duedate, priority, project.
            g|GroupBy
                Specify a grouping. Default is none, valid values are none, context,
                project, or priority.
        */
        static void Main(string[] args)
		{
			var cc = new ClientConsole();

			var showHelp = false;
			var p = new OptionSet() {
				{ "f|TodoFile=", "The todo file", f => ConsoleConfig.Instance.FilePath = f},
				{ "a|ArchiveFile=", "The archive file", a => ConsoleConfig.Instance.ArchiveFilePath = a},
				{ "s|SortBy=", "Specify a sort.", s => ConsoleConfig.Instance.SortType = 
                    DotNetExtensions.ParseEnum<SortType>(s, SortType.None)},
				{ "g|GroupBy=", "Specify a grouping.", g => ConsoleConfig.Instance.GroupByType = 
                    DotNetExtensions.ParseEnum<GroupByType>(g, GroupByType.Project)},
				{ "h|help",  "Display help", v => showHelp = v != null },
				};

			List<string> extra;
			try
			{
				extra = p.Parse(args);
			}
			catch (OptionException e)
			{
				Console.Write("ToDoConsole: ");
				Console.WriteLine(e.Message);
				Console.WriteLine("Try `ToDoConsole help' for help.");
				return;
			}

			if (showHelp)
			{
				ShowHelp(p);
				return;
			}

			foreach (var e in extra)
			{
				Console.WriteLine(e);
			}

            cc.Context = new CommandContext()
                {
                    FilePath = ConsoleConfig.Instance.FilePath,
                    ArchiveFilePath = ConsoleConfig.Instance.ArchiveFilePath,
                    DebugLevel = 1,
                    GroupByType = ConsoleConfig.Instance.GroupByType,
                    SortType = ConsoleConfig.Instance.SortType
                };

            if (!string.IsNullOrEmpty(ConsoleConfig.Instance.FilePath))
                cc.LoadTasks(ConsoleConfig.Instance.FilePath);

            cc.LoadCommands();
			cc.RunConsole();
		}
	}
}
