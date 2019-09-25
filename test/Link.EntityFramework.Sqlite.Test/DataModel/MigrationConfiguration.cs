using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.SQLite.EF6.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.EntityFramework.Sqlite.Test.DataModel
{
    internal class MigrationConfiguration : DbMigrationsConfiguration<TestContext>
    {
        public MigrationConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            SetSqlGenerator("System.Data.SQLite", new SQLiteMigrationSqlGenerator()); 
        }
    }
}
