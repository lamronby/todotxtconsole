using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ToDoLib;

namespace ClientConsole.Commands
{
	public class DoCommand : ITodoCommand
	{
		private TaskList _taskList;
		private IList<string> _keys = new List<string> {"do"};
		private readonly Regex _inputPattern = new Regex(@"(\s*(?<id>\d+),?)+");

		public DoCommand(TaskList taskList)
		{
			_taskList = taskList;
		}

		public IList<string> GetKeys()
		{
			return _keys;
		}

		public void Execute(string raw)
		{
			var filePath = ConfigurationManager.AppSettings["ArchiveFilePath"];
			TaskList archiveList = null;

			// Extract IDs from the raw string and find it in the task list.
			var matches = _inputPattern.Match(raw);
			var ids = matches.Groups["id"].Captures;
			if (ids.Count > 0)
			{
				archiveList = new TaskList(filePath);
			}

			for (int i = 0; i < ids.Count; i++)
			{
				var id = Int32.Parse(ids[i].Value);
				var task = _taskList.Tasks.FirstOrDefault(t => t.Id == id);
				// TODO Handle not found.
				if (task != null)
				{
					// TODO Add clone method to Task?
					// TODO Add date completed and 'x' to indicate complete.

					var archTask = task.Clone();
					archTask.Completed = true;
					archTask.CompletedDate = DateTime.Now;

					archiveList.Add(archTask);
					_taskList.Delete(task);
					Console.WriteLine("TODO: {0} marked as done.", archTask.Body);
				}
			}
		}
	}
}
