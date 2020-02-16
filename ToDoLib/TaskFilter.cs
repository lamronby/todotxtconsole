using System;
using System.Collections.Generic;
using System.Linq;

namespace ToDoLib
{
    public class TaskFilter : Task
    {
        public TaskFilter(string raw) : base(raw)
        {
        }

        public TaskFilter(int id, string raw) : base(id, raw)
        {
        }

        public TaskFilter(string priority, List<string> projects, List<string> contexts, string body, string dueDate = "", bool completed = false) : base(priority, projects, contexts, body, dueDate, completed)
        {
        }

        public bool Matches(Task task)
        {
            // TODO Functionality to implement here.
			if (this.Completed != task.Completed)
				return false;

            if (this.CompletedDate.HasValue && !task.CompletedDate.HasValue)
				return false;
			if (!this.CompletedDate.HasValue && task.CompletedDate.HasValue)
                return false;
			if (this.CompletedDate.HasValue && task.CompletedDate.HasValue)
			{
				int dateCompare = this.CompletedDate.Value.CompareTo(task.CompletedDate.Value);
				if (dateCompare != 0)
					return false;
			}

            if (!String.IsNullOrEmpty(this.Priority))
            {
                int priCompare = this.Priority.CompareTo(task.Priority);
                if (priCompare != 0)
                    return false;
            }

            if (!String.IsNullOrEmpty(this.Body))
            {
                int bodyCompare = this.Body.CompareTo(task.Body);
                if (bodyCompare != 0)
                    return false;
            }

            if (Projects.Count > 0)
            {
                // See if there are any matches
                if (!task.Projects.Intersect(this.Projects).Any())
                    return false;
            }

            if (Contexts.Count > 0)
            {
                if (!task.Contexts.Intersect(this.Contexts).Any())
                    return false;
            }

            return true;
		}
            
        public override string ToString()
        {
            // Format:
            // <body> project(s): <projects>, context(s): <contexts>
            var str = "";
            str = $"{Body} project(s): {string.Join(" ", Projects)} context(s): {string.Join(" ", Contexts)}";

            return str;
        }

    }
}