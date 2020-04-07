using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GUI;
using StockMonitor.Models.UIClasses;

namespace StockMonitor.Helpers
{
    public static class DatabaseHelper
    {
        // private static DbStockMonitor _dbContext = new DbStockMonitor();

        public static void InsertCompanyToDb(string symbol)
        {

            Company company = GUIDataHelper.GetCompanyBySymbol(symbol);
            try
            {
                using (DbStockMonitor _dbContext = new DbStockMonitor())
                {

                    _dbContext.Companies.Add(company);
                    _dbContext.SaveChanges();
                    Console.Out.WriteLine(company.ToString());
                }
            }
            catch (Exception e)
            {
                throw new SystemException("InsertCompanyToDb exception: {symbol} > {ex.Message}");
            }
        }

        public static Company GetCompanyFromDb(string symbol)
        {
            try
            {
                using (DbStockMonitor _dbContext = new DbStockMonitor())
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    Company company =
                        _dbContext.Companies.AsNoTracking().FirstOrDefault(p => p.Symbol == symbol) as Company;
                    sw.Stop();
                    TimeSpan span = sw.Elapsed;
                    Console.Out.WriteLine($"Get company {symbol} from db: {span.TotalMilliseconds} mills");
                    return company;
                }
            }
            catch (SystemException ex)
            {
                throw new SystemException($"GetCompanyFromDb exception: {symbol} > {ex.Message}");
            }
        }

        public static List<QuoteDaily> GetQuoteDailyListFromDb(string symbol)
        {
            try
            {
                using (DbStockMonitor _dbContext = new DbStockMonitor())
                {
                    List<QuoteDaily> result = _dbContext.QuoteDailies.AsNoTracking().Where(p => p.Symbol == symbol)
                        .ToList();
                    return result;
                }
            }
            catch (SystemException ex)
            {
                throw new SystemException($"GetQuoteDailyListFromDb exception: {symbol} > {ex.Message}");
            }
        }

        public static TradingRecord GetTradingRecordFromDb(int recordId)
        {
            try
            {
                using (DbStockMonitor _dbContext = new DbStockMonitor())
                {
                    TradingRecord record =
                        _dbContext.TradingRecords.AsNoTracking().FirstOrDefault(r => r.Id == recordId);
                    return record;
                }
            }
            catch (SystemException ex)
            {
                throw new SystemException($"GetTradingRecordFromDb exception, id: {recordId} > {ex.Message}");
            }
        }

        private static List<int> GetWatchCompanyIdsFromDb(int userId)
        {
            try
            {
                using (DbStockMonitor _dbContext = new DbStockMonitor())
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    List<int> result = _dbContext.WatchListItems.AsNoTracking().Where(i => i.UserId == userId)
                        .Select(i => i.CompanyId)
                        .ToList();

                    sw.Stop();
                    Console.Out.WriteLine(
                        $"\n>>> Time to get watchlistitems for user: {userId} is {sw.Elapsed.TotalMilliseconds} mills");

                    //result.ForEach(p=>Console.WriteLine(p.ToString()));
                    return result;
                }
            }
            catch (SystemException ex)
            {
                throw new SystemException($"GetWatListIdsFromDb exception, id: {userId} > {ex.Message}");
            }
        }

        public static List<Company> GetWatchListCompaniesFromDb(int userId)
        {
            try
            {
                using (DbStockMonitor _dbContext = new DbStockMonitor())
                {
                    Stopwatch sw = Stopwatch.StartNew();

                    List<int> companyIds = GetWatchCompanyIdsFromDb(userId);
                    List<Company> result = _dbContext.WatchListItems.Include("Company").AsNoTracking()
                        .Where(u => u.UserId == userId).Select(p => p.Company).ToList();

                    sw.Stop();
                    Console.Out.WriteLine(
                        $"\n----- Time to match all watch item with all companies for user: {userId} is {sw.Elapsed.TotalMilliseconds} mills");
                    return result;
                }
            }
            catch (SystemException ex)
            {
                throw new SystemException($"GetWatchListSymbolsFromDb exception, id: {userId} > {ex.Message}");
            }
        }


        public static void DeleteFromWatchListById(int userId, int companyId)
        {
            try
            {
                using (DbStockMonitor _dbContext = new DbStockMonitor())
                {
                    var result = _dbContext.WatchListItems.Include("Company")
                        .FirstOrDefault(w => w.UserId == userId && w.CompanyId == companyId);
                    if (result == null)
                    {
                        throw new SystemException($"*** No such item in watchlist db, {userId}, {companyId}");
                    }
                    else
                    {
                        _dbContext.WatchListItems.Attach(result);
                        _dbContext.WatchListItems.Remove(result);
                        _dbContext.SaveChanges();
                    }
                }
            }
            catch (SystemException ex)
            {
                throw new SystemException(
                    $"\n*** Delete item from watchlist fails. userid: {userId}, companyId: {companyId}. " +
                    ex.Message);
            }

        }

        public static void AddItemToWatchList(int userId, int companyId)
        {
            try
            {
                using (DbStockMonitor _dbContext = new DbStockMonitor())
                {
                    var result = _dbContext.WatchListItems
                        .Include("Company").FirstOrDefault(w => w.UserId == userId && w.CompanyId == companyId);
                    if (result != null)
                    {
                        throw new SystemException("Item EXISTS in database");
                    }
                    else
                    {
                        WatchListItem item = new WatchListItem { UserId = userId, CompanyId = companyId };
                        _dbContext.WatchListItems.Add(item);
                        _dbContext.SaveChanges();
                        Console.Out.WriteLine($"*** SUCCESSFULLY add item to watch list. {userId}, {companyId}");
                    }
                }
            }
            catch (SystemException ex)
            {
                throw new SystemException($"\n*** Add item to watchlist failed. {userId}:{companyId}. " + ex.Message);
            }

        }


        public static List<Company> GetCompanyListBySearch(string searchString)
        {
            try
            {
                using (DbStockMonitor _dbContext = new DbStockMonitor())
                {
                    var searchItems = _dbContext.Companies.AsNoTracking().Where(c => c.Symbol.Contains(searchString))
                        .OrderBy(c => c.Symbol).Take(10).ToList();
                    if (searchItems.Count != 0)
                    {
                        return searchItems.Select(item => item as Company).ToList();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (SystemException ex)
            {
                throw new SystemException($"Get company list by search failed: {searchString} " + ex.Message);
            }
        }

    }
}
