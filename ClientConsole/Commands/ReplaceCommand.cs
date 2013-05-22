using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ToDoLib;

namespace ClientConsole.Commands
{
	public class ReplaceCommand : ITodoCommand
	{
		private TaskList _taskList;
		private IList<string> _keys = new List<string> {"repl", "replace"};
		private readonly Regex _inputPattern = new Regex(@"(?<id>\d+)\s+(?<update>.*)$");

		public ReplaceCommand(TaskList taskList)
		{
			_taskList = taskList;
		}

		public IList<string> GetKeys()
		{
			return _keys;
		}

		public void Execute(string raw)
		{
			// Extract IDs from the raw string and find it in the task list.
			var matches = _inputPattern.Match(raw);
			var id = matches.Groups["id"].Value.Trim();
			var update = matches.Groups["update"].Value.Trim();

			if (!String.IsNullOrEmpty(id))
			{
				var idNum = Int32.Parse(id);
				var task = _taskList.Tasks.FirstOrDefault(t => t.Id == idNum);

				// TODO Handle not found.
				if (task != null)
				{
					task.Update(update);
					_taskList.Save();
					Console.WriteLine("Task {0} updated to {1}", id, update);
				}
			}
		}

	}
}
