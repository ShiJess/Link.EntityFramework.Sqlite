
# Link.EntityFramework.Sqlite —— Entity Framework 6 Sqlite

[![NuGet](https://img.shields.io/nuget/v/Link.EntityFramework.Sqlite.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/Link.EntityFramework.Sqlite/)


* [RoadMap](RoadMap.md)
* [More Guide](docs/ReadMe.md)

## Project Description

Migrations for Entity Framework 6 SQLite provider  
  
**Limitations:**  

 - Relationships are not enforced with constraints  
 - There can be only one identity column per table and will be created as integer and primary key (other primary keys will be ignored)  
 - ...  
  
## Simplest Usage

 - Download the library (using NuGet)  
 - Create a migration configuration  
 - Setup the migration configuration (usually during first context creation)  

```csharp
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

### Old Version Example

> Not Realy Delete Column,Just Rename Column
  
``` csharp
class Context : DbContext
{
    static Context()
    {
        Database.SetInitializer(new MigrateDatabaseToLatestVersion<Context, ContextMigrationConfiguration>(true));
    }

    // DbSets
}
```

### New Version Example

```csharp
class Context : DbContext
{
    static Context()
    {
        Database.SetInitializer(new MigrateDatabaseToLatestVersionExtention<Context, ContextMigrationConfiguration>(true));
    }

    // DbSets
}
```

## Fix



## Implementation

* Delete Column Imp
    * Rename Column
    * Record Delete Column Info
    * Add Additional Migration
        * Create Temp Table Base Record Info
        * Copy Data To Temp Table
        * Drop Original Table
        * Rename Temp Table

## Refrence

* [Sqlite AlterTable:Official Recommandition Process](https://sqlite.org/lang_altertable.html#otheralter)
* [EntityFramework 6](https://github.com/aspnet/EntityFramework6)
* [EnfityFramework Core](https://github.com/aspnet/EntityFrameworkCore)
* [Sqlite Logo](https://en.wikipedia.org/wiki/File:Sqlite-square-icon.svg)