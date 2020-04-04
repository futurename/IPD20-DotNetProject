using LiveCharts.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMonitor.Models.ApiModels
{
    public class Fmg1MinQuote : OhlcPoint
    {
        public DateTime Date { get; set; }
        public long Volume { get; set; }

        public override string ToString()
        {
            return $"Fmg1MinQuote: {Date}: {Open},{Low},{High},{Low},{Close},{Volume}";
        }
    }
}
