using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace ToDoLib
{
    public class RecurTaskList : TaskList
    {

        private DateTime _previousCheckDate = DateTime.Now;

        private DateTime _previousGenerateDate = DateTime.Now; // BUG. what's a reasonable value to have here, since we don't know the last time todos were generated?

        public List<RecurringTask> RecurringTasks { get; protected set; }

        public RecurTaskList(string filePath) : base(filePath)
        {

        } // TODO I don't think this will work


        public IList<Task> GetGeneratedRecurringTasks(TaskList activeTaskList)
        {
            IList<Task> result = new List<Task>();
            var now = DateTime.Now;

            // if a day (or more) has passed since generating recurring tasks, 
            // get new recurring tasks
            if (now.Date > _previousCheckDate.Date &&
                now.Date > _previousGenerateDate.Date )
            {
                Log.Debug($"GetGeneratedRecurringTasks: prev check date: {_previousCheckDate.Date:yyyy-MM-dd}, prev generate date: {_previousGenerateDate.Date:yyyy-MM-dd}. Will check for recurring tasks to add.");

                var daysSinceGenerated = (now.Date - _previousGenerateDate.Date).Days;
                result = GetNewRecurringTasks(activeTaskList, now);
                _previousGenerateDate = now;
            }

            _previousCheckDate = now;
            return result;
        }


        public override void ReloadTasks()
        {
            Log.Debug("Loading recurring tasks from {0}", FilePath);

         	try
            {
				var lines = File.ReadAllLines(FilePath);
                // first load
                this.Tasks = new List<Task>();
                this.RecurringTasks = new List<RecurringTask>();
                foreach (var t in lines.Where(l => l[0] != '#'))
                {
                    var id = GetNextId();
                    var task = new RecurringTask(id, t);
                    Log.Debug($"Adding recurring task: Frequency: {task.Frequency}, RecurIndex: {task.RecurIndex}, Strict? {task.Strict}, task: {task}");
                    this.Tasks.Add(task);
                    this.RecurringTasks.Add(task);
                }
                Log.Debug("Finished loading recurring tasks from {0}", FilePath);
				this.LastTaskListLoadDate = DateTime.Now;
            }
            catch (IOException ex)
            {
                var msg = "There was a problem trying to read from your recurring tasks file";
                Log.Error(msg, ex);
                throw new TaskException(msg, ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                throw;
            }
        }

        // public static for testability
        public IList<Task> GetNewRecurringTasks(TaskList activeTaskList, DateTime now)
        {
            var newRecurringTasks = new List<Task>();

            Action<RecurringTask, Func<DateTime, bool>> addTasks = (t, f) =>
            {
                for (var date = _previousGenerateDate.Date.AddDays(1); date <= now; date = date.AddDays(1))
                {
                    var rt = t as RecurringTask;
                    Log.Debug($"GetNewRecurringTasks: checking date {date:yyyy-MM-dd} should add {(rt.Strict ? "strict" : "non-strict")} {rt.Frequency} ({rt.RecurIndex}) task? {rt}"); 
                    
                    // has the task recurrence transpired since the last generate time?
                    if (f(date))
                    {
                        // this logic only adds one weekly item, even if the days since last
                        // generated spans > 1 week
                        Log.Debug($"GetNewRecurringTasks: adding {rt.Frequency} task {rt}");
                        newRecurringTasks.Add(t);
                        break;
                    }
                }
            };

            foreach (var task in this.RecurringTasks)
            {
                if (task.Frequency == RecurFrequency.Daily)
                {
                    // for strict, only add if there are no matching tasks, and only add one
                    if (task.Strict && activeTaskList.Tasks.Any(t => t.Equals(task)))
                    {
                        Log.Debug($"GetNewRecurringTasks: adding strict daily, did not find existing matching. Task: {task}");
                        newRecurringTasks.Add(task);
                    }
                    else if (!task.Strict)
                    {
                        Log.Debug($"GetNewRecurringTasks: adding daily. Task: {task}");
                        addTasks(task, (d => true));
                    }   
                }
                else if (task.Frequency == RecurFrequency.Weekly)
                {
                    addTasks(task, (d => (int)d.DayOfWeek == task.RecurIndex));
                }
                else // task.Frequency == RecurFrequency.Monthly)
                {
                    // has the monthday transpired since the last generate time?
                    var lastGenerated = _previousGenerateDate;
                    addTasks(task, (d => (int)d.Day == task.RecurIndex));
                }
            }
            return newRecurringTasks;
        }

    }
}
