using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using StockMonitor.Models.ApiModels;
using StockMonitor.Models.JSONModels;
using StockMonitor.Models.UIClasses;

namespace StockMonitor.Helpers
{
    public static class GUIDataHelper
    {

        public static async Task<UIComapnyRow> GetCompanyDataRowTask(string symbol)
        {
            DateTime start = DateTime.Now;

            FmgQuoteOnlyPrice fmgQuoteOnlyPrice = await RetrieveJsonDataHelper.RetrieveFmgQuoteOnlyPrice(symbol);
            //Fmg1MinQuote oneMinQuote = (await RetrieveJsonDataHelper.RetrieveAllFmg1MinQuote(symbol))[0];
            FmgSingleQuote singleQuote = await RetrieveJsonDataHelper.RetrieveFmgSingleQuote(symbol);

            DateTime end = DateTime.Now;
            TimeSpan timeSpan = new TimeSpan();
            timeSpan = end - start;
            Console.Out.WriteLine($"Time: {timeSpan.TotalMilliseconds} mills for {symbol}");

            Company company = DatabaseHelper.GetCompanyFromDb(symbol);

            UIComapnyRow companyRow = new UIComapnyRow();
            companyRow.Symbol = symbol;
            companyRow.Price = fmgQuoteOnlyPrice.Price;
            double openPrice = singleQuote.open;
            double curPrice = fmgQuoteOnlyPrice.Price;
            double changePercentage = (curPrice - openPrice) / openPrice * 100;
            double change = curPrice - openPrice;
            companyRow.Open = openPrice;
            companyRow.Volume = singleQuote.volume;
            companyRow.ChangePercentage = changePercentage;
            companyRow.PriceChange = change;
            companyRow.MarketCapital = company.MarketCapital;
            companyRow.Sector = company.Sector;
            companyRow.PriceToEarningRatio = company.PriceToEarningRatio;
            companyRow.PriceToSalesRatio = company.PriceToSalesRatio;
            companyRow.Industry = company.Industry;
            companyRow.Logo = company.Logo;
            companyRow.Description = company.Description;

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


        public static async Task<UICompanyRowDetail> GetUICompanyRowDetailTask(string symbol, List<UIComapnyRow> companyList)
        {
            FmgSingleQuote singleQuote = await RetrieveJsonDataHelper.RetrieveFmgSingleQuote(symbol);
            UIComapnyRow companyRow = companyList.Find(c => c.Symbol == symbol);
            Company company = DatabaseHelper.GetCompanyFromDb(symbol);
            UICompanyRowDetail result = new UICompanyRowDetail
            {
                Symbol = symbol,
                Name = company.CompanyName,
                Price = companyRow.Price,
                Open = companyRow.Open,
                High = singleQuote.dayHigh,
                Low = singleQuote.dayLow,
                Volume = companyRow.Volume,
                Change = companyRow.PriceChange,
                ChangePercentage = companyRow.ChangePercentage,
                Description = company.Description,
                Ceo = company.CEO,
                Industry = company.Industry,
                Sector = company.Sector
            };
            return result;
        }


        public static List<UIComapnyRow> GetWatchUICompanyRowList(int userId)
        {
            List<Company> watchListCompanies = DatabaseHelper.GetWatchListCompaniesFromDb(userId);
           
            List<Task<UIComapnyRow>> taskList = new List<Task<UIComapnyRow>>();
            foreach (var company in watchListCompanies)
            {
               taskList.Add( Task.Run(async () =>
               {
                   Stopwatch sw = Stopwatch.StartNew();

                   FmgQuoteOnlyPrice fmgQuoteOnlyPrice =
                       await RetrieveJsonDataHelper.RetrieveFmgQuoteOnlyPrice(company.Symbol);
                   FmgSingleQuote singleQuote = await RetrieveJsonDataHelper.RetrieveFmgSingleQuote(company.Symbol);

                   UIComapnyRow companyRow = new UIComapnyRow();
                   companyRow.Symbol = company.Symbol;
                   companyRow.Price = fmgQuoteOnlyPrice.Price;
                   double openPrice = singleQuote.open;
                   double curPrice = fmgQuoteOnlyPrice.Price;
                   double changePercentage = (curPrice - openPrice) / openPrice * 100;
                   double change = curPrice - openPrice;
                   companyRow.Open = openPrice;
                   companyRow.Volume = singleQuote.volume;
                   companyRow.ChangePercentage = changePercentage;
                   companyRow.PriceChange = change;
                   companyRow.MarketCapital = company.MarketCapital;
                   companyRow.Sector = company.Sector;
                   companyRow.PriceToEarningRatio = company.PriceToEarningRatio;
                   companyRow.PriceToSalesRatio = company.PriceToSalesRatio;
                   companyRow.Industry = company.Industry;
                   companyRow.Logo = company.Logo;
                   companyRow.Description = company.Description;
                   
                   sw.Stop();
                   Console.Out.WriteLine(
                       $"\n---- Add one companyRow to result in GetWatchUICompanyRowList: {company.Symbol}, time: {sw.Elapsed.TotalMilliseconds} mills");
                   return companyRow;
               }));
            }
            
            List<UIComapnyRow> result = new List<UIComapnyRow>();
            taskList.ForEach(t => result.Add(t.Result));

            return result;
        }

    }
}
