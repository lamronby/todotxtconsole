using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToDoLib;

namespace ClientConsole.Commands
{
	public class ReloadCommand : ITodoCommand
	{
		private IList<string> _keys = new List<string> {"reload", "rel"};

        public string Description => "Reload from the todo.txt file";

        public ReloadCommand()
		{
		}

		public IList<string> GetKeys()
		{
			return _keys;
		}

		public void Execute(string commandArgs, CommandContext context)
		{
			Console.WriteLine($"Todo list last loaded: {context.TaskList.LastTaskListLoadDate:MM/dd/yyyy HH:mm:ss}");
			Console.WriteLine($"Current todo file date: {context.TaskList.LastModifiedDate:MM/dd/yyyy HH:mm:ss}");
			if (context.TaskList.LastModifiedDate > context.TaskList.LastTaskListLoadDate)
			{
				Console.Write($"Current todo file date is more recent ({context.TaskList.LastModifiedDate:MM/dd/yyyy HH:mm:ss}), reloading...");
				context.TaskList.ReloadTasks();
				Console.WriteLine("Done.");
			}
			else
			{
				Console.WriteLine($"Todo list is up-to-date, no reload needed.");
			}
		}
	}
}
