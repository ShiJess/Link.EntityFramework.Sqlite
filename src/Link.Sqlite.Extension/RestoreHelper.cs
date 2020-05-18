using System;

namespace Link.Sqlite.Extension
{
    /// <summary>
    /// sqlite内容恢复辅助方法-sqlite还原
    /// </summary>
    public sealed class RestoreHelper
    {
        /// <summary>
        /// 从已有数据库还原数据至目标数据库
        /// </summary>
        /// <param name="sourcedb"></param>
        /// <param name="targetdb"></param>
        public static void RestoreFromDB(string sourcedb, string targetdb)
        {

        }

        public static void RestoreFromSQL()
        {

        }

        public static void RestoreFromCSV()
        {

        }
    }

    /// <summary>
    /// 还原模式
    /// </summary>
    public enum RestoreMode
    {
        /// <summary>
        /// 清除目标数据库数据后，还原内容
        /// </summary>
        ClearAdd,
        /// <summary>
        /// 更新不一样的字段内容
        /// </summary>
        UpdateAdd,
    }

}