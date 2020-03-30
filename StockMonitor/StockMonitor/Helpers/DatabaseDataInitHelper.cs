using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using StockMonitor.Models.ApiModels;
using StockMonitor.Models.JSONModels;

namespace StockMonitor.Helpers
{
    public static class DatabaseDataInitHelper
    {
        
        public static void FirstImportStockListToDatabase()
        {
            int counter = 0;
           
            List<FmgStockListEntity> stockList = RetrieveJsonDataHelper.RetrieveStockList();
            Console.Out.WriteLine("***Length of list: " + stockList.Count + "\n\n");
            foreach (var stock in stockList)
            {
                string symbol = stock.Symbol;
                Company company = null;
                try
                {
                    company = ExtractApiDataToPoCoHelper.GetCompanyBySymbol(symbol);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    Console.Out.WriteLine("!!!!! Failed: " + ex.Message + $" <{symbol}> ");
                }
                catch (ArgumentException ex)
                {
                    Console.Out.WriteLine("!!!!! Failed: " + ex.Message + $" <{symbol}> ");
                }
                catch (SystemException ex)
                {
                    Console.Out.WriteLine("!!!!! Failed: " + ex.Message + $" <{symbol}> ");
                }

                if (company == null)
                {
                    continue;
                }

                using (DbStockMonitor dbStockContext = new DbStockMonitor())
                {

                    dbStockContext.Companies.Add(company);
                    try
                    {

                        dbStockContext.SaveChanges();
                        Console.Out.WriteLine($">>>>> {++counter}: Successfully insert record: <{company.Symbol}>");
                    }
                    catch (SystemException ex)
                    {
                        Console.Out.WriteLine($"!!!!! Database save changes exception! <{company.Symbol}>, " + ex.Message);
                    }
                }

                
            }
        }
    }
}
