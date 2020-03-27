using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _20200326TestDatabase;

namespace _20200326EFMySqlConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            TestModel context = new TestModel();
            Quote q1 = new Quote() {price = 22.45, symbol = "AAPL"};
            context.Quotes.Add(q1);
            context.SaveChanges();

            var list = context.Quotes.Select(q => q);
            foreach (var quote in list)
            {
                Console.Out.WriteLine(quote.ToString());
            }

            Console.ReadKey();
        }
    }
}
