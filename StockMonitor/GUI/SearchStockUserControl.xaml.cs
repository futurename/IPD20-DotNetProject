using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StockMonitor.Helpers;
using StockMonitor.Models.ApiModels;
using StockMonitor.Models.UIClasses;
using static GUI.Wrapper;

namespace GUI
{
    /// <summary>
    /// Interaction logic for SearchStockUserControl.xaml
    /// </summary>
    public partial class SearchStockUserControl : UserControl
    {
        List<Task<UIComapnyRow>> taskList;

        BlockingCollection<UICompanyRowWrapper> companyDataRowList;

        DateTime start, end;

        private const int RealTimeInterval = 3000;
        private const int OneMinTimeInterval = 60000;

        public SearchStockUserControl()
        {
            start = DateTime.Now;

            string[] companyNames =
            {"VXUS", "AAPL", "AMZN", "GOOG", "BA", "LTM", "FB", "AAXN", "MSFT",
                "T", "VZ", "GM", "OKE", "TERP", "IRBT", "LULU", "W", "NFLX", "NSYS", "STZ" };

            taskList = new List<Task<UIComapnyRow>>();
            foreach (string name in companyNames)
            {
                taskList.Add(GUIHelper.GetCompanyDataRowTask(name));
            }
            Task t = SetListView();
            InitializeComponent();

            int counter = 0;

            Task.WhenAll(t).ContinueWith(p =>
            {
                foreach (var companyRowWrapper in companyDataRowList)
                {
                    Task.Factory.StartNew(() =>
                    {
                        while (true)
                        {
                            RefreshRealTImePrice(companyRowWrapper.Company);
                            Thread.Sleep(RealTimeInterval);
                        }
                    });

                    Task.Factory.StartNew(() =>
                    {
                        while (true)
                        {
                            Refresh1MinData(companyRowWrapper.Company);
                            Thread.Sleep(OneMinTimeInterval);
                        }
                    });
                }
            });



            /*   DateTime end = DateTime.Now;
               TimeSpan timeSpan = new TimeSpan();
               timeSpan = end - start;
               MessageBox.Show($"Loading time: {timeSpan.TotalMilliseconds} mills");*/
        }

        private async void Refresh1MinData(UIComapnyRow comapnyRow)
        {
            List<Fmg1MinQuote> quote1MinList = await RetrieveJsonDataHelper.RetrieveAllFmg1MinQuote(comapnyRow.Symbol);
            if (quote1MinList.Count > 0)
            {
                comapnyRow.Volume = quote1MinList[0].Volume;

                /**************************************************
                following line simulate Volume change during close hours.
                ****************************************************/
                comapnyRow.Volume += new Random().Next(10000) * 1000;
            }
        }

        private async void RefreshRealTImePrice(UIComapnyRow comapnyRow)
        {
            FmgQuoteOnlyPrice quote = await RetrieveJsonDataHelper.RetrieveFmgQuoteOnlyPrice(comapnyRow.Symbol);

            /**************************************************
                following line simulate Volume change during close hours.
            ****************************************************/
            quote.Price += new Random().NextDouble() * quote.Price / 10;

            if (Math.Abs(comapnyRow.Price - quote.Price) < 0.001)
            {
                Console.Out.WriteLine($"{comapnyRow.Symbol} No change, old: {comapnyRow.Price}, new: {quote.Price}, {DateTime.Now}");
            }
            else
            {
                Console.Out.WriteLine($"{comapnyRow.Symbol} CHANGE, old: {comapnyRow.Price}, new: {quote.Price}, {DateTime.Now}");
                comapnyRow.Price = quote.Price;
                double change = comapnyRow.Price - comapnyRow.Open;
                double changePercentage = (change / comapnyRow.Open) / comapnyRow.Open * 100;
                comapnyRow.PriceChange = change;
                comapnyRow.ChangePercentage = changePercentage;

            }
        }




        private async Task InitListView()
        {
            companyDataRowList = new BlockingCollection<UICompanyRowWrapper>();

            foreach (Task<UIComapnyRow> task in taskList)
            {
                try
                {
                    UIComapnyRow company = await task;
                    companyDataRowList.Add(new UICompanyRowWrapper(company));
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Console.Out.WriteLine("!!!!! Failed: " + ex.Message);
                }
                catch (SystemException ex)
                {
                    Console.Out.WriteLine("!!!! system exception " + ex.Message);
                }
            }
            //lsvMarketPreview.ItemsSource = companyDataRowList;
        }

        private async Task SetListView()
        {

            await Task.Run(InitListView);
            lsvMarketPreview.ItemsSource = companyDataRowList;


            end = DateTime.Now;
            TimeSpan timeSpan = new TimeSpan();
            timeSpan = end - start;
            Console.WriteLine("##############Total time:{0} milli####################", timeSpan);

        }
    }
}
