using Link.EntityFramework.Sqlite.Migrations.Record;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Link.EntityFramework.Sqlite
{
    /// <summary>
    /// Migrate Database To LatestVersion Extention
    /// * Sqlite Support Drop Column
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <typeparam name="TMigrationsConfiguration"></typeparam>
    public class MigrateDatabaseToLatestVersionExtention<TContext, TMigrationsConfiguration> : MigrateDatabaseToLatestVersion<TContext, TMigrationsConfiguration>
          where TContext : DbContext
          where TMigrationsConfiguration : DbMigrationsConfiguration<TContext>, new()
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="useSuppliedContext"></param>
        public MigrateDatabaseToLatestVersionExtention(bool useSuppliedContext) : base(useSuppliedContext)
        {

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
                    //pragma table_info('A_New')
                    var createtablesql = "select name,sql from sqlite_master where type='table'";
                    var createtablecommand = connection.CreateCommand();
                    createtablecommand.CommandText = createtablesql;
                    SQLiteDataAdapter sqlDataAdapter2 = new SQLiteDataAdapter(createtablecommand as SQLiteCommand);
                    DataTable dt_TableInfo = new DataTable();
                    sqlDataAdapter2.Fill(dt_TableInfo);

                    foreach (DataRow record in dt_Record.Rows)
                    {
                        var createsql = from c in dt_TableInfo.Rows.Cast<DataRow>().ToList() where c["name"].ToString() == record["TableName"].ToString() select c["sql"];

                        if (createsql != null && createsql.Count() > 0)
                        {
                            var sql = createsql.First().ToString();
                            var tableindex = sql.IndexOf("\"" + record["TableName"].ToString() + "\"");
                            var index = sql.IndexOf("\"" + record["NewColumn"].ToString() + "\"");
                            var preindex = sql.Substring(0, index + 1).LastIndexOf(',');
                            var nextindex = sql.IndexOf(',', index);
                            if (nextindex == -1)//最后一个字段
                            {
                                nextindex = sql.LastIndexOf(')');
                            }
                            var targetsql = sql.Substring(0, tableindex) + "\"Temp_" + sql.Substring(tableindex + 1, preindex - (tableindex + 1)) + sql.Substring(nextindex, sql.Length - nextindex);
                            //创建临时存储表
                            var createtemptablecommand = connection.CreateCommand();
                            createtemptablecommand.CommandText = targetsql;
                            createtemptablecommand.ExecuteNonQuery();

                            var newtable = $"select * from Temp_{record["TableName"].ToString()}";
                            var newtablecommand = connection.CreateCommand();
                            newtablecommand.CommandText = newtable;
                            SQLiteDataAdapter sqlDataAdapter3 = new SQLiteDataAdapter(newtablecommand as SQLiteCommand);
                            DataTable dt_NewTable = new DataTable();
                            sqlDataAdapter3.Fill(dt_NewTable);

                            string columns = "";
                            string targetcolumns = "";
                            foreach (DataColumn newcolumn in dt_NewTable.Columns)
                            {
                                columns += newcolumn.ColumnName;
                                columns += ",";
                                if (record["OldColumn"].ToString() == newcolumn.ColumnName)
                                {
                                    targetcolumns += record["NewColumn"].ToString() + " as " + newcolumn.ColumnName;
                                }
                                else
                                {
                                    targetcolumns += newcolumn.ColumnName;
                                }
                                targetcolumns += ",";
                            }
                            columns = columns.Trim(',');
                            targetcolumns = targetcolumns.Trim(',');

                            //迁移数据
                            var copydatasql = $"INSERT INTO Temp_{record["TableName"].ToString()} ({columns}) SELECT {targetcolumns} FROM {record["TableName"].ToString()}; ";
                            var copydatacommand = connection.CreateCommand();
                            copydatacommand.CommandText = copydatasql;
                            copydatacommand.ExecuteNonQuery();

                            //drop old & rename new
                            var dropandrename = $"DROP TABLE {record["TableName"].ToString()};";
                            dropandrename += $"ALTER TABLE Temp_{record["TableName"].ToString()} RENAME TO {record["TableName"].ToString()};";
                            var dropandrenamecommand = connection.CreateCommand();
                            dropandrenamecommand.CommandText = dropandrename;
                            dropandrenamecommand.ExecuteNonQuery();
                        }
                    }

                    //Regex regex = new Regex("");
                    ////todo 优化，提供删除旧版Drop_Column_yyyyMMddHHmmSS列支持
                    //foreach (DataRow table in dt_TableInfo.Rows)
                    //{
                    //    var tableinfo = $"pragma table_info('{table["name"]}')";
                    //    var tableinfocommand = connection.CreateCommand();
                    //    tableinfocommand.CommandText = tableinfo;
                    //    SQLiteDataAdapter sqlDataAdapter4 = new SQLiteDataAdapter(tableinfocommand as SQLiteCommand);
                    //    DataTable dt_TableInfo1 = new DataTable();
                    //    sqlDataAdapter4.Fill(dt_TableInfo1);

                    //    foreach (DataRow oldcolumn in dt_TableInfo1.Rows)
                    //    {

                    //        //匹配Drop_Column_
                    //        if (regex.IsMatch(oldcolumn["name"].ToString()))
                    //        {

                    //        }
                    //    }
                    //}

                }

                //若记录信息清空
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

            }
            else { base.InitializeDatabase(context); }
        }
    }
}
