using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMonitor.Models.ApiModels
{
    public class FmgInvestmentValuationRatios
    {
        public string PriceBookValueRatio { get; set; }
        public string PriceToBookRatio { get; set; }
        public string PriceToSalesRatio { get; set; }
        public string PriceEarningsRatio { get; set; }
        public string ReceivablesTurnover { get; set; }
        public string PriceToFreeCashFlowsRatio { get; set; }
        public string PriceToOperatingCashFlowsRatio { get; set; }
        public string PriceCashFlowRatio { get; set; }
        public string PriceEarningsToGrowthRatio { get; set; }
        public string PriceSalesRatio { get; set; }
        public string DividendYield { get; set; }
        public string EnterpriseValueMultiple { get; set; }
        public string PriceFairValue { get; set; }

        public override string ToString()
        {
            return $"FMGInvesValRations: PB:{PriceToBookRatio}, PE:{PriceEarningsRatio}, PS:{PriceSalesRatio}";
        }
    }
}
