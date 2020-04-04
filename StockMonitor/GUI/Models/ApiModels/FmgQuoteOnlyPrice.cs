using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMonitor.Models.ApiModels
{
    public class FmgQuoteOnlyPrice
    {
        public string Symbol { get; set; }
        public double Price { get; set; }

        public override string ToString()
        {
            return $"{Symbol}:{Price}";
        }
    }
}
