using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoLib
{
    public class RecurTaskList : TaskList
    {

        private DateTime _previousCheckDate = DateTime.Now;

        private DateTime _previousGenerateDate = DateTime.MinValue;

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
                var daysSinceGenerated = (now.Date - _previousGenerateDate.Date).Days;
                result = GetNewRecurringTasks(activeTaskList, daysSinceGenerated);
                _previousGenerateDate = now;
            }

            _previousCheckDate = now;
            return result;
        }

        // private IList<Task> GetNewRecurringTasks(TaskList activeTaskList, int daysSinceGenerated)
        // {
        //     var newRecurringTasks = new List<Task>();

        //     Action<Task, Func<DateTime, bool>> addTasks = (t, f) =>
        //     {
        //             var lastGenerated = _previousGenerateDate;
        //             for (int i = 0; i < daysSinceGenerated; i++)
        //             {
        //                 lastGenerated.AddDays(1);
        //                 // has the task recurrence transpired since the last generate time?
        //                 if (f(lastGenerated))
        //                 {
        //                     // this logic only adds one weekly item, even if the days since last
        //                     // generated spans > 1 week
        //                     newRecurringTasks.Add(t);
        //                     break;
        //                 }
        //             }
        //     };

        //     foreach (var task in this.RecurringTasks)
        //     {
        //         if (task.Frequency == RecurFrequency.Daily)
        //         {
        //             // for strict, only add if there are no matching tasks, and only add one
        //             if (task.Strict && activeTaskList.Tasks.Any(t => t.CompareTo(task) == 0))
        //             {
        //                 newRecurringTasks.Add(task);
        //             }
        //             else if (!task.Strict)
        //             {
        //                 addTasks(task, (d => true));
        //                 // for (int i = 0; i < daysSinceGenerated; i++)
        //                 // {
        //                 //     newRecurringTasks.Add(task);
        //                 // }
        //             }
        //         }
        //         else if (task.Frequency == RecurFrequency.Weekly)
        //         {
        //             addTasks(task, (d => (int)d.DayOfWeek == task.RecurIndex));

        //             // var lastGenerated = _previousGenerateDate;
        //             // for (int i = 0; i < daysSinceGenerated; i++)
        //             // {
        //             //     lastGenerated.AddDays(1);
        //             //     // has the weekday transpired since the last generate time?
        //             //     if ((int)lastGenerated.DayOfWeek == task.RecurIndex)
        //             //     {
        //             //         // this logic only adds one weekly item, even if the days since last
        //             //         // generated spans > 1 week
        //             //         newRecurringTasks.Add(task);
        //             //         break;
        //             //     }
        //             // }
        //         }
        //         else // task.Frequency == RecurFrequency.Monthly)
        //         {
        //             // has the monthday transpired since the last generate time?
        //             var lastGenerated = _previousGenerateDate;
        //             addTasks(task, (d => (int)d.Day == task.RecurIndex));
        //             // for (int i = 0; i < daysSinceGenerated; i++)
        //             // {
        //             //     lastGenerated.AddDays(1);
        //             //     // has the month day transpired since the last generate time?
        //             //     if (lastGenerated.Day == task.RecurIndex)
        //             //     {
        //             //         // this logic only adds one monthly item, even if the days since last
        //             //         // generated spans > 1 month
        //             //         newRecurringTasks.Add(task);
        //             //         break;
        //             //     }
        //             // }
        //         }
        //     }
        //     return newRecurringTasks;
        // }


        public override void ReloadTasks()
        {
            Log.Debug("Loading recurring tasks from {0}", FilePath);

         	try
            {
				var lines = File.ReadAllLines(FilePath);
                // first load
                this.Tasks = new List<Task>();
                this.RecurringTasks = new List<RecurringTask>();
                for (int i = 0; i < lines.Length; i++)
                {
                    var id = GetNextId();
                    var task = new RecurringTask(id, lines[i]);
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
        public IList<Task> GetNewRecurringTasks(TaskList activeTaskList, int daysSinceGenerated)
        {
            var newRecurringTasks = new List<Task>();

            Action<Task, Func<DateTime, bool>> addTasks = (t, f) =>
            {
                    var lastGenerated = _previousGenerateDate;
                    for (int i = 0; i < daysSinceGenerated; i++)
                    {
                        lastGenerated.AddDays(1);
                        // has the task recurrence transpired since the last generate time?
                        if (f(lastGenerated))
                        {
                            // this logic only adds one weekly item, even if the days since last
                            // generated spans > 1 week
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
                        newRecurringTasks.Add(task);
                    }
                    else if (!task.Strict)
                    {
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
