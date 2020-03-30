using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMonitor.Models.POCO
{
    public class CompanyDataRow
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public double Price { get; set; }
        public double ChangePercentage { get; set; }
        public double PriceChange { get; set; }
        public string MarketCapital { get; set; }
        public string PriceToEarningRatio { get; set; }
        public string PriceToSalesRatio { get; set; }
        public string Industry { get; set; }
        public string Sector { get; set; }


        public override string ToString()
        {
            return $"{Symbol}:{Price}, {ChangePercentage}%,{PriceChange},{Sector}";
        }
    }
}
