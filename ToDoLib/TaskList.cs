﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CommonExtensions;

namespace ToDoLib
{
    /// <summary>
    /// A thin data access abstraction over the actual todo.txt file
    /// </summary>
    public class TaskList
    {
        // It may look like an overly simple approach has been taken here, but it's well considered. This class
        // represents *the file itself* - when you call a method it should be as though you directly edited the file.
        // This reduces the likelihood of concurrent update conflicts by making each action as autonomous as possible.
        // Although this does lead to some extra IO, it's a small price for maintaining the integrity of the file.

        // NB, this is not the place for higher-level functions like searching, task manipulation etc. It's simply 
        // for CRUDing the todo.txt file. 
        
        private List<Task> _tasks;
        private readonly string _filePath;
    	private int _nextId = 0;

        public DateTime LastModifiedDate => File.GetLastAccessTime(_filePath);

        public DateTime LastTaskListLoadDate { get; private set; }
        
        public List<Task> Tasks { get { return _tasks; } }

    	public List<string> Priorities
    	{
			get { return _tasks.OrderBy(t => t.Priority).Select(t => t.Priority).Distinct().ToList(); }
    	}

		public List<string> Contexts
		{
			get { return _tasks.SelectMany(t => t.Contexts).Distinct().ToList(); }
		}

 		public List<string> Projects
    	{
			get { return _tasks.SelectMany(t => t.Projects).Distinct().ToList(); }
		}

        
        public TaskList(string filePath)
        {
            _filePath = filePath;
            ReloadTasks();
        }

