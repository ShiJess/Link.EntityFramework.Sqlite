﻿using Link.EntityFramework.Sqlite.Migrations.Record;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Diagnostics;

namespace System.Data.SQLite.EF6.Migrations
{
    /// <summary>
    /// Migration Ddl generator for SQLite
    /// </summary>
    public class SQLiteMigrationSqlGenerator : MigrationSqlGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        private string recordtablename = RecordContext.DefaultTableName;

        const string BATCHTERMINATOR = ";\r\n";

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteMigrationSqlGenerator"/> class.
        /// </summary>
        public SQLiteMigrationSqlGenerator()
        {
            base.ProviderManifest = ((DbProviderServices)(new SQLiteProviderFactory()).GetService(typeof(DbProviderServices))).GetProviderManifest("");
        }

        /// <summary>
        /// Converts a set of migration operations into database provider specific SQL.
        /// </summary>
        /// <param name="migrationOperations">The operations to be converted.</param>
        /// <param name="providerManifestToken">Token representing the version of the database being targeted.</param>
        /// <returns>
        /// A list of SQL statements to be executed to perform the migration operations.
        /// </returns>
        public override IEnumerable<MigrationStatement> Generate(IEnumerable<MigrationOperation> migrationOperations, string providerManifestToken)
        {
            List<MigrationStatement> migrationStatements = new List<MigrationStatement>();

            foreach (MigrationOperation migrationOperation in migrationOperations)
                migrationStatements.Add(GenerateStatement(migrationOperation));
            return migrationStatements;
        }

        private MigrationStatement GenerateStatement(MigrationOperation migrationOperation)
        {
            MigrationStatement migrationStatement = new MigrationStatement();
            migrationStatement.BatchTerminator = BATCHTERMINATOR;
            migrationStatement.Sql = GenerateSqlStatement(migrationOperation);
            return migrationStatement;
        }

        private string GenerateSqlStatement(MigrationOperation migrationOperation)
        {
            dynamic concreteMigrationOperation = migrationOperation;
            return GenerateSqlStatementConcrete(concreteMigrationOperation);
        }

        private string GenerateSqlStatementConcrete(MigrationOperation migrationOperation)
        {
            Debug.Assert(false);
            return string.Empty;
        }


        #region History operations

        private string GenerateSqlStatementConcrete(HistoryOperation migrationOperation)
        {
            SQLiteDdlBuilder ddlBuilder = new SQLiteDdlBuilder();

            foreach (DbModificationCommandTree commandTree in migrationOperation.CommandTrees)
            {
                List<DbParameter> parameters;
                // Take care because here we have several queries so we can't use parameters...
                switch (commandTree.CommandTreeKind)
                {
                    case DbCommandTreeKind.Insert:
                        ddlBuilder.AppendSql(SQLiteDmlBuilder.GenerateInsertSql((DbInsertCommandTree)commandTree, out parameters, true));
                        break;
                    case DbCommandTreeKind.Delete:
                        ddlBuilder.AppendSql(SQLiteDmlBuilder.GenerateDeleteSql((DbDeleteCommandTree)commandTree, out parameters, true));
                        break;
                    case DbCommandTreeKind.Update:
                        ddlBuilder.AppendSql(SQLiteDmlBuilder.GenerateUpdateSql((DbUpdateCommandTree)commandTree, out parameters, true));
                        break;
                    case DbCommandTreeKind.Function:
                    case DbCommandTreeKind.Query:
                    default:
                        throw new InvalidOperationException(string.Format("Command tree of type {0} not supported in migration of history operations", commandTree.CommandTreeKind));
                }
                ddlBuilder.AppendSql(BATCHTERMINATOR);
            }

            return ddlBuilder.GetCommandText();

        }

        #endregion

        #region Move operations (not supported by Jet)

        private string GenerateSqlStatementConcrete(MoveProcedureOperation migrationOperation)
        {
            throw new NotSupportedException("Move operations not supported by SQLite");
        }

        private string GenerateSqlStatementConcrete(MoveTableOperation migrationOperation)
        {
            throw new NotSupportedException("Move operations not supported by SQLite");
        }

        #endregion


        #region Procedure related operations (not supported by Jet)
        private string GenerateSqlStatementConcrete(AlterProcedureOperation migrationOperation)
        {
            throw new NotSupportedException("Procedures are not supported by SQLite");
        }

        private string GenerateSqlStatementConcrete(CreateProcedureOperation migrationOperation)
        {
            throw new NotSupportedException("Procedures are not supported by SQLite");
        }


