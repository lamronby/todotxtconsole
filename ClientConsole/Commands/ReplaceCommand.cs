using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ToDoLib;

namespace ClientConsole.Commands
{
	public class ReplaceCommand : ITodoCommand
	{
		private IList<string> _keys = new List<string> {"repl", "replace"};
		private readonly Regex _inputPattern = new Regex(@"(?<id>\d+)\s+(?<update>.*)$");
        public string Description => "Replace the contents of a task";

        public ReplaceCommand()
		{
		}

		public IList<string> GetKeys()
		{
			return _keys;
		}

        public void Execute(string commandArgs, CommandContext context)
        {
			// Extract IDs from the commandArgs string and find it in the task list.
			var matches = _inputPattern.Match(commandArgs);
			var id = matches.Groups["id"].Value.Trim();
			var update = matches.Groups["update"].Value.Trim();

			if (!String.IsNullOrEmpty(id))
			{
				var idNum = Int32.Parse(id);
                var task = context.TaskList.Tasks.FirstOrDefault( t => t.Id == idNum );

				// TODO Handle not found.
				if (task != null)
				{
					task.Update(update);
                    context.TaskList.Save();
					Console.WriteLine("Task {0} updated to {1}", id, update);
				}
			}
		}

	}
}
