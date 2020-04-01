using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using StockMonitor.Models.ApiModels;
using StockMonitor.Models.JSONModels;
using StockMonitor.Models.UIClasses;

namespace StockMonitor.Helpers
{
    public static class ExtractApiDataToPoCoHelper
    {

        public static async Task<UIComapnyRow> GetCompanyDataRowNo1MinData(string symbol)
        {
            DateTime start = DateTime.Now;

            FmgQuoteOnlyPrice fmgQuoteOnlyPrice = await RetrieveJsonDataHelper.RetrieveFmgQuoteOnlyPrice(symbol);
            //Fmg1MinQuote oneMinQuote = (await RetrieveJsonDataHelper.RetrieveAllFmg1MinQuote(symbol))[0];

            DateTime end = DateTime.Now;
            TimeSpan timeSpan = new TimeSpan();
            timeSpan = end - start;
            Console.Out.WriteLine($"Time: {timeSpan.TotalMilliseconds} mills for {symbol}");

            Company company = DatabaseHelper.GetCompanyFromDb(symbol);



            UIComapnyRow companyRow = new UIComapnyRow();
            companyRow.Symbol = symbol;
            companyRow.Price = fmgQuoteOnlyPrice.Price;
           // double openPrice = oneMinQuote.Close;
            double curPrice = fmgQuoteOnlyPrice.Price;
           // double changePercentage = (curPrice - openPrice) / openPrice * 100;
            //double change = curPrice - openPrice;
            //companyRow.Open = oneMinQuote.Open;
            //companyRow.Volume = oneMinQuote.Volume;
            //companyRow.ChangePercentage = changePercentage;
            //companyRow.PriceChange = change;
            companyRow.MarketCapital = company.MarketCapital;
            companyRow.Sector = company.Sector;
            companyRow.PriceToEarningRatio = company.PriceToEarningRatio;
            companyRow.PriceToSalesRatio = company.PriceToSalesRatio;
            companyRow.Industry = company.Industry;
            companyRow.Logo = company.Logo;

            return companyRow;
        }

        public static async Task<UIComapnyRow> GetCompanyDataRow(string symbol)
        {
            DateTime start = DateTime.Now;

            FmgQuoteOnlyPrice fmgQuoteOnlyPrice = await RetrieveJsonDataHelper.RetrieveFmgQuoteOnlyPrice(symbol);
            Fmg1MinQuote oneMinQuote = (await RetrieveJsonDataHelper.RetrieveAllFmg1MinQuote(symbol))[0];

            DateTime end = DateTime.Now;
            TimeSpan timeSpan = new TimeSpan();
            timeSpan = end - start;
            Console.Out.WriteLine($"Time: {timeSpan.TotalMilliseconds} mills for {symbol}");

            Company company = DatabaseHelper.GetCompanyFromDb(symbol);



            UIComapnyRow companyRow = new UIComapnyRow();
            companyRow.Symbol = symbol;
            companyRow.Price = fmgQuoteOnlyPrice.Price;
            double openPrice = oneMinQuote.Close;
            double curPrice = fmgQuoteOnlyPrice.Price;
            double changePercentage = (curPrice - openPrice) / openPrice * 100;
            double change = curPrice - openPrice;
            companyRow.Open = oneMinQuote.Open;
            companyRow.Volume = oneMinQuote.Volume;
            companyRow.ChangePercentage = changePercentage;
            companyRow.PriceChange = change;
            companyRow.MarketCapital = company.MarketCapital;
            companyRow.Sector = company.Sector;
            companyRow.PriceToEarningRatio = company.PriceToEarningRatio;
            companyRow.PriceToSalesRatio = company.PriceToSalesRatio;
            companyRow.Industry = company.Industry;
            companyRow.Logo = company.Logo;
          
            return companyRow;
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