        private string GenerateSqlStatementConcrete(DropProcedureOperation migrationOperation)
        {
            throw new NotSupportedException("Procedures are not supported by SQLite");
        }


        private string GenerateSqlStatementConcrete(RenameProcedureOperation migrationOperation)
        {
            throw new NotSupportedException("Procedures are not supported by SQLite");
        }

        #endregion


        #region Rename operations


        private string GenerateSqlStatementConcrete(RenameColumnOperation migrationOperation)
        {
            //throw new NotSupportedException("Cannot rename objects with Jet");
            //ALTER TABLE tablename RENAME COLUMN Height1 TO Height;

            SQLiteDdlBuilder ddlBuilder = new SQLiteDdlBuilder();

            ddlBuilder.AppendSql("ALTER TABLE ");
            ddlBuilder.AppendIdentifier(migrationOperation.Table);
            ddlBuilder.AppendSql(" RENAME COLUMN ");

            ddlBuilder.AppendIdentifier(migrationOperation.Name);
            ddlBuilder.AppendSql(" TO ");
            ddlBuilder.AppendIdentifier(migrationOperation.NewName);
            ddlBuilder.AppendNewLine();

            return ddlBuilder.GetCommandText();
        }

        private string GenerateSqlStatementConcrete(RenameIndexOperation migrationOperation)
        {
            //忽略重命名索引
            return string.Empty;
            //throw new NotSupportedException("Cannot rename objects with Jet");
        }

        private string GenerateSqlStatementConcrete(RenameTableOperation migrationOperation)
        {
            SQLiteDdlBuilder ddlBuilder = new SQLiteDdlBuilder();

            ddlBuilder.AppendSql("ALTER TABLE ");
            ddlBuilder.AppendIdentifier(migrationOperation.Name);
            ddlBuilder.AppendSql(" RENAME TO ");
            ddlBuilder.AppendIdentifier(migrationOperation.NewName);

            return ddlBuilder.GetCommandText();
        }

        #endregion

        #region Columns
        private string GenerateSqlStatementConcrete(AddColumnOperation migrationOperation)
        {
            SQLiteDdlBuilder ddlBuilder = new SQLiteDdlBuilder();

            ddlBuilder.AppendSql("ALTER TABLE ");
            ddlBuilder.AppendIdentifier(migrationOperation.Table);
            ddlBuilder.AppendSql(" ADD COLUMN ");

            ColumnModel column = migrationOperation.Column;

            ddlBuilder.AppendIdentifier(column.Name);
            ddlBuilder.AppendSql(" ");
            TypeUsage storeType = ProviderManifest.GetStoreType(column.TypeUsage);
            ddlBuilder.AppendType_Addcolumn(storeType, column.IsNullable ?? true, column.IsIdentity, column.DefaultValue ?? column.ClrDefaultValue);

            //column.DefaultValue
            ddlBuilder.AppendNewLine();


            return ddlBuilder.GetCommandText();
        }


        private string GenerateSqlStatementConcrete(DropColumnOperation migrationOperation)
        {
            //throw new NotSupportedException("Drop column not supported by SQLite");
            //sqlite不支持删除列，故为避免后续操作，将删除的列名重命名           
            SQLiteDdlBuilder ddlBuilder = new SQLiteDdlBuilder();

            ddlBuilder.AppendSql("ALTER TABLE ");
            ddlBuilder.AppendIdentifier(migrationOperation.Table);
            ddlBuilder.AppendSql(" RENAME COLUMN ");

            ddlBuilder.AppendIdentifier(migrationOperation.Name);
            ddlBuilder.AppendSql(" TO ");
            string newdropname = "Drop_" + migrationOperation.Name + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            ddlBuilder.AppendIdentifier(newdropname);
            ddlBuilder.AppendSql(";");
            ddlBuilder.AppendNewLine();


            //记录删除的列信息，以便后续的迁移操作
            ddlBuilder.AppendSql($"CREATE TABLE IF NOT EXISTS {recordtablename} (ID INTEGER CONSTRAINT {ddlBuilder.CreateConstraintName("PK", recordtablename)} PRIMARY KEY AUTOINCREMENT,TableName TEXT,OldColumn TEXT,NewColumn Text); ");

            ddlBuilder.AppendSql($" INSERT INTO {recordtablename}(TableName, OldColumn, NewColumn)");
            ddlBuilder.AppendSql($" VALUES('{SQLiteProviderManifestHelper.RemoveDbo(migrationOperation.Table)}', '{migrationOperation.Name}', '{newdropname}');");

            return ddlBuilder.GetCommandText();
        }

