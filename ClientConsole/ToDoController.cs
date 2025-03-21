using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ClientConsole.Commands;
using ClientConsole.Views;
using ToDoLib;
using Serilog;

namespace ClientConsole
{
    public class ToDoController : IToDoController
    {
        private const string InputPattern = @"^(?<command>\w+)(\s+(?<raw>.*))?$";
        private readonly Regex _inputPattern = new Regex(InputPattern, RegexOptions.Compiled);
        private readonly IDictionary<string, ITodoCommand> _commands = new Dictionary<string, ITodoCommand>();
        private readonly CommandContext _context;
        // the list of recurring tasks is not exposed to commands, so it's a member here rather than in CommandContext
        private RecurTaskList _recurTaskList;

        private readonly ConfigService _configService;
        private ITaskListView _taskListView;

        public ToDoController(
            ConfigService configService,
            TaskList taskList,
            RecurTaskList recurTaskList,
            IDictionary<string, ITodoCommand> commands,
            ITaskListView taskListView)
        {
            _configService = configService;
            _commands = commands;
            _taskListView = taskListView;
            _recurTaskList = recurTaskList;

            _context = new CommandContext()
            {
                TaskList =  taskList,
                GroupByType = configService.ToDoConfig.GroupByType, 
                SortType = configService.ToDoConfig.SortType,
                Filter = new TaskFilter(configService.ToDoConfig.FilterText),
                ListOnStart = configService.ToDoConfig.ListOnStart,
                ListAfterCommand = configService.ToDoConfig.ListAfterCommand,
                DisplayBeforeThresholdDate = configService.ToDoConfig.DisplayBeforeThresholdDate				
            };
            Log.Debug($"Command context: {_context.ToString()}");
        }

        public void Run()
        {
            if ( _context.ListOnStart )
                TaskListView.Display( _context.TaskList, _context, "" );

			while (true)
			{
				Console.Write("todo=> ");
				var line = Console.ReadLine();

				if (line?.Trim().ToLower() == "q")
					break;

                if (string.IsNullOrEmpty(line?.Trim()))
                {
                    foreach (var cmd in _commands.OrderBy(c => c.Key))
                    {
                        Console.WriteLine( $"{cmd.Key,-8}: {cmd.Value.Description}" );
                    }
                }
                else
                {
                    var matches = _inputPattern.Match(line);
                    var cmd = matches.Groups["command"].Value.Trim();
                    var raw = matches.Groups["raw"].Value.Trim();
                    ExecuteCommand(cmd, raw);
                }
                
                if (_context.ListAfterCommand)
			        TaskListView.Display(_context.TaskList, _context, "");
			}
        }

        private bool ExecuteCommand(string command, string raw)
        {
            bool success = false;

            if (_commands.ContainsKey(command.ToLower()))
            {
                var cmd = _commands[command.ToLower()];
                cmd.Execute(raw, _context);
                ProcessArchiveTasks();
                CheckRecurringTasks();
                success = true;
            }
            else
            {
                Console.WriteLine("Command not recognized: {0}", command);
                success = false;
            }

            return success;
        }

        private void ProcessArchiveTasks()
        {
            if (_context.TasksToArchive.Count == 0) return;

            try
            {
                using (var writer = File.AppendText(_configService.ToDoConfig.ArchiveFilePath))
                {
                    foreach (var task in _context.TasksToArchive)
                    {
                        var archiveTask = task.ToString();
                        Log.Debug("Archiving task '{0}'", archiveTask);
                        writer.WriteLine(archiveTask);
                    }
                }
                _context.TasksToArchive.Clear();
            }
            catch (IOException ex)
            {
                var msg = "An error occurred while trying to archive a task to the archive file";
                Log.Error(msg, ex);
                throw new TaskException(msg, ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        private void CheckRecurringTasks()
        {
            if (_recurTaskList == null) return;

            foreach (var task in _recurTaskList.GetGeneratedRecurringTasks(_context.TaskList))
            {
                _context.TaskList.Add(task);
            }
        }
    }
}