using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ToDoLib
{
    public enum RecurFrequency
    {
        Daily,
        Weekly,
        Monthly
    }

    public class RecurringTask : Task, IComparable<RecurringTask>, IEquatable<RecurringTask>
    {
        public RecurFrequency Frequency { get; set; }

        /// <summary>
        /// Daily frequency: 0
        /// Weekly frequency: day of the week, 0-6
        /// Monthly frequency: day of the month
        /// </summary>
        public int RecurIndex { get; set; }

        public bool Strict { get; set; }

        // daily: task    
        // [day]: task
        // [month_day](st|nd|rd|th): task
 
        private static readonly Regex RecurrenceRegex =
            new Regex(
                @"^((?<strict>\+)?((?<daily>daily)|(?<weekday>sun(?:day)?|mon(?:day)?|tue(?:sday)?|wed(?:nesday)?|thu(?:rsday)?|fri(?:day)?|sat(?:urday)?)|(?<monthday>[1-3]?\d)(st|nd|rd|th)?))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex RecurrenceRegexRaw =
            new Regex(RecurrenceRegex + @":(?:\s)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Dictionary<string, string> ShortToLongWeek = new Dictionary<string, string>
        {
            {"sun", "sunday"},
            {"mon", "monday"},
            {"tue", "tuesday"},
            {"wed", "wednesday"},
            {"thu", "thursday"},
            {"fri", "friday"},
            {"sat", "saturday"}
        };

        public RecurringTask(string raw) : base(RecurrenceRegexRaw.Replace(raw, ""))
        {
            ParseRaw(raw);
        }

		public RecurringTask(int id, string raw) : base(id, RecurrenceRegexRaw.Replace(raw, ""))
 		{
			Id = id;
		}


        public RecurringTask(string recurrence,
            string priority,
            List<string> projects, 
            List<string> contexts, 
            string body, 
            string dueDate = "", 
            bool completed = false) : base(priority, projects, contexts, body, dueDate, completed)
        {
            ParseRaw(recurrence);
        }

        private void ParseRaw(string raw)
        {
            var match = RecurrenceRegex.Match(raw);
            var strict = match.Groups["strict"].Value.Trim();
            var daily = match.Groups["daily"].Value.Trim();
            var weekday = match.Groups["weekday"].Value.Trim();
            var monthday = match.Groups["monthday"].Value.Trim();
                
            if (daily.Length > 0)
            {
                Frequency = RecurFrequency.Daily;
                RecurIndex = 0;
            }
            else if (weekday.Length > 0)
            {
                if (weekday.Length == 3)
                {
                    weekday = ShortToLongWeek[weekday.ToLowerInvariant()];
                }
                Frequency = RecurFrequency.Weekly;
                RecurIndex = (int)Enum.Parse<DayOfWeek>(weekday, true);
            }
            else
            {
                Frequency = RecurFrequency.Monthly;
                RecurIndex = int.Parse(monthday);
            }

            if (strict == "+")
            {
                Strict = true;
            }
        }

		public bool Equals(RecurringTask other)
		{
            if (!Equals(other as Task))
                return false;

			if (this.Frequency != other.Frequency)
				return false;

            if (this.RecurIndex != other.RecurIndex)
                return false;

            if (this.Strict != other.Strict)
                return false;

			return true;
		}

		public override int CompareTo(object obj)
		{
			// check null. This pointer is never null in C# methods.
			if (object.ReferenceEquals(obj, null))
				return -1;

			if (object.ReferenceEquals(this, obj))
				return 0;

			if (this.GetType() != obj.GetType())
				return -1;

			return CompareTo(obj as RecurringTask);			
		}

		public int CompareTo(RecurringTask other)
		{
            var taskCompareTo = CompareTo(other as Task);
            if (taskCompareTo != 0)
                return taskCompareTo;

			if (this.Frequency != other.Frequency)
				return -1;

            if (this.RecurIndex != other.RecurIndex)
                return -1;

            if (this.Strict != other.Strict)
                return -1;

			return 0;
		}

    }
}