//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StockMonitor
{
    using System;
    using System.Collections.Generic;
    
    public partial class User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User()
        {
            this.HoldingShares = new HashSet<HoldingShare>();
            this.TradingRecords = new HashSet<TradingRecord>();
            this.WatchLists = new HashSet<WatchList>();
            this.WatchListItems = new HashSet<WatchListItem>();
        }
    
        public int Id { get; set; }
        public string AccountName { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string Password { get; set; }
        public System.DateTime CreateDate { get; set; }
        public double Balance { get; set; }
        public byte[] Photo { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<HoldingShare> HoldingShares { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TradingRecord> TradingRecords { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WatchList> WatchLists { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WatchListItem> WatchListItems { get; set; }
    }
}
