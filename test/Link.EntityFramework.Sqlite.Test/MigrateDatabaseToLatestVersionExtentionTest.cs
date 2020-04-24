using Link.EntityFramework.Sqlite.Test.DataModel;
using System;
using System.Data.Entity;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Link.EntityFramework.Sqlite.Test
{
    public class MigrateDatabaseToLatestVersionExtentionTest
    {
        /// <summary>
        /// 测试管理器中输出
        /// </summary>
        ITestOutputHelper outputHelper;
        public MigrateDatabaseToLatestVersionExtentionTest(ITestOutputHelper output)
        {
            this.outputHelper = output;
        }

        [Fact]
        public void TestMigrateAndAdd()
        {
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<TestContext, MigrationConfiguration>(true));
            Database.SetInitializer(new MigrateDatabaseToLatestVersionExtention<TestContext, MigrationConfiguration>(true));

            //DbConnection connection = new SQLiteConnection("Data Source=Test.db;");
            //TestContext testContext = new TestContext(connection, true);
            //TestContext testContext = new TestContext("TestDataContext");
            TestContext testContext = new TestContext("Data Source=Test.db;");

            testContext.ASet.Add(new A { Name = "Name" + DateTime.Now.Ticks.ToString() });
            int count = testContext.SaveChanges();
            Assert.True(count > 0);
        }

        [Fact]
        public void TestMigrateAndSearch()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersionExtention<TestContext, MigrationConfiguration>(true));

            TestContext testContext = new TestContext("Data Source=Test.db;");
            var model = testContext.Set<A>().ToList();
            outputHelper.WriteLine(model.Count().ToString());
            foreach (var item in model)
            {
                outputHelper.WriteLine(item.Name ?? "");
            }
        }

        [Fact]
        public void TestMigrateAndAdd_ClearOldVersion()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersionExtention<TestContext, MigrationConfiguration>(true, true));

            TestContext testContext = new TestContext("Data Source=Test.db;");

            testContext.ASet.Add(new A { Name = "Name" + DateTime.Now.Ticks.ToString() });
            int count = testContext.SaveChanges();
            Assert.True(count > 0);
        }

        [Fact]
        public void TestMigrateAndSearch_ClearOldVersion()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersionExtention<TestContext, MigrationConfiguration>(true, true));

            using (TestContext testContext = new TestContext("Data Source=Test.db;"))
            {
                var model1 = testContext.Set<Bbb>().ToList();
                var model = testContext.Set<A>().ToList();
                //outputHelper.WriteLine(model.Count().ToString());
                //foreach (var item in model)
                //{
                //    outputHelper.WriteLine(item.Name ?? "");
                //}
            }
        }
    }
}