﻿using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using ToDoLib;

namespace ToDoTests
{
    [TestFixture]
    public class TaskTests
    {
        List<string> _projects = new List<string>() { "+test" };
        List<string> _contexts = new List<string>() { "@work" };

        #region Create
        [Test]
        public void Create_Priority_Body_Project_Context()
        {
            var task = new Task("(A) This is a test task +test @work");

            var expectedTask = new Task("(A)", _projects, _contexts, "This is a test task");
            ClassicAssert.AreEqual(expectedTask, task);
        }

        [Test]
        public void Create_Priority_Body_Context_Project()
        {
            var task = new Task("(A) This is a test task @work +test");

            var expectedTask = new Task("(A)", _projects, _contexts, "This is a test task");
            ClassicAssert.AreEqual(expectedTask, task);
        }

        [Test]
        public void Create_Trailing_Whitespace()
        {
            var task = new Task("(A) This is a test task @work +test  ");

            var expectedTask = new Task("(A)", _projects, _contexts, "This is a test task");
            ClassicAssert.AreEqual(expectedTask, task);
        }

        [Test]
        public void Create_Null_Priority()
        {
            var task = new Task("This is a test task @work +test ");
    
            var expectedTask = new Task("", _projects, _contexts, "This is a test task");
            ClassicAssert.AreEqual(expectedTask, task);
        }

        
        [Test]
        public void Create_Priority_In_Body()
        {
            var task = new Task("Oh (A) This is a test task @work +test ");

            var expectedTask = new Task("", _projects, _contexts, "Oh (A) This is a test task");
            ClassicAssert.AreEqual(expectedTask, task);
        }

        [Test]
        public void Create_Priority_Context_Project_Body()
        {
            var task = new Task("(A) @work +test This is a test task");

            var expectedTask = new Task("(A)", _projects, _contexts, "This is a test task");
            ClassicAssert.AreEqual(expectedTask, task);
        }

        [Test]
        public void Create_Completed()
        {
            var task = new Task("X @work +test This is a test task");

            var expectedTask = new Task("", _projects, _contexts, "This is a test task", "", true);
            ClassicAssert.AreEqual(expectedTask, task);
        }

        [Test]
        public void Create_CompletedData()
        {
            var task = new Task("X 2011-05-10 (A) @work +test This is a test task");

            ClassicAssert.AreEqual(DateTime.Parse("2011-05-10"), task.CompletedDate.Value);
        }

        [Test]
        public void Create_UnCompleted()
        {
            var task = new Task("(A) @work +test This is a test task");

            var expectedTask = new Task("(A)", _projects, _contexts, "This is a test task");
            ClassicAssert.AreEqual(expectedTask, task);
        }

        [Test]
        public void Create_Multiple_Projects()
        {
            var task = new Task("(A) @work +test +test2 This is a test task");

            var expectedTask = new Task("(A)", new List<string>(){"+test", "+test2"}, _contexts, "This is a test task");
            ClassicAssert.AreEqual(expectedTask, task);
        }

        [Test]
        public void Create_Multiple_Contexts()
        {
            var task = new Task("(A) @work @home +test This is a test task");

            var expectedTask = new Task("(A)", _projects, new List<string>(){"@work" , "@home"} , "This is a test task");
            ClassicAssert.AreEqual(expectedTask, task);
        }

        [Test]
        public void Create_DueDate()
        {
            var task = new Task("(A) due:2011-05-08 @work @home +test This is a test task");

            var expectedTask = new Task("(A)", _projects, new List<string>() { "@work", "@home" }, "This is a test task", "2011-05-08", false);
            
            ClassicAssert.AreEqual(expectedTask, task);
        }

        [Test]
        public void Create_DueToday()
        {
            var task = new Task("(A) due:today @work @home +test This is a test task");

            string due = DateTime.Now.ToString("yyyy-MM-dd");

            var expectedTask = new Task("(A)", _projects, new List<string>() { "@work", "@home" }, "This is a test task", due, false);
            ClassicAssert.AreEqual(expectedTask, task);
        }

