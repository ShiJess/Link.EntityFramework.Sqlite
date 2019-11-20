using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Link.EntityFramework.Sqlite.Test.DataModel
{

    [Table("A")]
    internal class A
    {
        public int ID { get; set; }

        /// <summary>
        /// 创建时间 —— 仅供数据库存储
        /// </summary>
        [Column("CreateDateTime")]
        public string CreateDateTimeStr { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        /// <summary>
        /// 创建时间
        /// </summary>
        [NotMapped]
        public DateTime CreateDateTime
        {
            get
            {
                DateTime dateTime = DateTime.Now;
                DateTime.TryParse(CreateDateTimeStr, out dateTime);
                return dateTime;
            }
            set
            {
                CreateDateTimeStr = value.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        [StringLength(50)]
        public string Name { get; set; }

        [MaxLength(50)]
        [Column("Age")]
        //[Column("AgeNew")]
        public string Age { get; set; }

        [StringLength(100)]
        public string Alias { get; set; }

        [MaxLength(50)]
        public string TestDrop { get; set; }

        public bool TestBool { get; set; }

    }

}