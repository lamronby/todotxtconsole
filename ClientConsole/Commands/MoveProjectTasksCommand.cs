﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ToDoLib;

namespace ClientConsole.Commands
{
	/// <summary>
	/// mvproj ITEM# from to 
	/// mvproj all from to
	/// </summary>
	public class MoveProjectTasksCommand : ITodoCommand
	{
		private TaskList _taskList;
		private IList<string> _keys = new List<string> {"mvproj"};
		private readonly Regex _inputPattern = new Regex(@"^(?<id>(all|\d+)\s+)?(?<proj1>\w+)\s+(?<proj2>\w+)");

		public MoveProjectTasksCommand(TaskList taskList)
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
			var proj1 = matches.Groups["proj1"].Value.Trim();
			var proj2 = matches.Groups["proj2"].Value.Trim();
			string id = null;
			if (matches.Groups["id"].Length > 0)
			{
				id = matches.Groups["id"].Captures[0].Value.Trim();
			}

			if (id == null)
			{
				// Get all tasks that have proj1 assigned.
				var tasks = _taskList.Tasks.Where(t => t.Projects.Contains(proj1)).ToList();

				foreach (var task in tasks)
				{
					MoveTaskProject(task, proj1, proj2);
				}
			}
			else
			{
				var intId = Int32.Parse(id);
				var task = _taskList.Tasks.FirstOrDefault(t => t.Id == intId);
				// TODO Handle not found.
				MoveTaskProject(task, proj1, proj2);
			}
		}

		private void MoveTaskProject(Task task, string proj1, string proj2)
		{
			if (task == null) return;

			var newTask = task.Clone();

			newTask.Projects.Remove(proj1);
			newTask.Projects.Add(proj2);

			_taskList.Save();
			Console.WriteLine("Moved task {0} from {1} to {2}", task.Id, proj1, proj2);
		}
	}

}
