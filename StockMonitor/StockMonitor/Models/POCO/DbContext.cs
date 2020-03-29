using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMonitor.Models.POCO
{
    public class DbContext:System.Data.Entity.DbContext
    {
        public virtual DbSet<Company> CompanySet { get; set; }
        public virtual DbSet<HoldingShare> HoldingShareSet { get; set; }
        public virtual DbSet<Quote1Min> Quote1MinSet { get; set; }
        public virtual DbSet<QuoteDaily> QuoteDailySet { get; set; }
        public virtual DbSet<TradingRecord> TradingRecordSet { get; set; }

    }
}
