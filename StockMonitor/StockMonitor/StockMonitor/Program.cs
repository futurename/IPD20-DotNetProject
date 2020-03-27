using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMonitor.Helpers;
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

            FinnCompanyProfile profle = RetrieveJsonData.RetriveFinnCompanyProfile("AAPL");
            Console.Out.WriteLine(profle.ToString());
            Console.ReadKey();
        }
    }
}
