using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ToDoLib;

namespace ClientConsole.Views
{
    public class TaskListView : ITaskListView
    {
        public static void Display(TaskList taskList, CommandContext context, string searchTerms)
        {
            // Create regex search string from commandArgs.
            var searchExpr = new Regex(searchTerms, RegexOptions.IgnoreCase);
            PrintTasks(taskList, searchExpr, context);
        }

        private static void PrintTasks(TaskList taskList, Regex searchExpr, CommandContext context)
        {
            var matchList = new List<Task>(taskList.Tasks);

            if (context.Filter != null)
            {
                matchList = taskList.Tasks
                                     .Where(t => context.Filter.Matches(t))
                                     .ToList();
            }

            if (searchExpr != null)
            {
                matchList = matchList
                    .Where(t => searchExpr.IsMatch(t.Body))
                    .ToList();
            }

            if (context.GroupByType == GroupByType.Project)
            {
                Console.WriteLine("=============== Projects ===============");

                var projects = matchList
                    .SelectMany(t => t.Projects)
                    .Distinct()
                    .ToList();

                projects.Sort();

                foreach (var project in projects)
                {
                    Console.WriteLine("\n--- {0} ---", project);

                    var tasks = matchList.Where(t => t.Projects.Contains(project)).ToList();
                    var sortedTasks = tasks.OrderBy(t => (t.Completed ? "z" : "a") + (string.IsNullOrEmpty(t.Priority) ? "zzz" : t.Priority));
                    foreach (var task in sortedTasks)
                    {
                        PrintTask(task, context);
                    }
                }

                var tasksNoProject = matchList.Where(t => t.Projects.Count == 0).ToList();
                if (tasksNoProject.Count > 0)
                {
                    Console.WriteLine("\n--- none ---");

                    var sortedTasks = tasksNoProject.OrderBy(t => (t.Completed ? "z" : "a") + (string.IsNullOrEmpty(t.Priority) ? "zzz" : t.Priority));
                    foreach (var task in sortedTasks)
                    {
                        PrintTask(task, context);
                    }
                }
            }
            else if (context.GroupByType == GroupByType.Context)
            {
                Console.WriteLine("===== Contexts =====");
                var contexts = matchList
                    .SelectMany(t => t.Contexts)
                    .Distinct()
                    .ToList();

                contexts.Sort();

                foreach (var c in contexts)
                {
                    Console.WriteLine("\n--- {0} ---", c);

                    var tasks = matchList.Where(t => t.Contexts.Contains(c)).ToList();
                    var sortedTasks = tasks.OrderBy(t => (t.Completed ? "z" : "a") + (string.IsNullOrEmpty(t.Priority) ? "zzz" : t.Priority));
                    foreach (var task in sortedTasks)
                    {
                        PrintTask(task, context);
                    }
                }

                var tasksNoContext = matchList.Where(t => t.Contexts.Count == 0).ToList();
                if (tasksNoContext.Count > 0)
                {
                    Console.WriteLine("\n--- none ---");

                    var sortedTasks = tasksNoContext.OrderBy(t => (t.Completed ? "z" : "a") + (string.IsNullOrEmpty(t.Priority) ? "zzz" : t.Priority));
                    foreach (var task in sortedTasks)
                    {
                        PrintTask(task, context);
                    }
                }
            }
            else
            {
                foreach (var task in Sort(matchList, context.SortType))
                {
                    PrintTask(task, context);
                }
            }
            Console.ResetColor();
        }

        private static void PrintTask(Task task, CommandContext context)
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
            switch (context.GroupByType)
            {
                case GroupByType.Context:
                    taskStr = string.Format("{0,-4}{1}{2}{3} {4}",
                        task.Id,
                        task.Completed ? "x " + task.CompletedDate + " " : "",
                        task.Priority == null ? "" : task.Priority + " ",
                        task.Body, string.Join(" ", task.Projects));
                    break;
                case GroupByType.Priority:
                    taskStr = string.Format("{0,-4}{1}{2} {3} {4}",
                        task.Id,
                        task.Completed ? "x " + task.CompletedDate + " " : "",
                        task.Body, string.Join(" ", task.Projects), string.Join(" ", task.Contexts));
                    break;
                case GroupByType.Project:
                    taskStr = string.Format("{0,-4}{1}{2} {3} {4}",
                        task.Id,
                        task.Completed ? "x " + task.CompletedDate + " " : "",
                        task.Priority == null ? "" : task.Priority + " ",
                        task.Body, string.Join(" ", task.Contexts));
                    break;
                case GroupByType.None:
                default:
                    taskStr = task.ToString();		// Print as-is.
                    break;
            }
            Console.WriteLine(taskStr);
            Console.ResetColor();
        }

        private static IEnumerable<Task> Sort(IEnumerable<Task> tasks, SortType sortType)
        {
            Log.Debug("Sorting {0} tasks by {1}", tasks.Count().ToString(), sortType.ToString());

            switch (sortType)
            {
                // nb, we sub-sort by completed for most sorts by prepending either a or z
                case SortType.Completed:
                    return tasks.OrderBy(t => t.Completed);
                case SortType.Context:
                    return tasks.OrderBy(t =>
                    {
                        var s = t.Completed ? "z" : "a";
                        if (t.Contexts != null && t.Contexts.Count > 0)
                            s += t.Contexts.Min().Substring(1);
                        else
                            s += "zzz";
                        return s;
                    });
                case SortType.Alphabetical:
                    return tasks.OrderBy(t => (t.Completed ? "z" : "a") + t.Body);
                case SortType.DueDate:
                    return tasks.OrderBy(t => (t.Completed ? "z" : "a") + (t.DueDate.HasValue ? t.DueDate : "zzz"));
                case SortType.Priority:
                    return tasks.OrderBy(t => (t.Completed ? "z" : "a") + (string.IsNullOrEmpty(t.Priority) ? "zzz" : t.Priority));
                case SortType.Project:
                    return tasks.OrderBy(t =>
                    {
                        var s = t.Completed ? "z" : "a";
                        if (t.Projects != null && t.Projects.Count > 0)
                            s += t.Projects.Min().Substring(1);
                        else
                            s += "zzz";
                        return s;
                    });
                case SortType.None:
                default:
                    return tasks;
            }
        }
    }
}
