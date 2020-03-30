using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMonitor.Models.POCO;

namespace StockMonitor.Helpers
{
    public static class DatabaseHelper
    {
        private static DbStockMonitor dbContext = new DbStockMonitor();


        public static void InsertCompany(string symbol)
        {
            Company company = ExtractApiDataToPoCoHelper.GetCompanyBySymbol(symbol);

            try
            {
                dbContext.Companies.Add(company);
                dbContext.SaveChanges();
                Console.Out.WriteLine(company.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static Company ReadCompany(string symbol)
        {
            try
            {
               return  dbContext.Companies.Where(p => p.Symbol == "CMCSA").FirstOrDefault() as Company;
            }
            catch (SystemException ex)
            {
                Console.Out.WriteLine("ReadCompany exception: " + symbol);
            }

            return null;
        }
    }

}
