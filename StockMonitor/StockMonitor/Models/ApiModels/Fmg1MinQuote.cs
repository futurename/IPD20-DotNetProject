using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMonitor.Models.ApiModels
{
    public class Fmg1MinQuote
    {
        public DateTime Date { get; set; }
        public double Open { get; set; }
        public double Low { get; set; }
        public double High { get; set; }
        public double Close { get; set; }
        public int Volume { get; set; }

        public override string ToString()
        {
            return $"{Date}: {Open},{Low},{High},{Low},{Close}";
        }
    }
}
