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
using Org.BouncyCastle.Math.EC.Endo;
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
        

        DateTime start, end;

        private const int RealTimeInterval = 3000;
        private const int OneMinTimeInterval = 3000;
        private const int CurrentUserId = 1;
        BlockingCollection<string> allRunningSymbolList = new BlockingCollection<string>();

        public SearchStockUserControl()
        {
            start = DateTime.Now;

            string[] companyNames =
            {
                "AAPL", "AMZN", "GOOG", "FB", "AAXN", "MSFT",
                "T", "VZ", "GM", "OKE", "IRBT", "LULU", "NFLX"
            };
            


            foreach (var symbol in companyNames)
            {
                allRunningSymbolList.Add(symbol);
            }

            taskList = new List<Task<UIComapnyRow>>();

            foreach (string symbol in companyNames)
            {
                taskList.Add(GUIDataHelper.GetUICompanyRowTaskBySymbol(symbol));
            }

            Task mainListTask = InitListView();

            List<Task<UIComapnyRow>> uiCompanyRowTaskList = GUIDataHelper.GetWatchUICompanyRowTaskList(CurrentUserId);
            Task watchlistTask = InitWatchListTaskList(uiCompanyRowTaskList);

            InitUICompanyRowsManager();


            InitializeComponent();

           

            LoopRefreshData(mainListTask, watchlistTask);

           


        }


        private void InitUICompanyRowsManager()
        {
            List<string> companyNames = new List<string>
            {
                "AAPL", "AMZN", "GOOG", "FB", "AAXN", "MSFT",
                "T", "VZ", "GM", "OKE", "IRBT", "LULU", "NFLX"
            };

            foreach (var symbol in companyNames)
            {
                Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        UIComapnyRow companyRow = await GUIDataHelper.GetUICompanyRowTaskBySymbol(symbol);
                        

                    }
                    catch (SystemException ex)
                    {
                        MessageBox.Show("Init defalut task manager failed:" + ex.Message);
                    }

                });
            }
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
                             /*   int threadId = Thread.CurrentThread.ManagedThreadId;

                                GlobalVariables.taskManager.TryAdd(threadId, companyRow.Symbol);
                                    
                                Console.Out.WriteLine($"\n&&&& current id: {threadId}");
                                foreach (KeyValuePair<int, string> pair in GlobalVariables.taskManager)
                                {
                                    Console.Out.WriteLine($"All threads running traverse: {pair.Key}: {pair.Value}, total: {GlobalVariables.taskManager.Count}");
                                }*/


                                RefreshRealTImePrice(companyRow);
                                //Thread.Sleep(RealTimeInterval);
                                await Task.Delay(RealTimeInterval);
                                Console.Out.WriteLine("\n&&&& current thread managed id: " + Thread.CurrentThread.ManagedThreadId);
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

                foreach (var uiComapnyRow in GlobalVariables.watchList)
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
            GlobalVariables.watchList = new BlockingCollection<UIComapnyRow>();
            foreach (Task<UIComapnyRow> task in uiCompanyRowTaskList)
            {
                UIComapnyRow comapnyRow = await task;
                GlobalVariables.watchList.Add(comapnyRow);
            }

            lsvWatchList.ItemsSource = GlobalVariables.watchList;
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
                quote.Price += new Random().NextDouble()* (new Random().Next(2) == 1? -1 : 1) * quote.Price / 40;

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
                            notifier.ShowSuccess($"Higher price warning:\n{comapnyRow.CompanyName} : {comapnyRow.Symbol} \nNow: {comapnyRow.Price:N2} Target high: { comapnyRow.NotifyPriceHigh:N2}\nTime: { DateTime.Now:HH: mm: ss}");
                            });
                    }

                    if ((comapnyRow.NotifyPriceLow != 0 && comapnyRow.Price < comapnyRow.NotifyPriceLow))
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            notifier.ShowError($"Lower price warning:\n{comapnyRow.CompanyName} : {comapnyRow.Symbol} \nNow: {comapnyRow.Price:N2} Target low: {comapnyRow.NotifyPriceLow:N2}\nTime: {DateTime.Now:HH:mm:ss}");
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
                        List<UIComapnyRow> tempList = GlobalVariables.watchList.ToList();
                        tempList.Remove(companyRow);

                        GlobalVariables.watchList = new BlockingCollection<UIComapnyRow>(new ConcurrentQueue<UIComapnyRow>(tempList));

                        this.Dispatcher.Invoke(() =>
                        {
                            lsvWatchList.ItemsSource = GlobalVariables.watchList;
                        });
                        //MessageBox.Show($"after delete, view: {lsvWatchList.Items.Count}, list:{watchList.Count}");
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
                    List<UIComapnyRow> tempList = GlobalVariables.watchList.ToList();
                    if (tempList.Find(row => row.Symbol == comapnyRow.Symbol) != null)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"{comapnyRow.Symbol} EXISTS in the watchlist", "Duplicate company",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                        return;
                    }
                    else
                    {
                        int companyId = comapnyRow.CompanyId;
                        Task t = GUIDataHelper.AddItemToWatchListTast(CurrentUserId, companyId);

                        Task.WhenAll(t).ContinueWith(p =>
                        {
                            tempList.Add(comapnyRow);
                            GlobalVariables.watchList = new BlockingCollection<UIComapnyRow>(new ConcurrentQueue<UIComapnyRow>(tempList));

                            this.Dispatcher.Invoke(() =>
                            {
                                lsvWatchList.ItemsSource = GlobalVariables.watchList;

                            });
                            Task.Factory.StartNew(async () =>
                            {
                                while (true)
                                {
                                    try
                                    {
                                        RefreshRealTImePrice(comapnyRow);
                                        //Thread.Sleep(RealTimeInterval);
                                        await Task.Delay(RealTimeInterval);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.Out.WriteLine(
                                            $"Watchlist loop thread exception {comapnyRow.Symbol} at {DateTime.Now}");
                                    }
                                }
                            });

                            // MessageBox.Show($"after add, view: {lsvWatchList.Items.Count}, list:{watchList.Count}");
                        });
                    }
                }
                catch (SystemException ex)
                {
                    Console.Out.WriteLine($"!!! Add item from watchlist failed: {ex.Message}");
                }
            }
        }

        private void TbSearchBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            tbSearchBox.Text = "";
        }
        private void tbSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            tbSearchBox.Text = "Search symbol here";
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
                        Task<UIComapnyRow> subTask = GUIDataHelper.GetUICompanyRowTaskBySymbol(comapny.Symbol);
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

            if (selCompany == null) { return; }

            RealTimePriceChart realTimeChart = new RealTimePriceChart(selCompany);

            realTimeChart.ShowDialog();
        }


        private void LsvMkt_miSetTargetPrice_OnClick(object sender, RoutedEventArgs e)
        {
            var item = lsvMarketPreview.SelectedItem;
            if (item != null)
            {
                UIComapnyRow companyRow = item as UIComapnyRow;
                SetTargetPriceNotificationDialog priceDialog = new SetTargetPriceNotificationDialog(companyRow);
                priceDialog.Owner = Application.Current.MainWindow;
                priceDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (priceDialog.ShowDialog() == true)
                {
                    
                }
            }
        }
    }



}
