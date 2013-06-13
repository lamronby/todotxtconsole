using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ToDoLib;

namespace ClientConsole.Commands
{
	// TODO Add filter support.
	// TODO Add sort support.
	public class ListCommand : ITodoCommand
	{
		private TaskList _taskList;
		private IList<string> _keys = new List<string> {"list", "ls"};

		public ListCommand(TaskList taskList)
		{
			_taskList = taskList;
		}

		public IList<string> GetKeys()
		{
			return _keys;
		}

		public void Execute(string commandArgs, CommandContext context)
		{
			// Create regex search string from commandArgs.
			Regex searchExpr = new Regex(commandArgs, RegexOptions.IgnoreCase);
			foreach (var task in _taskList.Tasks)
			{
				if (searchExpr.IsMatch(task.Body))
					PrintTask(task);
			}
			Console.ResetColor();
		}

		private void PrintTask(Task task)
		{
			switch (task.Priority)
			{
				case "(A)":
					Console.ForegroundColor = ConsoleColor.Yellow;
					break;
				case "(B)":
					Console.ForegroundColor = ConsoleColor.Green;
					break;
				case "(C)":
					Console.ForegroundColor = ConsoleColor.Cyan;
					break;
				case "(D)":
					Console.ForegroundColor = ConsoleColor.White;
					break;
			}

			string taskStr;
			taskStr = task.ToString();		// Print as-is.
			Console.WriteLine(taskStr);
		}

	}
}
