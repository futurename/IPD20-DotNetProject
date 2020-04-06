using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using StockMonitor.Models.ApiModels;
using StockMonitor.Models.JSONModels;

namespace StockMonitor.Helpers
{
    public static class DANGER_DatabaseDataInitHelper
    {
        public static void FirstImportStockListToDatabase()
        {
            int counter = 0;
            using (DbStockMonitor context = new DbStockMonitor())
            {
                counter = context.Companies.Count();
                Console.Out.WriteLine($"Counter start: {counter}");
            }

            List<FmgStockListEntity> stockList = RetrieveJsonDataHelper.RetrieveStockList();
            Console.Out.WriteLine("***Length of list: " + stockList.Count + "\n\n");
            for (int i = 0; i < 5000; i++)
            {
                Console.Out.Write($"{i}: ");
                string symbol = stockList[i].Symbol;
                Company company = null;
                try
                {
                    company = GUIDataHelper.GetCompanyBySymbol(symbol);
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

        public static void SecondInsertAllDailyQuote()
        {
            using (DbStockMonitor dbctx = new DbStockMonitor())
            {
                List<string> symbolList = dbctx.Companies.Select(p => p.Symbol).ToList();
                const int threadsNum = 5;
                int avgListLength = symbolList.Count / threadsNum;
                int lengthCounter = 0, curStart = 0;

                for (int i = 0; i < threadsNum; i++)
                {
                    List<string> subList;
                    if (i != threadsNum - 1)
                    {
                        subList = symbolList.GetRange(lengthCounter, avgListLength);
                        curStart = lengthCounter;
                        lengthCounter = lengthCounter + avgListLength + 1;
                    }
                    else
                    {
                        subList = symbolList.GetRange(lengthCounter, symbolList.Count - lengthCounter);
                    }

                    try
                    {
                        Thread t = new Thread(() => GetSubListDailyQuotes(subList, i));
                        t.Start();
                    }
                    catch (ArgumentNullException ex)
                    {
                        Console.Out.WriteLine("Thread start failure: " + ex.Message);
                    }
                }
            }
        }

        private static void GetSubListDailyQuotes(List<string> subList, int index)
        {
            for (int i = 0; i < subList.Count; i++)
            {
                string info = $"T{index}=>{i}: ";
                List<QuoteDaily> dailyQuoteList = GUIDataHelper.GetQuoteDailyList(subList[i]);
                TimeSpan timeConsume = new TimeSpan();
                using (DbStockMonitor dbctx = new DbStockMonitor())
                {
                    try
                    {
                        DateTime start = DateTime.Now;
                        dailyQuoteList.ForEach(p => dbctx.QuoteDailies.Add(p));
                        dbctx.SaveChanges();
                        DateTime end = DateTime.Now;
                        timeConsume = end - start;
                    }
                    catch (SystemException ex)
                    {
                        Console.Out.WriteLine(info + "!!!! DB save changes failure: " + subList[i] + ", " + ex.Message);
                    }

                    Console.Out.WriteLine(
                        $"{info}Insert {subList[i]}, from {dailyQuoteList[0].Date} to {dailyQuoteList[dailyQuoteList.Count - 1].Date}, total: {dailyQuoteList.Count}, time: {timeConsume.TotalSeconds} sec");
                }
            }
        }

        public static void FilterSybomlNoQuoteData()
        {
            int counter = 0;
            string filepath =
                "C:\\Users\\WW\\Desktop\\SourceTree\\IPD20-DotNetProject\\StockMonitor\\InvalidSymbols.txt";
            using (DbStockMonitor context = new DbStockMonitor())
            {
                List<string> symbolList = context.Companies.AsNoTracking().Select(p => p.Symbol).ToList();

                for (int i = 0; i < symbolList.Count; i++)
                {
                    string symbol = symbolList[i];
                    string response = RetrieveJsonDataHelper.GetQuoteStringBySymbol(symbol).Result;
                    if (response.Length < 10)
                    {
                        Console.Out.WriteLine($"<{i}>: find {++counter} - {symbol} single quote has NO data: <{response}>");
                        File.AppendAllText(filepath, $"{symbol}\n");
                    }
                }
            }
        }

        public static void DeleteNoQuoteDataSybolsAndRecordsFromDb()
        {
            string filepath =
                "C:\\Users\\WW\\Desktop\\SourceTree\\IPD20-DotNetProject\\StockMonitor\\InvalidSymbols.txt";
            List<string> list = File.ReadAllText(filepath).Split('\n').ToList();
            foreach (var symbol in list)
            {
                using (DbStockMonitor context = new DbStockMonitor())
                {

                    var dailyQuoteList =
                         context.QuoteDailies.Where(r => r.Symbol == symbol).ToList();
                    Console.Out.WriteLine($"Deleting records for {symbol}: {dailyQuoteList.Count}");

                    for (int i = 0; i < dailyQuoteList.Count; i++)
                    {
                        context.QuoteDailies.Remove(dailyQuoteList[i]);
                        context.SaveChanges();
                        if (i % 50 == 0 && i != 0)
                        {
                            Console.Out.WriteLine($"Deleted 50 records, left: {dailyQuoteList.Count - i}");
                        }
                    }

                    context.SaveChanges();

                    Company company = context.Companies.FirstOrDefault(c => c.Symbol == symbol);
                    context.Companies.Remove(company);
                    context.SaveChanges();

                    Console.Out.WriteLine($"Delete {symbol}");
                }
            }

        }


        public static void FilterCompanyHasSingleQuoteResult()
        {
            using (DbStockMonitor context = new DbStockMonitor())
            {
                List<Company> allCompanyList = context.Companies.AsNoTracking().ToList();
                foreach (var company in allCompanyList)
                {

                    string response = RetrieveJsonDataHelper.GetQuoteStringBySymbol(company.Symbol).Result;
                    if (response.Length < 20)
                    {
                        List<QuoteDaily> dailyQuoteList =
                            context.QuoteDailies.AsNoTracking().Where(q=>q.Symbol == company.Symbol).ToList();
                        Console.Out.WriteLine($"*** Found: {company.Symbol},will remove {dailyQuoteList.Count} records. response: {response}");

                        for (int i = 0;i < dailyQuoteList.Count; i++)
                        {
                            
                            context.QuoteDailies.Remove(dailyQuoteList[i]);
                            if(i % 50 == 0 && i != 0)
                            {
                                Console.Out.WriteLine($"{company.Symbol}: Deleted 50 records, left: {dailyQuoteList.Count - i}");
                            }
                            context.SaveChanges();
                        }

                        context.Companies.Remove(company);
                        context.SaveChanges();
                    }
                }
            }
           
        }
    }
}
