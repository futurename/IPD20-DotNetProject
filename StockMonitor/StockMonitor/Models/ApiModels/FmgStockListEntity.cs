using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMonitor.Models.ApiModels
{
    public class FmgStockListEntity
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Exchange { get; set; }
        public override string ToString()
        {
            return $"FmgStockEntity: {Symbol},{Name},{Price},{Exchange}";
        }
    }
}
