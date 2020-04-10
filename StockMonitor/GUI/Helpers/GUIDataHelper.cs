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
using GUI;
using GUI.Helpers;
using StockMonitor.Models.ApiModels;
using StockMonitor.Models.JSONModels;
using StockMonitor.Models.UIClasses;

namespace StockMonitor.Helpers
{
    public static class GUIDataHelper
    {

        public static async Task<UIComapnyRow> GetUICompanyRowTaskBySymbol(string symbol)//ex FormatException
        {//ex: ArgumentException, SystemException, FormatException
            try
            {
                DateTime start = DateTime.Now;

                FmgQuoteOnlyPrice fmgQuoteOnlyPrice = await RetrieveJsonDataHelper.RetrieveFmgQuoteOnlyPrice(symbol); //ex: ArgumentException
                FmgSingleQuote singleQuote = await RetrieveJsonDataHelper.RetrieveFmgSingleQuote(symbol); //ex: ArgumentException

                DateTime end = DateTime.Now;
                TimeSpan timeSpan = new TimeSpan();
                timeSpan = end - start;
                Console.Out.WriteLine($"Time: {timeSpan.TotalMilliseconds} mills for {symbol}");

                Company company = DatabaseHelper.GetCompanyFromDb(symbol);//ex: SystemException

                return new UIComapnyRow(company, fmgQuoteOnlyPrice, singleQuote);//ex: FormatException
            }
            catch (SystemException ex)
            {
                throw new SystemException(ex.Message);
            }
        }

        public static Company GetCompanyBySymbol(string symbol)
        {
            try
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
            catch (SystemException ex)
            {
                throw new SystemException(ex.Message);
            }
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
            try
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
            catch (SystemException ex)
            {
                throw new SystemException(ex.Message);
            }
        }


        public static async Task<UICompanyRowDetail> GetUICompanyRowDetailTask(string symbol, List<UIComapnyRow> companyList)
        {
            try
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
            catch (SystemException ex)
            {
                throw new SystemException(ex.Message);
            }
        }


        public static List<Task<UIComapnyRow>> GetWatchUICompanyRowTaskList(int userId)
        {
            try
            {
                List<Company> watchListCompanies = DatabaseHelper.GetWatchListCompaniesFromDb(userId);

                List<Task<UIComapnyRow>> taskList = new List<Task<UIComapnyRow>>();
                foreach (var company in watchListCompanies)
                {
                    taskList.Add(GetWatchUIComanyRowTask(userId, company));
                }

                return taskList;
            }
            catch (SystemException ex)
            {
                throw new SystemException(ex.Message);
            }
        }

        private static async Task<UIComapnyRow> GetWatchUIComanyRowTask(int userId, Company company)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                FmgQuoteOnlyPrice fmgQuoteOnlyPrice =
                    await RetrieveJsonDataHelper.RetrieveFmgQuoteOnlyPrice(company.Symbol);
                FmgSingleQuote singleQuote = await RetrieveJsonDataHelper.RetrieveFmgSingleQuote(company.Symbol);

                UIComapnyRow
                    companyRow = new UIComapnyRow(company, fmgQuoteOnlyPrice, singleQuote); //ex: FormatException

                sw.Stop();
                Console.Out.WriteLine(
                    $"\n---- Add one companyRow to result in GetWatchUICompanyRowList: {company.Symbol}, time: {sw.Elapsed.TotalMilliseconds} mills");
                return companyRow;
            }
            catch (SystemException ex)
            {
                throw new SystemException(ex.Message);
            }
        }

        public static async Task DeleteFromWatchListTask(int userId, int companyId)
        {
            try
            {
                await Task.Run(() => DatabaseHelper.DeleteFromWatchListById(userId, companyId));
            }
            catch (SystemException ex)
            {
                throw new SystemException(ex.Message);
            }
        }

        public static async Task AddItemToWatchListTast(int userId, int companyId)
        {
            try
            {
                await Task.Run(() => DatabaseHelper.AddItemToWatchList(userId, companyId));
            }
            catch (SystemException ex)
            {
                throw new SystemException(ex.Message);
            }

        }

        public static async Task<List<Company>> GetSearchCompanyListTask(string searchString)
        {
            try
            {
                return await Task.Run(() => DatabaseHelper.SearchCompanyListBySymbol(searchString));
            }
            catch (SystemException ex)
            {
                throw new SystemException(ex.Message);
            }
        }

        public static void InsertReservedTrading(ReservedTrading reservedTrading)//ex DateException, InvalidOperationException
        {
            TradeDatabaseHelper.InsertReservedTrading(reservedTrading);//ex DateException, InvalidOperationException
        }

        public static List<ReservedTrading> GetReservedList(int userId)//ex InvalidOperationException
        {
            return TradeDatabaseHelper.GetReservedTradingList(userId);//ex InvalidOperationException
        }

        public static void AddTradingRecord(TradingRecord tradingRecord)
        {
            TradeDatabaseHelper.AddTradingRecord(tradingRecord);
        }
        public static void DeleteReservedTrading(ReservedTrading reservedTrading)//ex DataException,InvalidOperationException
        {
            TradeDatabaseHelper.DeleteReservedTrading(reservedTrading);//ex DataException,InvalidOperationException
        }

        public static Dictionary<string, int> GetTradingRecourdList(int userId)//ex DataException,InvalidOperationException
        {
            return TradeDatabaseHelper.GetTradingRecordList(userId);//ex DataException,InvalidOperationException
        }
        



    }
}
