﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using GUI;

namespace StockMonitor
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DbStockMonitor : DbContext
    {
        public DbStockMonitor()
            : base("name=StockMonitorEntities")
        {

        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Company> Companies { get; set; }
        public virtual DbSet<HoldingShare> HoldingShares { get; set; }
        public virtual DbSet<Quote1Min> Quote1Min { get; set; }
        public virtual DbSet<QuoteDaily> QuoteDailies { get; set; }
        public virtual DbSet<TestQuoteDaily> TestQuoteDailies { get; set; }
        public virtual DbSet<TradingRecord> TradingRecords { get; set; }
        public virtual DbSet<WatchListItem> WatchListItems { get; set; }
        public virtual DbSet<User> Users { get; set; }
    }
}
