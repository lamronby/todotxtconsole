using NUnit.Framework;
using System;
using System.IO;
using ToDoLib;

namespace ToDoTests
{
    [TestFixture]
    public class RecurringTaskListTests
    {
        public static string TestDataPath = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}recurringtesttasks.txt";

		[OneTimeSetUp]
		public void Setup()
		{
            Log.LogLevel = LogLevel.Debug;
			if (!File.Exists(TestDataPath))
				File.WriteAllText(TestDataPath, "");
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (File.Exists(TestDataPath))
				File.Delete(TestDataPath);
		}
        
        [Test]
        public void Load_From_File()
        {
            var tl = new RecurTaskList(TestDataPath);
            var tasks = tl.Tasks;
        }

        
    }
}
