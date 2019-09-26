using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.EntityFramework.Sqlite.Test.DataModel
{
    internal class TestContext : DbContext
    {
        public TestContext(string connectionStr) : base(connectionStr)
        {
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<TestContext, MigrationConfiguration>(true));
            Database.SetInitializer(new MigrateDatabaseToLatestVersionExtention<TestContext, MigrationConfiguration>(true));
        }

        public TestContext(DbConnection existingConnection, bool contextOwnsConnection) : base(existingConnection, contextOwnsConnection)
        {
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<TestContext, MigrationConfiguration>(true));
            Database.SetInitializer(new MigrateDatabaseToLatestVersionExtention<TestContext, MigrationConfiguration>(true));
        }

        public virtual DbSet<A> ASet { get; set; }

    }
}
