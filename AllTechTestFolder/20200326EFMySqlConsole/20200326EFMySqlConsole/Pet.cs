namespace _20200326EFMySqlConsole
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("test1WeiWang.Pets")]
    public partial class Pet
    {
        public int Id { get; set; }

        public int OwnerId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Column(TypeName = "date")]
        public DateTime BoughtDate { get; set; }

        public virtual Person Person { get; set; }
    }
}
