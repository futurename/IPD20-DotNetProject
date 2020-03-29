using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMonitor.Models.JSONModels
{
    public class FinnCompanyProfile
    {
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Currency { get; set; }
        public string Cusip { get; set; }
        public string Sedol { get; set; }
        public string Description { get; set; }
        public int EmployeeTotal { get; set; }
        public string Exchange { get; set; }
        public string Ggroup { get; set; }
        public string Gind { get; set; }
        public string Gsector { get; set; }
        public string GsubInd { get; set; }
        public DateTime IpoDate { get; set; }
        public string Isin { get; set; }
        public string MarketCapitalization { get; set; }
        public string Naics { get; set; }
        public string NaicsNationalIndustry { get; set; }
        public string NaicsSector { get; set; }
        public string NaicsSubSector { get; set; }
        public string Name { get; set;}
        public string Phone { get; set; }
        public double ShareOutstanding { get; set; }
        public string State { get; set; }
        public string Symbol { get; set; }
        public string WebUrl { get; set; }

        public override string ToString()
        {
            return $"FinnCompanyProfile: {Symbol},{Description}";
        }
    }
}
