﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
                       Thread t= new Thread(()=>GetSubListDailyQuotes(subList, i));
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
                List<QuoteDaily> dailyQuoteList = ExtractApiDataToPoCoHelper.GetQuoteDailyList(subList[i]);
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
                        Console.Out.WriteLine(info + "!!!! DB save changes failure: " + subList[i]);
                    }

                    Console.Out.WriteLine(
                        $"{info}Insert {subList[i]}, from {dailyQuoteList[0].Date} to {dailyQuoteList[dailyQuoteList.Count - 1].Date}, total: {dailyQuoteList.Count}, time: {timeConsume.TotalSeconds} sec");
                }
            }
        }
    }
}