        public void ReloadTasks()
        {
            Log.Debug("Loading tasks from {0}", _filePath);

			/*
			 * Account for changes that could have occurred in the file since 
			 * last load.
			 * 
			 * Update types:
			 *   Add - line appended to file
			 *   Update - line number (ID) is the same, data changed.
			 *   Delete - line deleted somewhere, 1-n line numbers change,
			 *            where n = number of lines in new file.
			 *            
			 * Task identifier strategy:
			 * For each run of the client console (no exit), a task will
			 * maintain its ID.
			 *   1. If TaskList._tasks is empty, create new IDs for each
			 *      task. ID = line number.
			 *   2. If TaskList._tasks is not empty:
			 *      a. If Add (line count > _tasks.Count), maintain all IDs,
			 *         create new ID for new task.
			 *      b. If Update (line count == _tasks.Count), maintain all IDs.
			 *      c. If Delete (line count < _tasks.Count), compare each 
			 *         task, keep ID for all that are equal (which 
			 *         should be all based on file load model).
			 */

			try
            {
				var lines = File.ReadAllLines(_filePath);
				if (_tasks != null)
				{
					var fileTasks = lines.Select(t => new Task(t)).ToList();

					// A real reload.
					if (fileTasks.Count == _tasks.Count)
					{
						// Either the list and file are in sync or there was a
						// task update. Update all tasks but don't change the IDs.
						for (int i = 0; i < fileTasks.Count; i++)
						{
							if (fileTasks[i].CompareTo(_tasks[i]) != 0)
							{
								Log.Debug("Found updated task {0}: {1}", i.ToString(), lines[i]);
								_tasks[i].Update(lines[i]);
							}
						}
					}
					else if (lines.Length > _tasks.Count)
					{
						// Must have been an add.
						for (int i = _tasks.Count; i < lines.Length; i++)
						{
							Log.Debug("Adding new task '{0}'", lines[i]);
							_tasks.Add(new Task(GetNextId(), lines[i]));
						}
					}
					else
					{
						var toRemoves = fileTasks.Except(_tasks.Select(t => t));
							//lines.Except(_tasks.Select(t => t.Raw));

						foreach (var toRemove in toRemoves)
						{
							//var r = _tasks.FirstOrDefault(t => t.Raw == raw);
							Log.Debug("Removing task {0}: {1}", toRemove.Id.ToString(), toRemove.Body);
							_tasks.Remove(toRemove);
						}
					}
					Log.Debug("Finished reloading tasks from {0}", _filePath);
				}
				else
            	{
					// First load.
					_tasks = new List<Task>();
					for (int i = 0; i < lines.Length; i++)
					{
						_tasks.Add(new Task(GetNextId(), lines[i]));
					}
					Log.Debug("Finished loading tasks from {0}", _filePath);
				}
				this.LastTaskListLoadDate = DateTime.Now;
            }
            catch (IOException ex)
            {
                var msg = "There was a problem trying to read from your todo.txt file";
                Log.Error(msg, ex);
                throw new TaskException(msg, ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        public void Add(Task task)
        {
            try
            {
                var output = task.ToString();

                Log.Debug("Adding task '{0}'", output);

                var text = File.ReadAllText(_filePath);
                if (text.Length > 0 && !text.EndsWith(Environment.NewLine))
                    output = Environment.NewLine + output;

                File.AppendAllLines(_filePath, new string[] { output });

				Console.WriteLine("Task '{0}' added", output);
                Log.Debug("Task '{0}' added", output);

                ReloadTasks();
            }
            catch (IOException ex)
            {
                var msg = "An error occurred while trying to add your task to the task list file";
                Log.Error(msg, ex);
                throw new TaskException(msg, ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }

        }

        public void Delete(Task task)
        {
            try
            {
                Log.Debug("Deleting task {0}: {1}", task.Id.ToString(), task.ToString());

                ReloadTasks(); // make sure we're working on the latest file
                
                if (_tasks.Remove(_tasks.First(t => t == task)))
                    File.WriteAllLines(_filePath, _tasks.Select(t => t.ToString()));
                
                Log.Debug("Task {0} deleted", task.Id.ToString());
				Console.WriteLine("Task {0} deleted", task.Id.ToString());

                ReloadTasks();
            }
            catch (IOException ex)
            {
                var msg = "An error occurred while trying to remove your task from the task list file";
                Log.Error(msg, ex);
                throw new TaskException(msg, ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

      
        public void Save()
        {
            try
            {
                File.WriteAllLines(_filePath, _tasks.Select(t => t.ToString()));
                ReloadTasks();
            }
            catch (IOException ex)
            {
                var msg = "An error occurred while trying to update your task int the task list file";
                Log.Error(msg, ex);
                throw new TaskException(msg, ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        public IEnumerable<Task> Sort(SortType sort, bool FilterCaseSensitive, string Filter)
        {
                return SortList(sort, FilterList(_tasks, FilterCaseSensitive, Filter));
        }

        public static List<Task> FilterList(List<Task> tasks, bool FilterCaseSensitive, string Filter) 
        {
            var comparer = FilterCaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

            if (String.IsNullOrEmpty(Filter)) return tasks;

            var tasksFilter = new List<Task>();

            foreach (var task in tasks)
            {
                bool include = true;
                foreach (var filter in Filter.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (filter.Substring(0, 1) != "-")
                    {   // if the filter does not start with a minus and filter is contained in task then filter out
                        if (!task.ToString().Contains(filter, comparer))
                            include = false;
                    }
                    else
                    {   // if the filter starts with a minus then (ignoring the minus) check if the filter is contained in the task then filter out if so
                        if (task.ToString().Contains(filter.Substring(1), comparer))
                            include = false;
                    }
                }

                if (include)
                    tasksFilter.Add(task);
            }
            return tasksFilter;
        }

        public static IEnumerable<Task> SortList(SortType sort, List<Task> tasks)
        {
            Log.Debug("Sorting {0} tasks by {1}", tasks.Count().ToString(), sort.ToString());

            switch (sort)
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
                    return tasks.OrderBy(t => (t.Completed ? "z" : "a") + t.ToString());
                case SortType.DueDate:
                    return tasks.OrderBy(t => (t.Completed ? "z" : "a") + (string.IsNullOrEmpty(t.DueDate) ? "9999-99-99" : t.DueDate));
                case SortType.Priority:
                    return tasks.OrderBy(t => (t.Completed ? "z" : "a") + (string.IsNullOrEmpty(t.Priority) ? "(z)" : t.Priority));
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

		private int GetNextId()
		{
			return _nextId++;
		}
    }
}
