using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.EntityFramework.Sqlite.Migrations.Record
{
    /// <summary>
    /// 迁移记录数据结构
    /// </summary>
    public class RecordInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 旧列名 or 最终目标列名
        /// </summary>
        public string OldColumn { get; set; }

        /// <summary>
        /// 临时标记列名
        /// </summary>
        public string NewColumn { get; set; }
    }
}
