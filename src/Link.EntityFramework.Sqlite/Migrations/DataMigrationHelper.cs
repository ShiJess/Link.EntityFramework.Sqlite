using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;

namespace Link.EntityFramework.Sqlite.Migrations
{
    /// <summary>
    /// 数据迁移辅助类
    /// </summary>
    public sealed class DataMigrationHelper
    {
        /// <summary>
        /// 删除列
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="targetTableName"></param>
        /// <param name="dropColumnList"></param>
        public static void DropColumn(DbConnection connection, string targetTableName, List<string> dropColumnList)
        {
            var createtablesql = $"select sql from sqlite_master where type='table' and name='{targetTableName}'";
            var createtablecommand = connection.CreateCommand();
            createtablecommand.CommandText = createtablesql;
            var createsql = createtablecommand.ExecuteScalar()?.ToString();
            if (string.IsNullOrWhiteSpace(createsql))
            {
                //无对应表
                return;
            }
            createsql = createsql.Replace('[', '"').Replace(']', '"');//某些情况下语句中的表名、列名以[]框选

            var tableindex = createsql.IndexOf($"\"{targetTableName}\"");//create 前半段语句

            foreach (var columnName in dropColumnList)
            {
                var index = createsql.IndexOf($"\"{columnName}\"");//待删除字段起始位置
                var preindex = createsql.Substring(0, index + 1).LastIndexOf(',');//待删除字段前一个逗号位置
                if (preindex == -1)//第一个字段
                {
                    preindex = createsql.IndexOf('(') + 1;
                }
                var nextindex = createsql.IndexOf(',', index);//待删除字段结束位置
                if (nextindex == -1)//最后一个字段
                {
                    nextindex = createsql.LastIndexOf(')');
                }
                createsql = createsql.Remove(preindex, nextindex - preindex);
            }

            //最终表创建语句
            //var targetsql = createsql;
            var targetsql = createsql.Substring(0, tableindex) + "\"Temp_" + createsql.Substring(tableindex + 1, createsql.Length - (tableindex + 1));

            //删除存在的同名临时表
            var dropandrenamesql = $"DROP TABLE IF EXISTS Temp_{targetTableName};";
            var droptemptablecommand = connection.CreateCommand();
            droptemptablecommand.CommandText = dropandrenamesql;
            droptemptablecommand.ExecuteNonQuery();

            //创建临时存储表
            var createtemptablecommand = connection.CreateCommand();
            createtemptablecommand.CommandText = targetsql;
            createtemptablecommand.ExecuteNonQuery();

            var tableinfo = $"pragma table_info('Temp_{targetTableName}')";
            var newtablecommand = connection.CreateCommand();
            newtablecommand.CommandText = tableinfo;
            SQLiteDataAdapter sqlDataAdapter3 = new SQLiteDataAdapter(newtablecommand as SQLiteCommand);
            DataTable dt_NewTable = new DataTable();
            sqlDataAdapter3.Fill(dt_NewTable);

            string columns = "";
            foreach (DataRow newcolumn in dt_NewTable.Rows)
            {
                columns += newcolumn["name"]?.ToString();
                columns += ",";
            }
            columns = columns.Trim(',');

            //迁移数据
            var copydatasql = $"INSERT INTO Temp_{targetTableName} ({columns}) SELECT {columns} FROM {targetTableName}; ";
            var copydatacommand = connection.CreateCommand();
            copydatacommand.CommandText = copydatasql;
            copydatacommand.ExecuteNonQuery();

            //drop old & rename new
            var dropandrename = $"DROP TABLE {targetTableName};";
            dropandrename += $"ALTER TABLE Temp_{targetTableName} RENAME TO {targetTableName};";
            var dropandrenamecommand = connection.CreateCommand();
            dropandrenamecommand.CommandText = dropandrename;
            dropandrenamecommand.ExecuteNonQuery();
        }

