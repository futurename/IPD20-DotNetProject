using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using StockMonitor.Helpers;
using StockMonitor.Models.ApiModels;
using StockMonitor.Models.JSONModels;
using StockMonitor.Models.UIClasses;

namespace StockMonitor
{
    class Program
    {

        static void Main(string[] args)
        {
            /*List<FmgCandleDaily> fmgDataDaily = RetrieveJsonData.RetriveFmgDataDaily("AAPL");
            foreach (var candleDaily in fmgDataDaily)
            {
                Console.Out.WriteLine(candleDaily.ToString());
            }
            Console.ReadKey();*/
            /*
                        FinnCompanyProfile profle = RetrieveJsonData.RetrieveFinnCompanyProfile("AAPL");
                        Console.Out.WriteLine(profle.ToString());*/


            /* FmgCompanyProfile profle = RetrieveJsonData.RetrieveFmgCompanyProfile("AAPL");
             Console.Out.WriteLine(profle.ToString());*/


            /* FinnQuote quote = RetrieveJsonData.RetrieveFinnQuote("AAPL");
             Console.Out.WriteLine(quote);*/


            /* List<FmgMajorIndex> indexList = RetrieveJsonDataHelper.RetrieveFmgMajorIndexes();
             foreach (var index in indexList)
             {
                 Console.Out.WriteLine(index.ToString());
             }*/

            /*   List<FmgQuoteOnlyPrice> prices = RetrieveJsonDataHelper.RetrievAllFmgQuoteOnlyPrice();
               foreach (var quote in prices)
               {
                   Console.Out.WriteLine(quote);
               }*/

            /*  FmgQuoteOnlyPrice quote = RetrieveJsonDataHelper.RetrieveFmgQuoteOnlyPrice("AAPL");
              Console.Out.WriteLine(quote);*/

            /*   DateTime time1 = DateTime.Now;
               List<Fmg1MinQuote> fmg1MinQuotes = RetrieveJsonDataHelper.RetrieveAllFmg1MinQuote("AAPL");
              // fmg1MinQuotes.ForEach(q => Console.WriteLine(q.ToString()));
               DateTime time2 = DateTime.Now;
               Console.Out.WriteLine((time2-time1) + " ms");*/


            /*FmgInvestmentValuationRatios investment =
                RetrieveJsonDataHelper.RetrieveFmgInvestmentValuationRatios("AAPL");
            Console.Out.WriteLine(investment.ToString());*/

            //DatabaseHelper.InsertCompanyToDb("AAPL");

            //RetrieveJsonDataHelper.RetrieveStockList().ForEach(o=>Console.WriteLine(o.ToString()));

            // DatabaseDataInitHelper.FirstImportStockListToDatabase();

            //Console.Out.WriteLine(DatabaseHelper.GetCompanyFromDb("CMCSA"));

            //DatabaseDataInitHelper.SecondInsertAllDailyQuote();
            /*  double prePrice = 0, curPrice = 0;
              int counter = 0;

              while (true)
              {
                  curPrice = RetrieveJsonDataHelper.RetrieveFmgQuoteOnlyPrice("AAPL").Price;
                  //Console.Out.WriteLine("Before display: "+ curPrice);
                  prePrice = AsyncGetQuoteOnlyPrice("AAPL", curPrice, prePrice, counter).Result;
              }
  */

            //DateTime start = DateTime.Now;

            /*   List<QuoteDaily> dailyList = DatabaseHelper.GetQuoteDailyListFromDb("AAPL");
               DateTime end = DateTime.Now;
               TimeSpan span = new TimeSpan();
               span = end - start;
               Console.Out.WriteLine($"Read list time: {span.Milliseconds} mills");
               //dailyList.ForEach(p=>Console.WriteLine(p.ToString()));
     */

            /*  int num = 200;
              DateTime start = DateTime.Now;

              TestBatchRetrieveDailyQuoteList(num);
              DateTime end = DateTime.Now;
              TimeSpan timeSpan= new TimeSpan();
              timeSpan = end - start;
              Console.Out.WriteLine($"Total time for <{num}> companies: {timeSpan.Milliseconds} mills");*/
            /*
                        for (int i = 0; i < 10; i++)
                        {

                            DateTime start = DateTime.Now;

                            *//*     Console.Out.WriteLine(DatabaseHelper.GetCompanyFromDb("AAPL"));
                                 Console.Out.WriteLine(DatabaseHelper.GetCompanyFromDb("GOOG"));
                                 Console.Out.WriteLine(DatabaseHelper.GetCompanyFromDb("AMZN"));*//*

                            Company company = DatabaseHelper.GetCompanyFromDb("AAPL");

                            DateTime end = DateTime.Now;
                            TimeSpan timeSpan = new TimeSpan();
                            timeSpan = end - start;
                            Console.Out.WriteLine($"***Time spent: {timeSpan.TotalMilliseconds} mills\n");
                        }*/

            // DatabaseDataInitHelper.FilterSybomlNoQuoteData();

            //   DatabaseDataInitHelper.DeleteNoQuoteDataSybolsAndRecordsFromDb();

         /*   List<UIComapnyRow> list = new List<UIComapnyRow>();
            list.Add(GUIHelper.GetCompanyDataRowTask("AAPL").Result);
            list.Add(GUIHelper.GetCompanyDataRowTask("FB").Result);
            UICompanyRowDetail detail1 = GUIHelper.GetUICompanyRowDetailTask("AAPL", list).Result;
            Console.Out.WriteLine(detail1);
            UICompanyRowDetail detail2 = GUIHelper.GetUICompanyRowDetailTask("FB", list).Result;
            Console.Out.WriteLine(detail2);*/


         List<Fmg1MinQuote> quotes = RetrieveJsonDataHelper.RetrieveAllFmg1MinQuote("AAPL").Result;
         quotes.ForEach(p=> Console.WriteLine(p.ToString()));

            Console.ReadKey();
        }
        private static async Task<double> AsyncGetQuoteOnlyPrice(string symbol, double curPrice, double prePrice, int counter)
        {

            if (Math.Abs(prePrice - curPrice) < 0.001)
            {
                await Task.Delay(2000);
                Console.Out.WriteLine($"No change, sleep 2 sec. {curPrice}");
            }
            else
            {
                Console.Out.WriteLine($"{counter++}: AAPL: {curPrice}");
                prePrice = curPrice;
            }

            return prePrice;
        }

        private static void TestBatchRetrieveDailyQuoteList(int num)
        {
            using (DbStockMonitor dbContext = new DbStockMonitor())
            {
                List<string> companyList = dbContext.Companies.Select(p => p.Symbol).ToList();
                Random rand = new Random();
                for (int i = 0; i < num; i++)
                {
                    int index = rand.Next(companyList.Count);

                    DateTime start = DateTime.Now;
                    List<QuoteDaily> quoteList = DatabaseHelper.GetQuoteDailyListFromDb(companyList[index]);

                    DateTime end = DateTime.Now;
                    TimeSpan timeSpan = new TimeSpan();
                    timeSpan = end - start;
                    Console.Out.WriteLine($"Retrieve {companyList[index]}, Use: {timeSpan.Milliseconds} mills, count: {quoteList.Count}");
                }
            }
        }
    }

}
