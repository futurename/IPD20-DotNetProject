using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMonitor.Models.UIClasses;

namespace StockMonitor.Helpers
{
    public static class DatabaseHelper
    {
        private static DbStockMonitor dbContext = new DbStockMonitor();
        public static void InsertCompanyToDb(string symbol)
        {
            Company company = GUIHelper.GetCompanyBySymbol(symbol);
            try
            {
                dbContext.Companies.Add(company);
                dbContext.SaveChanges();
                Console.Out.WriteLine(company.ToString());
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
                Stopwatch sw = Stopwatch.StartNew();
                Company company =
                    dbContext.Companies.AsNoTracking().FirstOrDefault(p => p.Symbol == symbol) as Company;
                sw.Stop();
                TimeSpan span = sw.Elapsed;
                Console.Out.WriteLine($"Get company {symbol} from db: {span.TotalMilliseconds} mills");
                return company;
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
                List<QuoteDaily> result = dbContext.QuoteDailies.AsNoTracking().Where(p => p.Symbol == symbol)
                        .ToList();
                return result;
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
                TradingRecord record = dbContext.TradingRecords.AsNoTracking().Where(r => r.Id == recordId).FirstOrDefault();
                return record;
            }
            catch (SystemException ex)
            {
                throw new SystemException($"GetTradingRecordFromDb exception, id: {recordId} > {ex.Message}");
            }
        }

        


    }

}
