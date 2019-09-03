
# Link.EntityFramework.Sqlite
# Entity Framework 6 Sqlite

[![NuGet](https://img.shields.io/nuget/v/Link.EntityFramework.Sqlite.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Link.EntityFramework.Sqlite/)


**Project Description**  
Migrations for Entity Framework 6 SQLite provider  
  
**Limitations:**  
 - Relationships are not enforced with constraints  
 - There can be only one identity column per table and will be created as integer and primary key (other primary keys will be ignored)  
 - ...  
  
**How to use it**  
 - Download the library (using NuGet)  
 - Create a migration configuration  
 - Setup the migration configuration (usually during first context creation)  
  
_Example_  
  
```c#
    class Context : DbContext
    {
        static Context()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<Context, ContextMigrationConfiguration>(true));
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

## Fix

* Rename column imp;
* Delete column imp —— use rename column,not realy delete
* some other bug;


## RoadMap

* add test project - use xunit
* alter table process improve - e.g:delete column
    * [Official Recommandition Process](https://sqlite.org/lang_altertable.html#otheralter) - All should in one transaction:
    ```
    1. Create new table
    2. Copy data
    3. Drop old table
    4. Rename new into old
    ```

## Refrence

* [EnfityFramework Core](https://github.com/aspnet/EntityFrameworkCore)
* [EntityFramework](https://github.com/aspnet/EntityFramework6)