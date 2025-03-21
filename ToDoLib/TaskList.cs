using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CommonExtensions;
using Serilog;

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
        
    	private int _nextId;

        private bool _fullReloadAfterChanges;

        private FileSystemWatcher _watcher;

        public string FilePath;

        public DateTime LastModifiedDate => File.GetLastAccessTime(FilePath);

        public DateTime LastTaskListLoadDate { get; protected set; }
        
        public List<Task> Tasks { get; protected set; }

    	public List<string> Priorities
    	{
			get { return this.Tasks.OrderBy(t => t.Priority).Select(t => t.Priority).Distinct().ToList(); }
    	}

		public List<string> Contexts
		{
			get { return this.Tasks.SelectMany(t => t.Contexts).Distinct().ToList(); }
		}

 		public List<string> Projects
    	{
			get { return this.Tasks.SelectMany(t => t.Projects).Distinct().ToList(); }
		}

        
        public TaskList(string filePath, bool fullReloadAfterChanges = false)
        {
            FilePath = filePath;
            _fullReloadAfterChanges = fullReloadAfterChanges;

            _watcher = new FileSystemWatcher(Path.GetDirectoryName(filePath), Path.GetFileName(filePath));
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.Changed += OnFileChanged;

            ReloadTasks();
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }            
            Console.WriteLine($"Detected {e.FullPath} has been modified. Reloading file.");
            Log.Debug($"Detected {0} has been modified. Reloading file.", e.FullPath);
            ReloadTasks();
        }

        public virtual void ReloadTasks()
        {
            Log.Debug("Loading tasks from {0}", FilePath);
            _watcher.EnableRaisingEvents = false;
            
			/*
			 * TODO: Fix. General strategy (implemented outside of TaskList) - before executing a task (e.g. do), if file timestamp has changed, abort and reload file. Make user redo task. Reloads should be rare enough that this is not annoying.
			 * 
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
                var filteredLines = File.ReadAllLines(this.FilePath).Where(l => l[0] != '#').ToList();
                
                if (this.Tasks == null || _fullReloadAfterChanges)
                {
                    // Full load.
                    this.Tasks = new List<Task>();
                    _nextId = 0;
                    foreach (var t in filteredLines)
                    {
                        this.Tasks.Add(new Task(GetNextId(), t));
                    }
                    Log.Debug("Finished loading {0} tasks from {1}", this.Tasks.Count, this.FilePath);
                }
                else
                {
                    var fileTasks = filteredLines.Select(t => new Task(t)).ToList();

                    // A real reload.
                    if (fileTasks.Count == this.Tasks.Count)
                    {
                        // Either the list and file are in sync or there was a
                        // task update. Update all tasks but don't change the IDs.
                        for (int i = 0; i < fileTasks.Count; i++)
                        {
                            if (!fileTasks[i].Equals(this.Tasks[i]))
                            {
                                Log.Debug("Found updated task {0}: {1}", i.ToString(), filteredLines[i]);
                                this.Tasks[i] = new Task(this.Tasks[i].Id, filteredLines[i]);
                            }
                        }
                    }
                    else if (filteredLines.Count > this.Tasks.Count)
                    {
                        // Must have been an add.
                        for (int i = this.Tasks.Count; i < filteredLines.Count; i++)
                        {
                            Log.Debug("Adding new task '{0}'", filteredLines[i]);
                            this.Tasks.Add(new Task(GetNextId(), filteredLines[i]));
                        }
                    }
                    else
                    {
                        var toRemoves = fileTasks.Except(this.Tasks.Select(t => t));

                        foreach (var toRemove in toRemoves)
                        {
                            //var r = _tasks.FirstOrDefault(t => t.Raw == raw);
                            Log.Debug("Removing task {0}: {1}", toRemove.Id.ToString(), toRemove.Body);
                            this.Tasks.Remove(toRemove);
                        }
                    }
                    Log.Debug("Finished reloading {0} tasks from {1}", this.Tasks.Count, this.FilePath);
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
            _watcher.EnableRaisingEvents = true;
            
        }

        public void Add(Task task)
        {
            try
            {
                var output = task.ToString();

                Log.Debug("Adding task '{0}'", output);

                var text = File.ReadAllText(FilePath);
                if (text.Length > 0 && !text.EndsWith(Environment.NewLine))
                    output = Environment.NewLine + output;

                _watcher.EnableRaisingEvents = false;
                File.AppendAllLines(FilePath, new string[] { output });

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

                if (this.Tasks.Remove(this.Tasks.First(t => t.Equals(task))))
                {
                    _watcher.EnableRaisingEvents = false;
                    File.WriteAllLines(FilePath, this.Tasks.Select(t => t.ToString()));
                    _watcher.EnableRaisingEvents = true;
                }
                
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
                File.WriteAllLines(FilePath, this.Tasks.Select(t => t.ToString()));
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
                return SortList(sort, FilterList(this.Tasks, FilterCaseSensitive, Filter));
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
                    return tasks.OrderBy(t => (t.Completed ? "z" : "a") + (t.DueDate.HasValue ? t.DueDate : "9999-99-99"));
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

		protected int GetNextId()
		{
			return _nextId++;
		}
    }
}
