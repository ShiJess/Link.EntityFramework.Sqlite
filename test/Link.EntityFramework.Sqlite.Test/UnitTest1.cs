using Link.EntityFramework.Sqlite.Test.DataModel;
using System;
using System.Data.Common;
using System.Data.SQLite;
using Xunit;

namespace Link.EntityFramework.Sqlite.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            //DbConnection connection = new SQLiteConnection("Data Source=Test.db;");
            //TestContext testContext = new TestContext(connection, true);
            //TestContext testContext = new TestContext("TestDataContext");
            TestContext testContext = new TestContext("Data Source=Test.db;");

            testContext.ASet.Add(new A { Name = "Name" + DateTime.Now.Ticks.ToString() });
            int count = testContext.SaveChanges();
            Assert.True(count > 0);
        }
    }
}
