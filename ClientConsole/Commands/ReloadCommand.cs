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
            context.TaskList.Add(new Task(commandArgs));
		}
	}
}
