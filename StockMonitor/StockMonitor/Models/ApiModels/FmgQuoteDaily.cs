using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMonitor.Models.JSONModels
{
    public class FmgCandleDaily
    {
        public DateTime Date { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double AdjClose { get; set; }
        public string Volume { get; set; }  //Todo, parse string to long
        public double UnadjustedVolume { get; set; }
        public double Change { get; set; }
        public double ChangePercent { get; set; }
        public double Vwap { get; set; }
        public string Label { get; set; }
        public double ChangeOverTime { get; set; }

        public override string ToString()
        {
            return
                $"FmgCandleDaily: {Date},{Open},{High},{Low},{Close},{Volume},{UnadjustedVolume},{Change},{ChangePercent},{Vwap},{Label},{ChangeOverTime}";
        }
    }
}
