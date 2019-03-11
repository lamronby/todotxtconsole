using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ToDoLib;

namespace ClientConsole.Commands
{
	/// <summary>
	/// del ITEM# [TERM]
	/// rm ITEM# [TERM]
	/// Deletes the task on line ITEM# in todo.txt.
	/// If TERM specified, deletes only TERM from the task.
	/// </summary>
	// TODO: Implement support for [TERM].
	public class DeleteCommand : ITodoCommand
	{
		private IList<string> _keys = new List<string> { "del", "rm" };
		private readonly Regex _inputPattern = new Regex(@"(?<id>\d+)");

        public string Description => "Delete a todo list item";

        public DeleteCommand()
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
			if (!String.IsNullOrEmpty(id))
			{
				var idNum = Int32.Parse(id);
				var task = context.TaskList.Tasks.FirstOrDefault(t => t.Id == idNum);
				// TODO Handle not found.
				if (task != null)
				{
					Console.Write("Delete {0}? (y/n) ", task.Body);
					var line = Console.ReadLine();
					if (!String.IsNullOrWhiteSpace(line) && line.Trim() == "y")
					{
                        context.TaskList.Delete( task );
					}
				}
			}
		}
	}
}
