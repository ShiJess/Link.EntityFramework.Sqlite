using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Link.EntityFramework.Sqlite.Test.DataModel
{
    [Table("A")]
    internal class A
    {
        public int ID { get; set; }

        public string Name { get; set; }

        [Column("Age")]
        //[Column("AgeNew")]
        public string Age { get; set; }

        [StringLength(100)]
        public string Alias { get; set; }

    }
}
