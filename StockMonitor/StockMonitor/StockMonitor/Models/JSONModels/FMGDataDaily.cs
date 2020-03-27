using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMonitor.Models.JSONModels
{
    public class FmgDataDaily
    {
        public string Symbol { get; set; }
        public List<FmgCandleDaily> DailyDataList { get; set; }
        public override string ToString()
        {
            return $"{Symbol}\n" + DailyDataList.Select(p=>p.ToString() + "\n");
        }
    }


}
