  
_Example_  
  
```c#
    class Context : DbContext
    {
        static Context()
        {
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<Context, ContextMigrationConfiguration>(true));
            Database.SetInitializer(new MigrateDatabaseToLatestVersionSqliteExt<Context, ContextMigrationConfiguration>(true));
        }

        // DbSets
    }

    internal sealed class ContextMigrationConfiguration : DbMigrationsConfiguration<Context>
    {
        public ContextMigrationConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            SetSqlGenerator("System.Data.SQLite", new SQLiteMigrationSqlGenerator());
        }
    }

```