        [Test]
        public void Create_DueTomorrow()
        {
            var task = new Task("(A) due:tOmORRoW @work @home +test This is a test task");

            string due = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

            var expectedTask = new Task("(A)", _projects, new List<string>() { "@work", "@home" }, "This is a test task", due, false);
            ClassicAssert.AreEqual(expectedTask, task);
        }

        [Test]
        public void Create_DueDayOfWeek()
        {
            var task = new Task("(A) due:thUrsday @work @home +test This is a test task");

            var dueDate=DateTime.Now;
            do{
                dueDate=dueDate.AddDays(1);
            } while (!string.Equals(dueDate.ToString("dddd"), "thursday", StringComparison.CurrentCultureIgnoreCase));
            string due = dueDate.ToString("yyyy-MM-dd");

            var expectedTask = new Task("(A)", _projects, new List<string>() { "@work", "@home" }, "This is a test task", due, false);
            ClassicAssert.AreEqual(expectedTask, task);
        }

        [Test]
        public void Create_CreationDate()
        {
            var task = new Task("(A) 2011-05-07 due:2011-05-08 @work @home +test This is a test task");

            ClassicAssert.AreEqual(DateTime.Parse("2011-05-07"), task.CreationDate);
        }

		[Test]
		public void Create_Project_with_non_alpha()
		{
			var task = new Task("This is a test task +work&home");

			ClassicAssert.AreEqual("+work&home", task.Projects[0]);
		}
        #endregion

        #region ToString
        [Test]
        [Ignore("Incompatible with ClientConsole")]
        public void ToString_From_Raw()
        {
            var task = new Task("(A) @work +test This is a test task");
            ClassicAssert.AreEqual("(A) @work +test This is a test task", task.ToString());
        }

        [Test]
        public void ToString_From_Parameters()
        {
            var task = new Task("(A)", _projects, _contexts, "This is a test task");
            ClassicAssert.AreEqual("(A) This is a test task +test @work", task.ToString());
        }
        #endregion

        [Test]
        public void Completed_adds_x_to_begining()
        {
            var t = new Task("A new task");
            t.Completed = true;


        }

        #region Test Propery IsTaskDue

        [Test]
        public void Task_with_out_due_date()
        {
            var t = new Task("Task with out due date task");
            ClassicAssert.AreEqual(t.IsTaskDue, Due.NotDue);
        }

        [Test]
        public void Task_Complete_with_out_due_date()
        {
            var t = new Task("x Task Complete with out due date task");
            ClassicAssert.AreEqual(t.IsTaskDue, Due.NotDue);
        }

        [Test]
        public void Task_with_future_due_date()
        {
            var t = new Task("Task with future task due:" + DateTime.Now.AddDays(2).ToString("yyyy-MM-dd"));
            ClassicAssert.AreEqual(t.IsTaskDue, Due.NotDue);
        }

        [Test]
        public void Task_Complete_with_future_due_date()
        {
            var t = new Task("x Task Complete with future task due:" + DateTime.Now.AddDays(2).ToString("yyyy-MM-dd"));
            ClassicAssert.AreEqual(t.IsTaskDue, Due.NotDue);
        }

        [Test]
        public void Task_with_today_due_date()
        {
            var t = new Task("Task with today due:" + DateTime.Now.ToString("yyyy-MM-dd"));
            ClassicAssert.AreEqual(t.IsTaskDue, Due.Today);
        }

        [Test]
        public void Task_Complete_with_today_due_date()
        {
            var t = new Task("x Task Complete with today due:" + DateTime.Now.ToString("yyyy-MM-dd"));
            ClassicAssert.AreEqual(t.IsTaskDue, Due.NotDue);
        }

        [Test]
        public void Task_with_over_due_date()
        {
            var t = new Task("Task with overdue date due:" + DateTime.Now.AddDays(-4).ToString("yyyy-MM-dd"));
            ClassicAssert.AreEqual(t.IsTaskDue, Due.Overdue);

        }

        [Test]
        public void Task_Complete_with_over_due_date()
        {
            var t = new Task("x Task Complete with overdue date due:" + DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd"));
            ClassicAssert.AreEqual(t.IsTaskDue, Due.NotDue);
        }

        #endregion
    }
}
