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
      private static DbContext dbContext = new DbContext();


      public static void InsertCompany(string symbol)
      {
          Company company = ExtractApiDataToPoCoHelper.GetCompanyBySymbol(symbol);

          try
          {
              dbContext.CompanySet.Add(company);
              dbContext.SaveChanges();
              Console.Out.WriteLine(company.ToString());
            }
          catch (Exception e)
          {
              Console.WriteLine(e.Message);
          }
         
        }
    }
    
}
