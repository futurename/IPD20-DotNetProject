using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMonitor.Models.UIClasses;

namespace StockMonitor.Helpers
{
    public static class DatabaseHelper
    {
        public static void InsertCompanyToDb(string symbol)
        {
            Company company = ExtractApiDataToPoCoHelper.GetCompanyBySymbol(symbol);

            using (DbStockMonitor dbContext = new DbStockMonitor())
            {
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
        }

        public static Company GetCompanyFromDb(string symbol)
        {
            using (DbStockMonitor dbContext = new DbStockMonitor())
            {
                try
                {
                    return dbContext.Companies.FirstOrDefault(p => p.Symbol == symbol) as Company;
                }
                catch (SystemException ex)
                {
                   throw new SystemException($"GetCompanyFromDb exception: {symbol} > {ex.Message}"); 
                }
            }
        }

        public static List<QuoteDaily> GetQuoteDailyListFromDb(string symbol)
        {

            using (DbStockMonitor dbContext = new DbStockMonitor())
            {
                try
                {
                    List<QuoteDaily> result = dbContext.QuoteDailies.Where(p => p.Symbol == symbol).ToList();
                    return result;
                }
                catch (SystemException ex)
                {
                    throw new SystemException("GetQuoteDailyListFromDb exception: {symbol} > {ex.Message}");
                }
            }
        }

         
    }

}
