using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMonitor.Models.JSONModels
{
    public class FinnQuote
    {
        public double C { get; set;} //current price
        public double H { get; set; } //high price
        public double L { get; set; } //low price
        public double O { get; set; } //open price
        public double Pc { get; set; } //previous close price
        public string T { get; set; } //time stamp of Unix format

        public override string ToString()
        {
            return $"FinnQuote- CurPrice: {C}, High: {H}, Low: {L}, Timestamp: {T}";
        }
    }
}
