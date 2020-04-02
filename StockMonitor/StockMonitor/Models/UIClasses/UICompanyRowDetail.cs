using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMonitor.Models.UIClasses
{
    public class UICompanyRowDetail
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public long Volume { get; set; }
        public double Change { get; set; }
        public double ChangePercentage { get; set; }
        public string Description { get; set; }
        public string Ceo { get; set; }
        public string Industry { get; set; }
       public string Sector { get; set; }
       public override string ToString()
       {
           return $"UIcompanyRowDetail: {Symbol}: {Name}, {Open}, {High}, {Industry}";
       }
    }
}
