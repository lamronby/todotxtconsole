using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CommonExtensions;

namespace ToDoLib
{
	/// <summary>
	/// Constraints:
	///   Can accept all supported todo.txt formats, but updates are written
	///   to disk in a specific (compatible) format.
	/// Changes from original ToDoLib Task class
	///   Removed the Raw property.
	///	  Comparisons and updates are derived from the other properties, not 
	///   from Raw.
	///   Added an Id property for task manipulation via Id.
	/// </summary>
	public enum Due
	{
		NotDue,
		Today,
		Overdue	
	}

	public class Task : IComparable, IComparable<Task>, IEquatable<Task>
	{
		// Following original todo.txt rules, each task may start with up to four known fields:
		// "x": marks completion
		// (A): priority (optional)
		// YYYY-MM-DD: completion date
		// YYYY-MM-DD: creation date
		private static readonly Regex InitialFieldsRegex =
			new Regex(
				@"^(?<completed>x\s)?(?<priority>\([A-Z]\)\s)?(?<date1>((\d{4})-(\d{2})-(\d{2})\s))?(?<date2>((\d{4})-(\d{2})-(\d{2})\s))?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex PriorityRegex = new Regex(@"^(?<priority>\([A-Z]\)\s)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private const string RelativeDatePatternBare =
			@"(?<dateRelative>today|tomorrow|(?<weekday>mon(?:day)?|tue(?:sday)?|wed(?:nesday)?|thu(?:rsday)?|fri(?:day)?|sat(?:urday)?|sun(?:day)?))";

		private static readonly Regex RelativeDatePatternRegex =
			new Regex(RelativeDatePatternBare, RegexOptions.Compiled | RegexOptions.IgnoreCase);
		
		private static readonly Regex DueRelativeRegex = new Regex(@"\bdue:" + RelativeDatePatternBare + @"\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex DueDateRegex = new Regex(@"\bdue:(?<date>(\d{4})-(\d{2})-(\d{2}))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex ThresholdRelativeRegex = new Regex(@"\bt:"+ RelativeDatePatternBare, RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex ThresholdDateRegex = new Regex(@"\bt:(?<date>(\d{4})-(\d{2})-(\d{2}))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex ProjectRegex = new Regex(@"(?<proj>\+[^\s]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex ContextRegex = new Regex(@"(?<context>\@[^\s]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private const string DateOutputFormat = "yyyy-MM-dd";
	
		public int Id { get; internal init; }
		public List<string> Projects { get; private set; }
		public List<string> Contexts { get; private set; }
		public DateTime? DueDate { get; private set; }
		public DateTime? CompletedDate { get; set; }
		public DateTime? CreationDate { get; private set; }
		public DateTime? ThresholdDate { get; private set; }
		public string Priority { get; private set; }
		public string Body { get; private set; }

		private bool _completed;

		public bool Completed
		{
			get
			{
				return _completed;
			}

			set
			{
				_completed = value;
				if (_completed)
				{
					this.CompletedDate = DateTime.Now;
					this.Priority = "";
				}
				else
				{
					this.CompletedDate = null;
				}
			}
		}

		public Due IsTaskDue
		{
			get
			{
				if (Completed)
					return Due.NotDue;

				if (!DueDate.HasValue)
					return Due.NotDue;
				
				if (DueDate < DateTime.Today)
					return Due.Overdue;
				if (DueDate == DateTime.Today)
					return Due.Today;
				return Due.NotDue;
			}
		}

		// Parsing needs to comply with these rules: https://github.com/ginatrapani/todo.txt-touch/wiki/Todo.txt-File-Format

		public Task(string raw)
		{
			this.Contexts = new List<string>();
			this.Projects = new List<string>();
			this.Id = -1;
			if (raw == null) return;
			
			ParseRaw(raw);
		}

		public Task(int id, string raw)
			: this(raw)
 		{
			Id = id;
		}

		public Task(string priority, List<string> projects, List<string> contexts, string body, string dueDate = "", bool completed = false)
		{
			Priority = priority;
			Projects = projects;
			Contexts = contexts;
			DueDate = dueDate == "" ? null : DateTime.Parse(dueDate);
			Body = body;
			Completed = completed;
		}

		public void Update(string raw)
		{
			ParseRaw(raw);
		}

		public override string ToString()
		{
			// Format:
			// <completed_flag> <completed_date> <creation_date> <priority> <body> <due_date> <projects> <contexts>
			return string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
				Completed ? "x " + $"{CompletedDate.Value.ToString(DateOutputFormat)} " : "",
				CreationDate.HasValue ? $"{CreationDate.Value.ToString(DateOutputFormat)} " : "",
				string.IsNullOrEmpty(Priority) ? "" : Priority + " ",
				Body,
				ThresholdDate.HasValue ? $" t:{ThresholdDate.Value.ToString(DateOutputFormat)} " : "",
				DueDate.HasValue ? $" due:{DueDate.Value.ToString(DateOutputFormat)} " : "",
				Projects == null ? "" : " " + string.Join(" ", Projects),
				Contexts == null ? "" : " " + string.Join(" ", Contexts));
		}

		public Task Clone()
		{
			return (Task)this.MemberwiseClone();
		}

		public override bool Equals(object right)
		{
			// check null. This pointer is never null in C# methods.
			if (object.ReferenceEquals(right, null))
				return false;

			if (object.ReferenceEquals(this, right))
				return true;

			if (this.GetType() != right.GetType())
				return false;

			return this.Equals(right as Task);
		}

		public bool Equals(Task other)
		{
			if (this.Completed != other.Completed)
				return false;

			if (this.CompletedDate.HasValue && !other.CompletedDate.HasValue)
				return false;

			if (!this.CompletedDate.HasValue && other.CompletedDate.HasValue)
				return false;
				
			if (this.CompletedDate.HasValue && other.CompletedDate.HasValue
				&& this.CompletedDate.Value.Equals(other.CompletedDate.Value))
					return false;

			if (this.Priority.CompareTo(other.Priority) != 0)
				return false;

			if (this.Body.CompareTo(other.Body) != 0)
				return false;

			if (string.Join(" ", this.Projects).CompareTo(string.Join(" ", other.Projects)) != 0)
				return false;

			if (string.Join(" ", this.Contexts).CompareTo(string.Join(" ", other.Contexts)) != 0)
				return false;

			return true;
		}

		public virtual int CompareTo(object obj)
		{
			// check null. This pointer is never null in C# methods.
			if (object.ReferenceEquals(obj, null))
				return -1;

			if (object.ReferenceEquals(this, obj))
				return 0;

			if (this.GetType() != obj.GetType())
				return -1;

			return CompareTo(obj as Task);			
		}

		public int CompareTo(Task other)
		{
			if (this.Completed != other.Completed)
				return -1;

			if (this.CompletedDate.HasValue && !other.CompletedDate.HasValue)
				return -1;
			if (!this.CompletedDate.HasValue && other.CompletedDate.HasValue)
				return -1;
			if (this.CompletedDate.HasValue && other.CompletedDate.HasValue)
			{
				int dateCompare = this.CompletedDate.Value.CompareTo(other.CompletedDate.Value);
				if (dateCompare != 0)
					return dateCompare;
			}

			int priCompare = this.Priority.CompareTo(other.Priority);
			if (priCompare != 0)
				return priCompare;

			int bodyCompare = this.Body.CompareTo(other.Body);
			if (bodyCompare != 0)
				return bodyCompare;

			int projectCompare = string.Join(" ", this.Projects).CompareTo(string.Join(" ", other.Projects));
			if (projectCompare != 0)
				return projectCompare;

			int contextCompare = string.Join(" ", this.Contexts).CompareTo(string.Join(" ", other.Contexts));
			if (contextCompare != 0)
				return contextCompare;

			return 0;
		}

		public void IncPriority()
		{
			ChangePriority(-1);
		}

		public void DecPriority()
		{
			ChangePriority(1);
		}

		public void SetPriority(char priority)
		{
			var priorityString = char.IsLetter(priority) ? new string(new char[] { '(', priority, ')' }) : "";

			Priority = priorityString;
		}

		private void ParseRaw(string raw)
		{
			// because we are removing matches as we go, the order we process is important. It must be:
			// - completed
			// - priority
			// - due date
			// - created date
			// - projects | contexts
			// What we have left is the body
			
			raw = raw.Replace(Environment.NewLine, ""); //make sure it's just on one line

			Body = raw.ParseRawElement(InitialFieldsRegex, (r) =>
			{
				var result = InitialFieldsRegex.Match(r);
				var date1Str = result.Groups["date1"].Value.Trim();
				var date2Str = result.Groups["date2"].Value.Trim();
				
				if (string.IsNullOrEmpty(result.Groups["completed"].Value.Trim()))
				{
					Completed = false;
					CompletedDate = null;
				}
				else
				{
					Completed = true;
					// If the task is completed, completion date must be date1
					if (date1Str.Length > 1)
					{
						CompletedDate = DateTime.Parse(date1Str);
					}
				}
				
				Priority = PriorityRegex.Match(r).Groups["priority"].Value.Trim();
				// If the task is not completed, created date is date1, otherwise date2.
				if (Completed && date2Str.Length > 0)
				{
					CreationDate = DateTime.Parse(date2Str);
				}
				else if (!Completed && date1Str.Length > 0)
				{
					CreationDate = DateTime.Parse(date1Str);
				}
			}).ParseRawElement(DueRelativeRegex, (r) =>
			{
				var result = DueRelativeRegex.Match(r).Groups["dateRelative"].Value.Trim();
				var relativeDueDate = ParseRelativeDate(result);
				if (relativeDueDate.HasValue)
				{
					DueDate = relativeDueDate;
				}
			}).ParseRawElement(DueDateRegex, (r) =>
			{
				var match = DueDateRegex.Match(r).Groups["date"].Value.Trim();
				if (!match.IsNullOrEmpty())
				{
					DueDate = DateTime.Parse(match);
				}
			}).ParseRawElement(ThresholdRelativeRegex, (r) =>
			{
				var result = ThresholdRelativeRegex.Match(r).Groups["dateRelative"].Value.Trim();
				var thresholdDate = ParseRelativeDate(result);
				if (thresholdDate.HasValue)
				{
					ThresholdDate = thresholdDate;
				}
			}).ParseRawElement(ThresholdDateRegex, (r) =>
			{
				var match = ThresholdDateRegex.Match(r).Groups["date"].Value.Trim();
				if (!match.IsNullOrEmpty())
				{
					ThresholdDate = DateTime.Parse(match);
				}
			}).ParseRawElement(ProjectRegex, (r) =>
			{
				var projects = ProjectRegex.Matches(r);

				foreach (Match project in projects)
				{
					var p = project.Groups["proj"].Value.Trim();
					Projects.Add(p);
				}
			}).ParseRawElement(ContextRegex, (r) =>
			{
				var contexts = ContextRegex.Matches(r);

				foreach (Match context in contexts)
				{
					var c = context.Groups["context"].Value.Trim();
					Contexts.Add(c);
				}
			}).Trim();
			
			// Body = raw.ParseRawElement(CompletedRegex, (r) =>
			// {
			// 	var s = CompletedRegex.Match(r).Value.Trim();
			// 	if (string.IsNullOrEmpty(s))
			// 	{
			// 		Completed = false;
			// 		CompletedDate = null;
			// 	}
			// 	else
			// 	{
			// 		Completed = true;
			// 		// TODO Handle error conditions.
			// 		if (s.Length > 1)
			// 			CompletedDate = DateTime.Parse(s.Substring(2));
			// 	}
			// }).ParseRawElement(PriorityRegex, (r) =>
			// {
			// 	Priority = PriorityRegex.Match(r).Groups["priority"].Value.Trim();
			// }).ParseRawElement(DueDateRegex, (r) =>
			// {
			// 	DueDate = DueDateRegex.Match(r).Groups["date"].Value.Trim();
			// }).ParseRawElement(CreatedDateRegex, (r) =>
			// {
			// 	CreationDate = CreatedDateRegex.Match(r).Groups["date"].Value.Trim();
			// }).ParseRawElement(ProjectRegex, (r) =>
			// {
			// 	Projects = new List<string>();
			// 	var projects = ProjectRegex.Matches(r);
			//
			// 	foreach (Match project in projects)
			// 	{
			// 		var p = project.Groups["proj"].Value.Trim();
			// 		Projects.Add(p);
			// 	}
			// }).ParseRawElement(ContextRegex, (r) =>
			// {
			// 	Contexts = new List<string>();
			// 	var contexts = ContextRegex.Matches(r);
			//
			// 	foreach (Match context in contexts)
			// 	{
			// 		var c = context.Groups["context"].Value.Trim();
			// 		Contexts.Add(c);
			// 	}
			// }).Trim();
		}
		
        private DateTime? ParseRelativeDate(string dateStr)
        {
	        //Replace relative days with hard date
            //Supports english: 'today', 'tomorrow', and full weekdays ('monday', 'tuesday', etc)
            //If today is the specified weekday, due date will be in one week
            //TODO other languages
            var match = RelativeDatePatternRegex.Match(dateStr);
            var dateRelative = match.Groups["dateRelative"].Value.Trim();

            if (dateRelative.IsNullOrEmpty()) return null;
            
            var isValid = false;
            var date = DateTime.Now;
            dateRelative = dateRelative.ToLower();
            if (dateRelative == "today")
            {
	            isValid = true;
            }
            else if (dateRelative == "tomorrow")
            {
	            date = date.AddDays(1);
	            isValid = true;
            }
            else if (match.Groups["weekday"].Success)
            {
	            var count = 0;
	            var lookingForShortDay = dateRelative.Substring(0, 3);

	            //if day of week, add days to today until weekday matches input
	            //if today is the specified weekday, due date will be in one week
	            do
	            {
		            count++;
		            date = date.AddDays(1);
		            isValid = string.Equals(date.ToString("ddd", new CultureInfo("en-US")),
			            lookingForShortDay,
			            StringComparison.CurrentCultureIgnoreCase);
	            } while (!isValid && (count < 7));
	            // The count check is to prevent an endless loop in case of other culture.
            }

            return isValid ? date : null;
        }
        
		// NB, you need asciiShift +1 to go from A to B, even though that's a 'decrease' in priority
		private void ChangePriority(int asciiShift)
		{
			if (Priority.IsNullOrEmpty())
			{
				SetPriority('A');
			}
			else
			{
				var current = Priority[1];

				var newPriority = (char)((int)(current) + asciiShift);

				if (char.IsLetter(newPriority))
				{
					SetPriority(newPriority);
				}
			}
		}
	}
}