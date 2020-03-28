using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMonitor.Models.JSONModels
{
    public class FmgCompanyProfile
    {
        public double Price { get; set; }
        public string Beta { get; set; }
        public string VolAvg { get; set; }
        public string MktCap { get; set; }
        public string LastDiv { get; set; }
        public string Range { get; set; }
        public double Changes { get; set; }
        public string ChangesPercentage { get; set; }
        public string CompanyName { get; set; }
        public string Exchange { get; set; }
        public string Industry { get; set; }
        public string Website { get; set; }
        public string Description { get; set; }
        public string Ceo { get; set; }
        public string Sector { get; set; }
        public string ImagePath { get; set; }

        public override string ToString()
        {
            return $"FmgCompanyProfile: {Price},{Industry},{Description}";
        }
    }
}