        /// <summary>
        /// 更改列信息
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="targetTableName"></param>
        /// <param name="alterColumnDic"></param>
        public static void AlterColumn(DbConnection connection, string targetTableName, Dictionary<string, string> alterColumnDic)
        {
            var createtablesql = $"select sql from sqlite_master where type='table' and name='{targetTableName}'";
            var createtablecommand = connection.CreateCommand();
            createtablecommand.CommandText = createtablesql;
            var createsql = createtablecommand.ExecuteScalar()?.ToString();
            if (string.IsNullOrWhiteSpace(createsql))
            {
                //无对应表
                return;
            }

            var tableindex = createsql.IndexOf($"\"{targetTableName}\"");//create 前半段语句
            foreach (var columnName in alterColumnDic.Values)
            {
                var index = createsql.IndexOf($"\"{columnName}\"");//待删除字段起始位置
                var preindex = createsql.Substring(0, index + 1).LastIndexOf(',');//待删除字段前一个逗号位置
                if (preindex == -1)//第一个字段
                {
                    preindex = createsql.IndexOf('(') + 1;
                }
                var nextindex = createsql.IndexOf(',', index);//待删除字段结束位置
                if (nextindex == -1)//最后一个字段
                {
                    nextindex = createsql.LastIndexOf(')');
                }
                createsql = createsql.Remove(preindex, nextindex - preindex);
            }

            //最终表创建语句
            var targetsql = createsql.Substring(0, tableindex) + "\"Temp_" + createsql.Substring(tableindex + 1, createsql.Length - (tableindex + 1));

            //删除存在的同名临时表
            var dropandrenamesql = $"DROP TABLE IF EXISTS Temp_{targetTableName};";
            var droptemptablecommand = connection.CreateCommand();
            droptemptablecommand.CommandText = dropandrenamesql;
            droptemptablecommand.ExecuteNonQuery();

            //创建临时存储表
            var createtemptablecommand = connection.CreateCommand();
            createtemptablecommand.CommandText = targetsql;
            createtemptablecommand.ExecuteNonQuery();

            var tableinfo = $"pragma table_info('Temp_{targetTableName}')";
            var newtablecommand = connection.CreateCommand();
            newtablecommand.CommandText = tableinfo;
            SQLiteDataAdapter sqlDataAdapter3 = new SQLiteDataAdapter(newtablecommand as SQLiteCommand);
            DataTable dt_NewTable = new DataTable();
            sqlDataAdapter3.Fill(dt_NewTable);

            string columns = "";
            string pkcolumn = "ID";//表主键——用于后面列数据更新时，关联
            foreach (DataRow newcolumn in dt_NewTable.Rows)
            {
                string targetcolumn = newcolumn["name"]?.ToString();
                if (newcolumn["pk"]?.ToString() == "1")
                {
                    pkcolumn = targetcolumn;
                }
                //先迁移未更改列的数据
                if (alterColumnDic.ContainsKey(targetcolumn))
                {
                    continue;
                }
                columns += targetcolumn;
                columns += ",";
            }
            columns = columns.Trim(',');

            //迁移未更改列的数据
            var copydatasql = $"INSERT INTO Temp_{targetTableName} ({columns}) SELECT {columns} FROM {targetTableName}; ";
            var copydatacommand = connection.CreateCommand();
            copydatacommand.CommandText = copydatasql;
            copydatacommand.ExecuteNonQuery();

            //迁移更改列的数据——一列一列迁移，可以保证如果列数据不兼容，则只丢失更改列的数据
            foreach (var column in alterColumnDic)
            {
                try
                {
                    var updatedatasql = $"UPDATE Temp_{targetTableName} set {column.Key}=(select {column.Value} from {targetTableName} where Temp_{targetTableName}.{pkcolumn}={targetTableName}.{pkcolumn});";
                    var updatedatacommand = connection.CreateCommand();
                    updatedatacommand.CommandText = updatedatasql;
                    updatedatacommand.ExecuteNonQuery();
                }
                catch { }
            }

            //drop old & rename new
            var dropandrename = $"DROP TABLE {targetTableName};";
            dropandrename += $"ALTER TABLE Temp_{targetTableName} RENAME TO {targetTableName};";
            var dropandrenamecommand = connection.CreateCommand();
            dropandrenamecommand.CommandText = dropandrename;
            dropandrenamecommand.ExecuteNonQuery();
        }

    }
}