﻿using System;
using System.Collections.Generic;
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

	public class Task : IComparable
	{
		private const string CompletedPattern = @"^X\s((\d{4})-(\d{2})-(\d{2}))?";
		private const string priorityPattern = @"^(?<priority>\([A-Z]\)\s)";
		private const string createdDatePattern = @"(?<date>(\d{4})-(\d{2})-(\d{2}))";
		private const string dueRelativePattern = @"due:(?<dateRelative>today|tomorrow|monday|tuesday|wednesday|thursday|friday|saturday|sunday)";
		private const string dueDatePattern = @"due:(?<date>(\d{4})-(\d{2})-(\d{2}))";
        private const string projectPattern = @"(?<proj>\+[^\s]+)";
        private const string contextPattern = @"(?<context>\@[^\s]+)";

		public int Id { get; internal set; }
		public List<string> Projects { get; set; }
		public List<string> Contexts { get; set; }
		public string DueDate { get; set; }
		public DateTime? CompletedDate { get; set; }
		public string CreationDate { get; set; }
		public string Priority { get; set; }
		public string Body { get; set; }

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

				DateTime tmp = new DateTime();

				if (!DateTime.TryParse(DueDate, out tmp))
					return Due.NotDue;

					if (tmp < DateTime.Today)
					return Due.Overdue;
					if (tmp == DateTime.Today)
					return Due.Today;
				return Due.NotDue;
				}
				}

		// Parsing needs to comply with these rules: https://github.com/ginatrapani/todo.txt-touch/wiki/Todo.txt-File-Format

		public Task(string raw)
		{
			ParseRaw(raw);
			this.Id = -1;
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
			DueDate = dueDate;
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
			// <id> <completed_flag> <completed_date> <priority> <body> <projects> <contexts>
			var str = "";
			str = string.Format("{0}{1}{2} {3} {4}",
				Completed ? "x " + CompletedDate.Value.ToString("yyyy-MM-dd") : "",
				String.IsNullOrEmpty(Priority) ? "" : Priority + " ",
                Body, string.Join(" ", Projects), string.Join(" ", Contexts));

			return str;
		}

		public Task Clone()
		{
			return (Task)this.MemberwiseClone();
		}

		public int CompareTo(object obj)
		{
			var other = (Task)obj;

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

		//TODO priority regex need to only recognize upper case single chars
		private void ParseRaw(string raw)
		{
			// because we are removing matches as we go, the order we process is important. It must be:
			// - completed
			// - priority
			// - due date
			// - created date
			// - projects | contexts
			// What we have left is the body

			var reg = new Regex(CompletedPattern, RegexOptions.IgnoreCase);

			var s = reg.Match(raw).Value.Trim();

			if (string.IsNullOrEmpty(s))
			{
				Completed = false;
				CompletedDate = null;
			}
			else
			{
				Completed = true;
				// TODO Handle error conditions.
				if (s.Length > 1)
					CompletedDate = DateTime.Parse(s.Substring(2));
			}
			raw = reg.Replace(raw, "");


			reg = new Regex(priorityPattern, RegexOptions.IgnoreCase);
			Priority = reg.Match(raw).Groups["priority"].Value.Trim();
			raw = reg.Replace(raw, "");

			reg = new Regex(dueDatePattern);
			DueDate = reg.Match(raw).Groups["date"].Value.Trim();
			raw = reg.Replace(raw, "");

			reg = new Regex(createdDatePattern);
			CreationDate = reg.Match(raw).Groups["date"].Value.Trim();
			raw = reg.Replace(raw, "");

			Projects = new List<string>();
			reg = new Regex(projectPattern);
			var projects = reg.Matches(raw);

			foreach (Match project in projects)
			{
				var p = project.Groups["proj"].Value.Trim();
				Projects.Add(p);
			}

			raw = reg.Replace(raw, "");


			Contexts = new List<string>();
			reg = new Regex(contextPattern);
			var contexts = reg.Matches(raw);

			foreach (Match context in contexts)
			{
				var c = context.Groups["context"].Value.Trim();
				Contexts.Add(c);
			}

			raw = reg.Replace(raw, "");


			Body = raw.Trim();
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

				var newPriority = (Char)((int)(current) + asciiShift);

				if (Char.IsLetter(newPriority))
				{
					SetPriority(newPriority);
				}
			}
		}
	}
}