        private string GenerateSqlStatementConcrete(AlterColumnOperation migrationOperation)
        {
            //修改旧列列名
            SQLiteDdlBuilder ddlBuilder = new SQLiteDdlBuilder();

            ddlBuilder.AppendSql("ALTER TABLE ");
            ddlBuilder.AppendIdentifier(migrationOperation.Table);
            ddlBuilder.AppendSql(" RENAME COLUMN ");

            ddlBuilder.AppendIdentifier(migrationOperation.Column.Name);
            ddlBuilder.AppendSql(" TO ");
            string newdropname = "Alter_" + migrationOperation.Column.Name + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            ddlBuilder.AppendIdentifier(newdropname);
            ddlBuilder.AppendSql(";");
            ddlBuilder.AppendNewLine();


            //添加新数据结构的新列
            ddlBuilder.AppendSql("ALTER TABLE ");
            ddlBuilder.AppendIdentifier(migrationOperation.Table);
            ddlBuilder.AppendSql(" ADD COLUMN ");

            ColumnModel column = migrationOperation.Column;

            ddlBuilder.AppendIdentifier(column.Name);
            ddlBuilder.AppendSql(" ");
            TypeUsage storeType = ProviderManifest.GetStoreType(column.TypeUsage);
            ddlBuilder.AppendType_Addcolumn(storeType, column.IsNullable ?? true, column.IsIdentity, column.DefaultValue ?? column.ClrDefaultValue);

            //column.DefaultValue
            ddlBuilder.AppendSql(";");
            ddlBuilder.AppendNewLine();

            //记录修改的列信息，以便后续的迁移操作
            ddlBuilder.AppendSql($"CREATE TABLE IF NOT EXISTS {recordtablename} (ID INTEGER CONSTRAINT {ddlBuilder.CreateConstraintName("PK", recordtablename)} PRIMARY KEY AUTOINCREMENT,TableName TEXT,OldColumn TEXT,NewColumn Text); ");

            ddlBuilder.AppendSql($" INSERT INTO {recordtablename}(TableName, OldColumn, NewColumn)");
            ddlBuilder.AppendSql($" VALUES('{SQLiteProviderManifestHelper.RemoveDbo(migrationOperation.Table)}', '{migrationOperation.Column.Name}', '{newdropname}');");

            return ddlBuilder.GetCommandText();

            //throw new NotSupportedException("Alter column not supported by SQLite");
        }

        #endregion


        #region Foreign keys creation

        private string GenerateSqlStatementConcrete(AddForeignKeyOperation migrationOperation)
        {

            /* 
             * SQLite supports foreign key creation only during table creation
             * At least, during table creation we could try to create relationships but it
             * Requires that we sort tables in dependency order (and that there is not a circular reference
             *
             * Actually we do not create relationship at all
            */

            return "";
        }

        #endregion

        #region Primary keys creation

        private string GenerateSqlStatementConcrete(AddPrimaryKeyOperation migrationOperation)
        {
            // Actually primary key creation is supported only during table creation

            SQLiteDdlBuilder ddlBuilder = new SQLiteDdlBuilder();
            ddlBuilder.AppendSql(" PRIMARY KEY (");
            ddlBuilder.AppendIdentifierList(migrationOperation.Columns);
            ddlBuilder.AppendSql(")");
            return ddlBuilder.GetCommandText();
        }

        #endregion

        #region Table operations

        private string GenerateSqlStatementConcrete(AlterTableOperation migrationOperation)
        {
            /* 
             * SQLite does not support alter table
             * We should rename old table, create the new table, copy old data to new table and drop old table
            */

            throw new NotSupportedException("Alter column not supported by SQLite");

        }

