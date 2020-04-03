using LiveCharts.Defaults;
using StockMonitor.Models.ApiModels;
using StockMonitor.Models.UIClasses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    public class Wrapper
    {
        public class Fmg1MinQuoteWapper : OhlcPoint
        {
            public Fmg1MinQuoteWapper(Fmg1MinQuote fmg1MinQuote)
            {
                Open = fmg1MinQuote.Open;
                Low = fmg1MinQuote.Low;
                High = fmg1MinQuote.High;
                Close = fmg1MinQuote.Close;
                Date = fmg1MinQuote.Date;
            }

            public DateTime Date { get; set; }
            public long Volume { get; set; }
        }

        public class UICompanyRowWraper
        {
            public UIComapnyRow Company { get; set; }

            public double MarketCapital { get; set; }
            public double PriceToEarningRatio { get; set; }
            public double PriceToSalesRatio { get; set; }
            public UICompanyRowWraper(UIComapnyRow company)//ex FormatException
            {
                Company = company;
                MarketCapital = double.Parse(company.MarketCapital, CultureInfo.InvariantCulture);//ex
                PriceToEarningRatio = double.Parse(company.PriceToEarningRatio, CultureInfo.InvariantCulture);//ex
                PriceToSalesRatio = double.Parse(company.PriceToSalesRatio, CultureInfo.InvariantCulture);//ex
            }
        }
    }
}
