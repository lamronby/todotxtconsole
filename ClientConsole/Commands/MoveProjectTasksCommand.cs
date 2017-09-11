using System;
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
		private IList<string> _keys = new List<string> {"mvproj"};
        private readonly Regex _inputPattern = new Regex(@"^(?<id>(all|\d+)\s+)?(?<proj1>\+?\w+)\s+(?<proj2>\+?\w+)");

		public MoveProjectTasksCommand()
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
			var proj1 = matches.Groups["proj1"].Value.Trim();
			var proj2 = matches.Groups["proj2"].Value.Trim();
			string id = null;
			if (matches.Groups["id"].Length > 0)
			{
				id = matches.Groups["id"].Captures[0].Value.Trim();
			}

            // Append a '+' if not already there.
            if (proj1[0] != '+')
                proj1 = "+" + proj1;

            if (proj2[0] != '+')
                proj2 = "+" + proj2;

			if (id == null)
			{
				// Get all tasks that have proj1 assigned.
                var tasks = context.TaskList.Tasks.Where( t => t.Projects.Contains( proj1 ) ).ToList();

				foreach (var task in tasks)
				{
					MoveTaskProject(task, proj1, proj2);
                    context.TaskList.Save();
                    Console.WriteLine( "Moved task {0} from {1} to {2}", task.Id, proj1, proj2 );
                }
			}
			else
			{
				var intId = Int32.Parse(id);
                var task = context.TaskList.Tasks.FirstOrDefault( t => t.Id == intId );
				// TODO Handle not found.
				MoveTaskProject(task, proj1, proj2);
			}
		}

		private static void MoveTaskProject(Task task, string proj1, string proj2)
		{
			if (task == null) return;

			var newTask = task.Clone();

			newTask.Projects.Remove(proj1);
			newTask.Projects.Add(proj2);
		}
	}

}
