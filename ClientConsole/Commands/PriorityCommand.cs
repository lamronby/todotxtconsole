using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ToDoLib;

namespace ClientConsole.Commands
{
	public class PriorityCommand : ITodoCommand
	{
		private TaskList _taskList;
		private IList<string> _keys = new List<string> {"pri", "prio"};
		private readonly Regex _inputPattern = new Regex(@"(?<id>\d+)\s+(?<priority>\(*[A-Z]\)*)");

		public PriorityCommand(TaskList taskList)
		{
			_taskList = taskList;
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
			var prio = matches.Groups["priority"].Value.Trim();

			if (prio.Length != 1)
			{
				Console.WriteLine("Priority {0} is not valid.", prio);
				return;
			}

			if (!String.IsNullOrEmpty(id))
			{
				var idNum = Int32.Parse(id);
				var task = _taskList.Tasks.FirstOrDefault(t => t.Id == idNum);
				var prioChar = Char.Parse(prio);

				// TODO Handle not found.
				if (task != null)
				{
					task.SetPriority(prioChar);
					_taskList.Save();
					Console.WriteLine("Priority for task {0} updated to {1}", id, prio);
				}
			}
		}

	}
}
