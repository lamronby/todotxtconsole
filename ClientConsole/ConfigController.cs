using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClientConsole.Commands;
using ClientConsole.Views;

namespace ClientConsole
{
    public class ConfigController
    {
        private readonly ConfigService _configService;
        private readonly IDictionary<string, ITodoCommand> _commands = new Dictionary<string, ITodoCommand>();

        public ConfigController(
            ConfigService configService,
            IDictionary<string, ITodoCommand> commands)
        {
            _configService = configService;
            _commands = commands;
            
            // Look for required values that either don't exist in config or aren't set
            
            if ( string.IsNullOrEmpty( configService.ToDoConfig.FilePath ) )
            {
                // TODO Prompt
                return;
            }

            if ( string.IsNullOrEmpty( configService.ToDoConfig.ArchiveFilePath ) )
            {
                // TODO Prompt
                return;
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
        }

        public void Run()
        {
            Console.WriteLine("Looks like you don't have a path configured for your todo.txt file");
            Console.Write("Enter a path for your todo.txt file, or Enter to use the default []");

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
                // else
                // {
                //     var matches = _inputPattern.Match(line);
                //     var cmd = matches.Groups["command"].Value.Trim();
                //     var raw = matches.Groups["raw"].Value.Trim();
                //     ExecuteCommand(cmd, raw);
                // }
                
           //      if (_configService.ToDoConfig.ListAfterCommand)
			        // TaskListView.Display(_context.TaskList, _context, "");
			}
        }
        
    }
}