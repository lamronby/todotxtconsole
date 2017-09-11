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
		private IList<string> _keys = new List<string> {"do"};
		private readonly Regex _inputPattern = new Regex(@"(\s*(?<id>\d+),?)+");

		public DoCommand()
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
			var ids = matches.Groups["id"].Captures;

			for (int i = 0; i < ids.Count; i++)
			{
				var id = Int32.Parse(ids[i].Value);
                var task = context.TaskList.Tasks.FirstOrDefault( t => t.Id == id );
				// TODO Handle not found.
				if (task != null)
				{
					// TODO Add clone method to Task?
					// TODO Add date completed and 'x' to indicate complete.

					var archTask = task.Clone();
					archTask.Completed = true;
					archTask.CompletedDate = DateTime.Now;

                    context.ArchiveList.Add( archTask );
                    context.TaskList.Delete( task );
					Console.WriteLine("TODO: {0} marked as done.", archTask.Body);
				}
			}
		}
	}
}