        private string GenerateSqlStatementConcrete(CreateTableOperation migrationOperation)
        {
            SQLiteDdlBuilder ddlBuilder = new SQLiteDdlBuilder();

            ddlBuilder.AppendSql("CREATE TABLE ");
            ddlBuilder.AppendIdentifier(migrationOperation.Name);
            ddlBuilder.AppendSql(" (");
            ddlBuilder.AppendNewLine();

            bool first = true;
            string autoincrementColumnName = null;
            foreach (ColumnModel column in migrationOperation.Columns)
            {
                if (first)
                    first = false;
                else
                    ddlBuilder.AppendSql(",");

                ddlBuilder.AppendSql(" ");
                ddlBuilder.AppendIdentifier(column.Name);
                ddlBuilder.AppendSql(" ");
                if (column.IsIdentity)
                {
                    autoincrementColumnName = column.Name;
                    ddlBuilder.AppendSql(" integer constraint ");
                    ddlBuilder.AppendIdentifier(ddlBuilder.CreateConstraintName("PK", migrationOperation.Name));
                    ddlBuilder.AppendSql(" primary key autoincrement");
                    ddlBuilder.AppendNewLine();
                }
                else
                {
                    TypeUsage storeTypeUsage = ProviderManifest.GetStoreType(column.TypeUsage);
                    ddlBuilder.AppendType(storeTypeUsage, column.IsNullable ?? true, column.IsIdentity);
                    ddlBuilder.AppendNewLine();
                }

            }

            if (migrationOperation.PrimaryKey != null && autoincrementColumnName == null)
            {
                ddlBuilder.AppendSql(",");
                ddlBuilder.AppendSql(GenerateSqlStatementConcrete(migrationOperation.PrimaryKey));
            }

            ddlBuilder.AppendSql(")");

            return ddlBuilder.GetCommandText();
        }

        #endregion

        #region Index

        private string GenerateSqlStatementConcrete(CreateIndexOperation migrationOperation)
        {
            SQLiteDdlBuilder ddlBuilder = new SQLiteDdlBuilder();
            ddlBuilder.AppendSql("CREATE ");
            if (migrationOperation.IsUnique)
                ddlBuilder.AppendSql("UNIQUE ");
            ddlBuilder.AppendSql("INDEX ");
            ddlBuilder.AppendIdentifier(SQLiteProviderManifestHelper.GetFullIdentifierName(migrationOperation.Table, migrationOperation.Name));
            ddlBuilder.AppendSql(" ON ");
            ddlBuilder.AppendIdentifier(migrationOperation.Table);
            ddlBuilder.AppendSql(" (");
            ddlBuilder.AppendIdentifierList(migrationOperation.Columns);
            ddlBuilder.AppendSql(")");

            return ddlBuilder.GetCommandText();
        }

        #endregion

        #region Drop

        private string GenerateSqlStatementConcrete(DropForeignKeyOperation migrationOperation)
        {
            return string.Empty;
            //sqlite 不支持
            //SQLiteDdlBuilder ddlBuilder = new SQLiteDdlBuilder();
            //ddlBuilder.AppendSql("ALTER TABLE ");
            //ddlBuilder.AppendIdentifier(migrationOperation.PrincipalTable);
            //ddlBuilder.AppendSql(" DROP CONSTRAINT ");
            //ddlBuilder.AppendIdentifier(migrationOperation.Name);
            //ddlBuilder.AppendNewLine();
            //return ddlBuilder.GetCommandText();

        }

        private string GenerateSqlStatementConcrete(DropPrimaryKeyOperation migrationOperation)
        {
            return string.Empty;
            //sqlite 不支持
            //SQLiteDdlBuilder ddlBuilder = new SQLiteDdlBuilder();
            //ddlBuilder.AppendSql("ALTER TABLE ");
            //ddlBuilder.AppendIdentifier(migrationOperation.Table);
            //ddlBuilder.AppendSql(" DROP CONSTRAINT ");
            //ddlBuilder.AppendIdentifier(migrationOperation.Name);
            //ddlBuilder.AppendNewLine();
            //return ddlBuilder.GetCommandText();
        }

        private string GenerateSqlStatementConcrete(DropIndexOperation migrationOperation)
        {
            SQLiteDdlBuilder ddlBuilder = new SQLiteDdlBuilder();
            ddlBuilder.AppendSql("DROP INDEX if exists");
            ddlBuilder.AppendIdentifier(SQLiteProviderManifestHelper.GetFullIdentifierName(migrationOperation.Table, migrationOperation.Name));
            //ddlBuilder.AppendSql(" ON ");
            //ddlBuilder.AppendIdentifier(migrationOperation.Table);
            ddlBuilder.AppendNewLine();
            return ddlBuilder.GetCommandText();
        }

        private string GenerateSqlStatementConcrete(DropTableOperation migrationOperation)
        {
            SQLiteDdlBuilder ddlBuilder = new SQLiteDdlBuilder();
            ddlBuilder.AppendSql("DROP TABLE ");
            ddlBuilder.AppendIdentifier(migrationOperation.Name);
            ddlBuilder.AppendNewLine();
            return ddlBuilder.GetCommandText();
        }

        #endregion

    }
}
