using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMonitor.Models.POCO
{
    public class CompanyDataRow
    {
        public string Symbol { get; set; }
        public double Price { get; set; }
        public double ChangePercentage { get; set; }
        public double Change { get; set; }
        public string MarketCapital { get; set; }
        public int EmployeesTotal { get; set; }
        public string Sector { get; set; }

        public override string ToString()
        {
            return $"{Symbol}:{Price}, {ChangePercentage}%,{Change},{Sector}";
        }
    }
}
