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
using LiveCharts;
using StockMonitor;
using StockMonitor.Helpers;
using StockMonitor.Models.ApiModels;
using StockMonitor.Models.UIClasses;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace GUI
{
    /// <summary>
    /// Interaction logic for SearchStockUserControl.xaml
    /// </summary>
    public partial class SearchStockUserControl : UserControl
    {
        List<Task<UIComapnyRow>> taskList;

        BlockingCollection<UIComapnyRow> companyDataRowList;
        BlockingCollection<UIComapnyRow> watchList;

        DateTime start, end;

        private const int RealTimeInterval = 3000;
        private const int OneMinTimeInterval = 3000;
        private const int CurrentUserId = 3;

        public SearchStockUserControl()
        {
            start = DateTime.Now;

            string[] companyNames =
            {
                "AAPL", "AMZN", "GOOG", "FB", "AAXN", "MSFT",
                "T", "VZ", "GM", "OKE", "IRBT", "LULU", "NFLX", "STZ"
            };

            taskList = new List<Task<UIComapnyRow>>();

            List<Task<UIComapnyRow>> uiCompanyRowTaskList = GUIDataHelper.GetWatchUICompanyRowTaskList(CurrentUserId);
            Task watchlistTask = InitWatchListTaskList(uiCompanyRowTaskList);

            foreach (string symbol in companyNames)
            {
                taskList.Add(GUIDataHelper.GetCompanyDataRowTask(symbol));
            }

            Task mainListTask = InitListView();

            InitializeComponent();



            LoopRefreshData(mainListTask, watchlistTask);

            



        }



        private void LoopRefreshData(Task mainListTask, Task watchlistTask)
        {
            Task.WhenAll(mainListTask, watchlistTask).ContinueWith(p =>
            {
                foreach (var companyRow in companyDataRowList)
                {
                    Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            while (true)
                            {
                                RefreshRealTImePrice(companyRow);
                                //Thread.Sleep(RealTimeInterval);
                                await Task.Delay(RealTimeInterval);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.Out.WriteLine(
                                $"Mainwindow realtimeprice loop thread exception {companyRow.Symbol} at {DateTime.Now}");
                        }
                    });

                    Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            while (true)
                            {
                                Refresh1MinData(companyRow);
                                await Task.Delay(OneMinTimeInterval);
                                //Thread.Sleep(OneMinTimeInterval);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.Out.WriteLine(
                                $"1Mindata loop thread exception {companyRow.Symbol} at {DateTime.Now}");
                        }
                    });
                }

                foreach (var uiComapnyRow in watchList)
                {
                    Task.Factory.StartNew(async () =>
                    {
                        while (true)
                        {
                            try
                            {
                                RefreshRealTImePrice(uiComapnyRow);
                                //Thread.Sleep(RealTimeInterval);
                                await Task.Delay(RealTimeInterval);
                            }
                            catch (Exception ex)
                            {
                                Console.Out.WriteLine(
                                    $"Watchlist loop thread exception {uiComapnyRow.Symbol} at {DateTime.Now}");
                            }
                        }
                    });
                }
            });
        }

        private async Task InitWatchListTaskList(List<Task<UIComapnyRow>> uiCompanyRowTaskList)
        {
            watchList = new BlockingCollection<UIComapnyRow>();
            foreach (Task<UIComapnyRow> task in uiCompanyRowTaskList)
            {
                UIComapnyRow comapnyRow = await task;
                watchList.Add(comapnyRow);
            }

            lsvWatchList.ItemsSource = watchList;
        }

        private async void Refresh1MinData(UIComapnyRow comapnyRow)
        {
            try
            {
                List<Fmg1MinQuote> quote1MinList =
                    await RetrieveJsonDataHelper.RetrieveAllFmg1MinQuote(comapnyRow.Symbol);
                if (quote1MinList.Count > 0)
                {
                    comapnyRow.Volume = quote1MinList[0].Volume;

                    /**************************************************
                    following line simulate Volume change during close hours.
                    ****************************************************/
                    comapnyRow.Volume += new Random().Next(1000) * 10000;
                }
            }
            catch (SystemException ex)
            {
                Console.Out.WriteLine($"\n!!! Refresh1Mindata exception for {comapnyRow.Symbol} at {DateTime.Now}");
            }
        }

        private async void RefreshRealTImePrice(UIComapnyRow comapnyRow)
        {
            try
            {
                FmgQuoteOnlyPrice quote = await RetrieveJsonDataHelper.RetrieveFmgQuoteOnlyPrice(comapnyRow.Symbol);

                /**************************************************
                    following line simulate Volume change during close hours.
                ****************************************************/
                quote.Price += new Random().NextDouble() * quote.Price / 20;

                if (Math.Abs(comapnyRow.Price - quote.Price) < 0.001)
                {
                    Console.Out.WriteLine(
                        $"{comapnyRow.Symbol} No change, old: {comapnyRow.Price}, new: {quote.Price}, {DateTime.Now}");
                }
                else
                {
                    Console.Out.WriteLine(
                        $"{comapnyRow.Symbol} Price CHANGEd, old: {comapnyRow.Price}, new: {quote.Price}, {DateTime.Now}");
                    comapnyRow.Price = quote.Price;
                    double change = comapnyRow.Price - comapnyRow.Open;
                    double changePercentage = change / comapnyRow.Open * 100;
                    comapnyRow.PriceChange = change;
                    comapnyRow.ChangePercentage = changePercentage;
                    if ((comapnyRow.NotifyPriceHigh != 0 && comapnyRow.Price > comapnyRow.NotifyPriceHigh))
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            notifier.ShowSuccess($"Higher price warning: {comapnyRow.Symbol}, target high: {comapnyRow.NotifyPriceHigh:N2}, current: {comapnyRow.Price:N2} on {DateTime.Now:HH:mm:ss}");
                        });
                    }

                    if ((comapnyRow.NotifyPriceLow != 0 && comapnyRow.Price < comapnyRow.NotifyPriceLow))
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            notifier.ShowWarning($"Lower price warning: {comapnyRow.Symbol}, target low: {comapnyRow.NotifyPriceLow:N2}, current: {comapnyRow.Price:N2} on {DateTime.Now:HH:mm:ss}");
                        });
                    }
                }
            }
            catch (SystemException ex)
            {
                Console.Out.WriteLine(
                    $"\n!!! RefreshRealtimePrice exception for {comapnyRow.Symbol} at {DateTime.Now} for {ex.Message}");
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
            lsvMarketPreview.ItemsSource = companyDataRowList;

            end = DateTime.Now;
            TimeSpan timeSpan = new TimeSpan();
            timeSpan = end - start;
            Console.WriteLine("##############Total time:{0} milli####################", timeSpan);
        }


        private void LsvWatch_miAddToWatchList_OnClick(object sender, RoutedEventArgs e)
        {
            tbSearchBox.Focus();
        }

        private void LsvWatch_miDeleteFromWatchList_OnClick(object sender, RoutedEventArgs e)
        {
            var item = lsvWatchList.SelectedItem;
            if (item != null)
            {
                UIComapnyRow companyRow = item as UIComapnyRow;
                try
                {
                    Task t = GUIDataHelper.DeleteFromWatchListTask(CurrentUserId, companyRow.CompanyId);
                    Task.WhenAll(t).ContinueWith(p =>
                    {
                        List<UIComapnyRow> tempList = new List<UIComapnyRow>(watchList);
                        tempList.Remove(companyRow);
                        watchList = new BlockingCollection<UIComapnyRow>(new ConcurrentBag<UIComapnyRow>(tempList));

                        // MessageBox.Show("Watchlist count left: "  + watctList.Count.ToString());

                        this.Dispatcher.Invoke(() =>
                        {
                            lsvWatchList.ItemsSource = watchList;


                        });
                        MessageBox.Show($"after delete, view: {lsvWatchList.Items.Count}, list:{watchList.Count}");
                    });
                }
                catch (SystemException ex)
                {
                    Console.Out.WriteLine($"!!! Delete item from watchlist failed: {ex.Message}");
                }
            }
        }

        private void LsvMkt_miAddToWatchList_OnClick(object sender, RoutedEventArgs e)
        {
            var item = lsvMarketPreview.SelectedItem;
            if (item != null)
            {
                UIComapnyRow comapnyRow = item as UIComapnyRow;
                try
                {
                    int companyId = comapnyRow.CompanyId;
                    Task t = GUIDataHelper.AddItemToWatchListTast(CurrentUserId, companyId);

                    Task.WhenAll(t).ContinueWith(p =>   //FIXME
                    {
                        watchList.Add(comapnyRow);
                        List<UIComapnyRow> tempList = new List<UIComapnyRow>(watchList);
                        watchList = new BlockingCollection<UIComapnyRow>(new ConcurrentBag<UIComapnyRow>(tempList));

                        this.Dispatcher.Invoke(() =>
                        {
                            lsvWatchList.ItemsSource = watchList;

                        });
                       MessageBox.Show($"after add, view: {lsvWatchList.Items.Count}, list:{watchList.Count}");
                    });
                }
                catch (SystemException ex)
                {
                    Console.Out.WriteLine($"!!! Add item from watchlist failed: {ex.Message}");
                }
            }
        }

        private void TbSearchBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (tbSearchBox.Text == "Search symbol here")
            {
                tbSearchBox.Text = "";
            }
        }


        private void TbSearchBox_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            string searchSymbol = tbSearchBox.Text;

            if (e.Key == Key.Enter)
            {
                List<UIComapnyRow> searchComapnyRowList = new List<UIComapnyRow>();
                Task t = Task.Run(() =>
                {
                    List<Company> companyList = GUIDataHelper.GetSearchCompanyListTask(searchSymbol).Result;
                    foreach (Company comapny in companyList)
                    {
                        Task<UIComapnyRow> subTask = GUIDataHelper.GetCompanyDataRowTask(comapny.Symbol);
                        searchComapnyRowList.Add(subTask.Result);
                    }
                });
                Task.WhenAll(t).ContinueWith(p =>
                {
                    this.Dispatcher.Invoke(() => { lsvMarketPreview.ItemsSource = searchComapnyRowList; });
                });
                tbSearchBox.Text = "Search symbol here";
                lbSearchResult.Visibility = Visibility.Hidden;
            }
            else
            {
                Task.Run(() =>
                {
                    List<Company> companyList = GUIDataHelper.GetSearchCompanyListTask(searchSymbol).Result;
                    if (companyList != null)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            lbSearchResult.ItemsSource = companyList;
                            lbSearchResult.Height = companyList.Count * 25;
                            lbSearchResult.Visibility = Visibility.Visible;
                        });
                    }
                    else
                    {
                        this.Dispatcher.Invoke(() => { lbSearchResult.Visibility = Visibility.Hidden; });
                    }
                });
            }
        }


        private Notifier notifier = new Notifier(cfg =>
        {
            cfg.PositionProvider = new WindowPositionProvider(
                parentWindow: Application.Current.MainWindow,
                corner: Corner.BottomRight,
                offsetX: 5,
                offsetY: 5);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(3),
                maximumNotificationCount: MaximumNotificationCount.FromCount(5));

            cfg.Dispatcher = Application.Current.Dispatcher;
        });
        
        
        private void BtClearSearch_OnClick(object sender, RoutedEventArgs e)
        {
            tbSearchBox.Text = "Search symbol here";
            Task.Run(() => { this.Dispatcher.Invoke(() => { lsvMarketPreview.ItemsSource = companyDataRowList; }); });
        }

        private void LsvWatch_SetTargetPrice_OnClick(object sender, RoutedEventArgs e)
        {
            var item = lsvMarketPreview.SelectedItem;
            if (item != null)
            {
                UIComapnyRow companyRow = item as UIComapnyRow;
                SetTargetPriceNotificationDialog priceDialgo = new SetTargetPriceNotificationDialog(companyRow);
                if (priceDialgo.ShowDialog() == true)
                {

                }
            }
        }

        private void lsvMarketPreview_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            UIComapnyRow selCompany = (UIComapnyRow)lsvMarketPreview.SelectedItem;

            if(selCompany == null) { return; }

            RealTimePriceChart realTimeChart = new RealTimePriceChart(selCompany);

            realTimeChart.ShowDialog();
        }

        private void LsvMkt_miSetTargetPrice_OnClick(object sender, RoutedEventArgs e)
        {
            var item = lsvMarketPreview.SelectedItem;
            if (item != null)
            {
                UIComapnyRow companyRow = item as UIComapnyRow;
                SetTargetPriceNotificationDialog priceDialgo = new SetTargetPriceNotificationDialog(companyRow);
                priceDialgo.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (priceDialgo.ShowDialog() == true)
                {
                
                   
                         
                   
                }
            }
        }
    }



}
