using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _20200326TestDatabase
{
    [Table("test1WeiWang.Quotes")]
    public class Quote
    {
        [Key]
        public int Id { set; get; }

        [Required]
        [MaxLength(50)]
        public string symbol { get; set; }

        [Required]
        public double price { get; set; }

        public override string ToString()
        {
            return $"{Id}-{symbol}:{price}";
        }
    }
}
