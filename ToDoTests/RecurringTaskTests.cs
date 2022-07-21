using NUnit.Framework;
using System;
using System.Collections.Generic;
using ToDoLib;

namespace ToDoTests
{
    [TestFixture]
    public class RecurringTaskTests
    {
        List<string> _projects = new List<string>() { "+test" };
        List<string> _contexts = new List<string>() { "@work" };

        #region Create

        [Test]
        public void Create_Daily_Body()
        {
            var task = new RecurringTask("daily: run the dishwasher +test @work");
            var expectedTask = new RecurringTask("daily", "", _projects, _contexts, "run the dishwasher");
            Assert.AreEqual(expectedTask, task);
        }

        [Test]
        public void Create_Daily_Body_Strict()
        {
            var task = new RecurringTask("+daily: run the dishwasher +test @work");
            var expectedTask = new RecurringTask("+daily", "", _projects, _contexts, "run the dishwasher");
            Assert.True(task.Strict);
            Assert.AreEqual(expectedTask, task);
        }

        [Test]
        public void Create_Weekly_Body()
        {
            var task = new RecurringTask("thursday: (B) take out trash and recycling +test @work");
            var expectedTask = new RecurringTask("thursday", "(B)", _projects, _contexts, "take out trash and recycling");
            Assert.True(task.RecurIndex == (int)DayOfWeek.Thursday);
            Assert.AreEqual(expectedTask, task);
        }

        [Test]
        public void Create_Weekly_Strict()
        {
            var task = new RecurringTask("+fri: (B) time to make the donuts +test @work");
            var expectedTask = new RecurringTask("+fri", "(B)", _projects, _contexts, "time to make the donuts");
            Assert.True(task.Strict);
            Assert.True(task.RecurIndex == (int)DayOfWeek.Friday);
            Assert.AreEqual(expectedTask, task);
        }
        #endregion
                
    }
}
