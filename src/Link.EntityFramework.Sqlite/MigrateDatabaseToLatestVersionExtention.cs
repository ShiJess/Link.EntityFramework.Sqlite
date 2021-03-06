﻿using Link.EntityFramework.Sqlite.Migrations;
using Link.EntityFramework.Sqlite.Migrations.Record;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.Sqlite.Utilities;
using System.Data.SQLite;
using System.Data.SQLite.EF6.Migrations;
using System.Linq;
using System.Text.RegularExpressions;

namespace Link.EntityFramework.Sqlite
{
    /// <summary>
    /// Migrate Database To LatestVersion Extention
    /// * Sqlite Support Drop Column
    /// * Sqlite Support Alter Column
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TMigrationsConfiguration"></typeparam>
    public class MigrateDatabaseToLatestVersionExtention<TContext, TMigrationsConfiguration> : MigrateDatabaseToLatestVersion<TContext, TMigrationsConfiguration>
          where TContext : DbContext
          where TMigrationsConfiguration : DbMigrationsConfiguration<TContext>, new()
    {
        /// <summary>
        /// 清理旧版本迁移[or 迁移失败]导致的残留内容
        /// e.g. Drop_ColumnName_yyyyMMddHHmmSS
        /// </summary>
        private bool ClearOldVersionMigration { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="useSuppliedContext"></param>
        /// <param name="clearOldVersion"></param>
        public MigrateDatabaseToLatestVersionExtention(bool useSuppliedContext, bool clearOldVersion = false)
            : this(useSuppliedContext, new TMigrationsConfiguration(), clearOldVersion)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="useSuppliedContext"></param>
        /// <param name="configuration"></param>
        /// <param name="clearOldVersion"></param>
        public MigrateDatabaseToLatestVersionExtention(bool useSuppliedContext, TMigrationsConfiguration configuration, bool clearOldVersion = false)
            : base(useSuppliedContext, configuration)
        {
            Check.NotNull(configuration, "configuration");

            ClearOldVersionMigration = clearOldVersion;
            configuration.SetSqlGenerator("System.Data.SQLite", new SQLiteMigrationSqlGenerator(true));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void InitializeDatabase(TContext context)
        {
            if (context != null && context.Database != null && context.Database.Connection is SQLiteConnection)
            {
                string recordtablename = RecordContext.DefaultTableName;

                bool needclose = false;
                var connection = context.Database.Connection;

                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    needclose = true;
                    connection.Open();
                }

                var createrecordtable = $"CREATE TABLE IF NOT EXISTS {recordtablename}(ID INTEGER CONSTRAINT PK_A_{DateTime.Now.Ticks} PRIMARY KEY AUTOINCREMENT,TableName TEXT,OldColumn TEXT,NewColumn Text); ";
                var createcommand = connection.CreateCommand();
                createcommand.CommandText = createrecordtable;
                createcommand.ExecuteNonQuery();

                //若存在旧数据，则清空
                var clearcommand = connection.CreateCommand();
                clearcommand.CommandText = $"DELETE FROM {recordtablename};";
                clearcommand.ExecuteNonQuery();

                if (needclose)
                {
                    connection.Close();
                    needclose = false;
                }

                base.InitializeDatabase(context);

                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    needclose = true;
                    connection.Open();
                }

                var recordinfo = $"select * from {recordtablename};";
                var recordinfocommand = connection.CreateCommand();
                recordinfocommand.CommandText = recordinfo;
                SQLiteDataAdapter sqlDataAdapter1 = new SQLiteDataAdapter(recordinfocommand as SQLiteCommand);
                DataTable dt_Record = new DataTable();
                sqlDataAdapter1.Fill(dt_Record);

                if (dt_Record != null && dt_Record.Rows.Count > 0)
                {
                    List<RecordInfo> recordinfos = new List<RecordInfo>();
                    foreach (DataRow record in dt_Record.Rows)
                    {
                        recordinfos.Add(new RecordInfo
                        {
                            //ID=record["ID"]?.ToString(),
                            TableName = record["TableName"]?.ToString(),
                            OldColumn = record["OldColumn"]?.ToString(),
                            NewColumn = record["NewColumn"]?.ToString()
                        });
                    }
                    var migraTables = recordinfos.GroupBy(p => p.TableName);
                    foreach (var table in migraTables)
                    {
                        var tableinfo = $"pragma table_info('{table.Key}')";
                        var tableinfocommand = connection.CreateCommand();
                        tableinfocommand.CommandText = tableinfo;
                        SQLiteDataAdapter sqlDataAdapter4 = new SQLiteDataAdapter(tableinfocommand as SQLiteCommand);
                        DataTable dt_TableInfo1 = new DataTable();
                        sqlDataAdapter4.Fill(dt_TableInfo1);
                        List<string> tablecolumn = (from c in dt_TableInfo1.Rows.Cast<DataRow>() select c["name"]?.ToString()).ToList();

                        List<string> dropcolumns = (from c in table
                                                    where !tablecolumn.Contains(c.OldColumn)
                                                    select c.NewColumn).ToList();
                        Dictionary<string, string> altercolumns = (from c in table
                                                                   where tablecolumn.Contains(c.OldColumn)
                                                                   select c).ToDictionary(p => p.OldColumn, p => p.NewColumn);
                        //删除列处理
                        if (ClearOldVersionMigration)
                        {
                            Regex regex = new Regex(@"Drop_\w*_[0-9]{14}");
                            var droplist = from c in tablecolumn
                                           where regex.IsMatch(c)
                                           select c;
                            dropcolumns = dropcolumns.Union(droplist).ToList();
                        }
                        DataMigrationHelper.DropColumn(connection, table.Key, dropcolumns);
                        //修改列处理
                        DataMigrationHelper.AlterColumn(connection, table.Key, altercolumns);
                        if (ClearOldVersionMigration)
                        {
                            //清理旧版列更改数据
                            Regex regex = new Regex(@"Alter_\w*_[0-9]{14}");
                            var alterlist = from c in tablecolumn
                                            where regex.IsMatch(c)
                                            select c;
                            List<string> dropaltercolumns = alterlist.Except(altercolumns.Values).ToList();
                            DataMigrationHelper.DropColumn(connection, table.Key, dropaltercolumns);
                        }
                    }
                }

                //当标记清理历史版本迁移信息时，过滤一遍所有context下的表，删除所有残留多余字段
                if (ClearOldVersionMigration)
                {
                    var entities = (context as IObjectContextAdapter).ObjectContext.MetadataWorkspace.GetItems<EntityContainer>(DataSpace.SSpace);
                    foreach (var item in entities)
                    {
                        foreach (var subitem in item.EntitySets)
                        {
                            var tablename = subitem.Table;

                            var tableinfo = $"pragma table_info('{tablename}')";
                            var tableinfocommand = connection.CreateCommand();
                            tableinfocommand.CommandText = tableinfo;
                            SQLiteDataAdapter sqlDataAdapter4 = new SQLiteDataAdapter(tableinfocommand as SQLiteCommand);
                            DataTable dt_TableInfo1 = new DataTable();
                            sqlDataAdapter4.Fill(dt_TableInfo1);
                            List<string> tablecolumn = (from c in dt_TableInfo1.Rows.Cast<DataRow>() select c["name"]?.ToString()).ToList();

                            Regex regex = new Regex(@"Drop_\w*_[0-9]{14}");
                            Regex regex1 = new Regex(@"Alter_\w*_[0-9]{14}");
                            var droplist = (from c in tablecolumn
                                            where regex.IsMatch(c) || regex1.IsMatch(c)
                                            select c).ToList();

                            if (droplist.Count() > 0)
                            {
                                //todo 迁移数据
                                //DataMigrationHelper.DropColumn(connection, tablename, droplist);
                            }
                        }
                    }
                }

                //记录信息清空
                var clearcommand1 = connection.CreateCommand();
                clearcommand1.CommandText = $"DELETE FROM {recordtablename};";
                clearcommand1.ExecuteNonQuery();

                var droprecordtable = $"DROP TABLE IF EXISTS {recordtablename};";
                var dropcommand = connection.CreateCommand();
                dropcommand.CommandText = droprecordtable;
                dropcommand.ExecuteNonQuery();

                if (needclose)
                {
                    connection.Close();
                    needclose = false;
                }

                //todo 非空字段的数据中，空数据设置默认值

            }
            else { base.InitializeDatabase(context); }
        }
    }
}
