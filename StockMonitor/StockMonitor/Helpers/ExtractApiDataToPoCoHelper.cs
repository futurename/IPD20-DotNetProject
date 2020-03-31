using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            companyDataRow.PriceChange = change;
            companyDataRow.MarketCapital = fmgCompanyProfile.MktCap;
            companyDataRow.Sector = fmgCompanyProfile.Sector;
            companyDataRow.PriceToEarningRatio = fmgInvestmentValuationRatios.PriceEarningsRatio;
            companyDataRow.PriceToSalesRatio = fmgInvestmentValuationRatios.PriceToSalesRatio;
            companyDataRow.Industry = fmgCompanyProfile.Industry;
            return companyDataRow;
        }

        public static Company GetCompanyBySymbol(string symbol)
        {
            FmgCompanyProfile fmgCompanyProfile = RetrieveJsonDataHelper.RetrieveFmgCompanyProfile(symbol);
            FmgInvestmentValuationRatios fmgInvestmentValuationRatios =
                RetrieveJsonDataHelper.RetrieveFmgInvestmentValuationRatios(symbol);
            Company company = new Company()
            {
                CompanyName = fmgCompanyProfile.CompanyName,
                Symbol = symbol,
                Exchange = fmgCompanyProfile.Exchange,
                MarketCapital = fmgCompanyProfile.MktCap,
                PriceToEarningRatio = fmgInvestmentValuationRatios.PriceEarningsRatio,
                PriceToSalesRatio = fmgInvestmentValuationRatios.PriceToSalesRatio,
                Industry = fmgCompanyProfile.Industry,
                Sector = fmgCompanyProfile.Sector,
                Description = fmgCompanyProfile.Description,
                //Website = fmgCompanyProfile.Website,
                CEO = fmgCompanyProfile.Ceo,
                Website = fmgCompanyProfile.Website,
                Logo = GetImageFromUrl(fmgCompanyProfile.Image)
            };
            return company;
        }

        private static byte[] GetImageFromUrl(string url)
        {
            using (WebClient webClient = new WebClient())
            {
                byte[] result = webClient.DownloadData(url);
                return result;
            }
        }

        public static List<QuoteDaily> GetQuoteDailyList(string symbol)
        {
            List<QuoteDaily> result = new List<QuoteDaily>();
            List<FmgCandleDaily> quoteList = RetrieveJsonDataHelper.RetrieveFmgDataDaily(symbol);
            foreach (var dailyQuote in quoteList)
            {
                QuoteDaily quoteDaily = new QuoteDaily
                {
                    Symbol = symbol,
                    Date = dailyQuote.Date,
                    Open = dailyQuote.Open,
                    High = dailyQuote.High,
                    Low = dailyQuote.Low,
                    Close = dailyQuote.Close,
                    Volume = dailyQuote.Volume,
                    Vwap = dailyQuote.Vwap,
                    ChangeOverTime = dailyQuote.ChangeOverTime
                };
                result.Add(quoteDaily);
            }

            return result;
        } 
    }
}
