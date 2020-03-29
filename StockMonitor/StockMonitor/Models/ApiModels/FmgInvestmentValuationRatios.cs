using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockMonitor.Models.ApiModels
{
    public class FmgInvestmentValuationRatios
    {
        public double PriceBookValueRatio { get; set; }
        public double PriceToBookRatio { get; set; }
        public double PriceToSalesRatio { get; set; }
        public double PriceEarningsRatio { get; set; }
        public double ReceivablesTurnover { get; set; }
        public double PriceToFreeCashFlowsRatio { get; set; }
        public double PriceToOperatingCashFlowsRatio { get; set; }
        public double PriceCashFlowRatio { get; set; }
        public double PriceEarningsToGrowthRatio { get; set; }
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
