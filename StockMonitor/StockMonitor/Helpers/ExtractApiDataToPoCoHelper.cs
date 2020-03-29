using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMonitor.Models.ApiModels;
using StockMonitor.Models.JSONModels;
using StockMonitor.Models.POCO;

namespace StockMonitor.Helpers
{
    public static class ExtractApiDataToPoCoHelper
    {
        public static CompanyDataRow GetCompanyDataRow(string symbol)
        {
            FmgCompanyProfile fmgCompanyProfile = RetrieveJsonDataHelper.RetrieveFmgCompanyProfile(symbol);
            FmgQuoteOnlyPrice fmgQuoteOnlyPrice = RetrieveJsonDataHelper.RetrieveFmgQuoteOnlyPrice(symbol);
            FmgInvestmentValuationRatios fmgInvestmentValuationRatios =
                RetrieveJsonDataHelper.RetrieveFmgInvestmentValuationRatios(symbol);
            Fmg1MinQuote oneMinQuote = RetrieveJsonDataHelper.RetrieveAllFmg1MinQuote(symbol)[0];

            CompanyDataRow companyDataRow = new CompanyDataRow();
            companyDataRow.Symbol = symbol;
            companyDataRow.Price = fmgQuoteOnlyPrice.Price;
            double openPrice = oneMinQuote.Close;
            double curPrice = fmgQuoteOnlyPrice.Price;
            double changePercentage = (curPrice - openPrice) / openPrice * 100;
            double change = curPrice - openPrice;
            companyDataRow.ChangePercentage = changePercentage;
            companyDataRow.Change = change;
            companyDataRow.MarketCapital = fmgCompanyProfile.MktCap;
            companyDataRow.Sector = fmgCompanyProfile.Sector;
            companyDataRow.PriceToEarningRatio = fmgInvestmentValuationRatios.PriceEarningsRatio;
            companyDataRow.PriceToSalesRatio = fmgInvestmentValuationRatios.PriceToSalesRatio;
            companyDataRow.Industry = fmgCompanyProfile.Industry;


            return companyDataRow;
        }

    }
}
