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

namespace TestUIMain
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Task<UIComapnyRow>> taskList;

        BlockingCollection<UIComapnyRow> companyDataRowList;

        DateTime start, end;
        public MainWindow()
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


            Task.WhenAll(t).ContinueWith(p =>
            {
                foreach (var companyRow in companyDataRowList)
                {
                    Task.Factory.StartNew(() =>
                    {
                        while (true)
                        {
                            RefreshPriceBySymbol(companyRow);
                            Thread.Sleep(3000);
                            this.Dispatcher.Invoke(() => { lsvMarketPreview.Items.Refresh(); });
                        }
                    });
                }
            });



            /*   DateTime end = DateTime.Now;
               TimeSpan timeSpan = new TimeSpan();
               timeSpan = end - start;
               MessageBox.Show($"Loading time: {timeSpan.TotalMilliseconds} mills");*/
        }


        private async void RefreshPriceBySymbol(UIComapnyRow comapnyRow)
        {
            FmgQuoteOnlyPrice quote = await RetrieveJsonDataHelper.RetrieveFmgQuoteOnlyPrice(comapnyRow.Symbol);
            List<Fmg1MinQuote> quote1MinList = await RetrieveJsonDataHelper.RetrieveAllFmg1MinQuote(comapnyRow.Symbol);

            // Console.Out.WriteLine($"{comapnyRow.Symbol}, old: {comapnyRow.Price}, new: {quote.Price}, {DateTime.Now}");
            // quote.Price += new Random().NextDouble();
            if (Math.Abs(comapnyRow.Price - quote.Price) < 0.001)
            {
                Console.Out.WriteLine($"{comapnyRow.Symbol} No change, old: {comapnyRow.Price}, new: {quote.Price}, {DateTime.Now}");
            }
            else
            {
                Console.Out.WriteLine($"{comapnyRow.Symbol} CHANGE, old: {comapnyRow.Price}, new: {quote.Price}, {DateTime.Now}, {quote1MinList[0].Volume}");
                comapnyRow.Price = quote.Price;
                double change = comapnyRow.Price - comapnyRow.Open;
                double changePercentage = (change / comapnyRow.Open) / comapnyRow.Open * 100;
                comapnyRow.PriceChange = change;
                comapnyRow.ChangePercentage = changePercentage;
                if (quote1MinList.Count > 0)
                {
                    comapnyRow.Volume = quote1MinList[0].Volume;
                }
            }
        }




        private async Task InitListView()
        {
            companyDataRowList = new BlockingCollection<UIComapnyRow>();

            foreach (Task<UIComapnyRow> task in taskList)
            {
                try
                {
                    UIComapnyRow company = await task;
                    companyDataRowList.Add(company);
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
