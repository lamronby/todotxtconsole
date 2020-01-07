using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ClientConsole.Commands;
using ClientConsole.Views;
using ToDoLib;
using CommonExtensions;

namespace ClientConsole
{
    public class ToDoController : IToDoController
    {
        private const string InputPattern = @"^(?<command>\w+)(\s+(?<raw>.*))?$";
        private readonly Regex _inputPattern = new Regex(InputPattern);

        private IDictionary<string, ITodoCommand> _commands = new Dictionary<string, ITodoCommand>();

        private CommandContext _context;
        private string _archiveFilePath;

        private readonly IConfigService _configService;
        private ITaskListView _taskListView;

        public ToDoController(
            IConfigService configService,
            TaskList taskList,
            string archiveFilePath,
            IDictionary<string, ITodoCommand> commands,
            ITaskListView taskListView)
        {
            _configService = configService;
            _commands = commands;
            _taskListView = taskListView;
            _archiveFilePath = archiveFilePath;

            _context = new CommandContext()
            {
                TaskList =  taskList,
                DebugLevel = Int32.Parse( configService.GetValue( "debug_level" ) ),
                GroupByType = DotNetExtensions.ParseEnum<GroupByType>(configService.GetValue(ConfigService.GROUP_BY_TYPE_KEY), GroupByType.None),
                SortType = DotNetExtensions.ParseEnum<SortType>(configService.GetValue(ConfigService.SORT_TYPE_KEY), SortType.Project),
                Filter = new TaskFilter(configService.GetValue(ConfigService.FILTER_TEXT_KEY)),
            };

            bool listOnStart;
            if ( Boolean.TryParse( configService.GetValue( "list_on_start" ), out listOnStart ) )
            {
                _context.ListOnStart = listOnStart;
            }

            bool listAfterCommand;
            if ( Boolean.TryParse( configService.GetValue( "list_after_command" ), out listAfterCommand ) )
            {
                _context.ListAfterCommand = listAfterCommand;
            }
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
                using (var writer = File.AppendText(_archiveFilePath))
                {
                    foreach (var task in _context.TasksToArchive)
                    {
                        var archiveTask = task.ToString();
                        Log.Debug("Archiving task '{0}'", archiveTask);
                        writer.WriteLine(archiveTask);
                    }
                }
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
    }
}