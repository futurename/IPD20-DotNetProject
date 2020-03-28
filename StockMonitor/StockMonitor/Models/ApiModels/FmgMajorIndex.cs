using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMonitor.Models.JSONModels
{
    public class FmgMajorIndex
    {
        public string Ticker { get; set; }
        public double Changes { get; set; }
        public double Price { get; set; }
        public string IndexName { get; set; }

        public override string ToString()
        {
            return $"FmgMajorIndex: {IndexName}:{Price},{Changes},{Ticker}";
        }
    }
}
