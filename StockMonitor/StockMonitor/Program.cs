using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using StockMonitor.Helpers;
using StockMonitor.Models.ApiModels;
using StockMonitor.Models.JSONModels;

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

         DatabaseHelper.InsertCompany("AAPL");

            Console.ReadKey();
        }


    }
}
