using System.Configuration;
using System.IO;
using ClientConsole;
using NUnit.Framework;
using ToDoLib;

namespace ToDoTests
{
    [TestFixture]
    class ConsoleConfigTests
    {
        [TestFixtureSetUp]
        public void TFSetup()
        {
            var appSettings = ConfigurationManager.AppSettings;
            appSettings.Clear();

        //<add key="FilePath" value="C:\\Users\\fpd-yoga-user2\\Dropbox\\todo.txt" />
        //<add key="CurrentSort" value="" />
        //<add key="CurrentGrouping" value="" />
        //<add key="FilterText" value="" />
        //<add key="ArchiveFilePath" value="C:\\Users\\fpd-yoga-user2\\Dropbox\\done.txt" />
        //<add key="AddCreationDate" value="true" />
        //<add key="DebugLoggingOn" value="true" />

            if (!File.Exists(Data.TestDataPath))
                File.WriteAllText(Data.TestDataPath, "");
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            if (File.Exists(Data.TestDataPath))
                File.Delete(Data.TestDataPath);
        }

        [Test]
        public void HandlesEmptyConfig()
        {
            var appSettings = ConfigurationManager.AppSettings;
            appSettings.Clear();

            var config = ConsoleConfig.Instance;

            Assert.IsNotNull(config, "ConsoleConfig instance is null.");
        }

        [Test]
        public void FilePathValid()
        {
            var filePath = @"C:\Temp\todo.txt";

            var appSettings = ConfigurationManager.AppSettings;
            appSettings.Clear();

            appSettings.Add("FilePath", filePath);

            var config = ConsoleConfig.Instance;

            Assert.AreEqual(filePath, config.FilePath);
        }
    }
}
