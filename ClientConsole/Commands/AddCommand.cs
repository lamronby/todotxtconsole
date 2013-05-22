using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToDoLib;

namespace ClientConsole.Commands
{
	public class AddCommand : ITodoCommand
	{
		private TaskList _taskList;
		private IList<string> _keys = new List<string> {"add", "a"};

		public AddCommand(TaskList taskList)
		{
			_taskList = taskList;
		}

		public IList<string> GetKeys()
		{
			return _keys;
		}

		public void Execute(string raw)
		{
			_taskList.Add(new Task(raw));
		}
	}
}
