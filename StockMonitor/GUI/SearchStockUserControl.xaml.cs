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


        public BlockingCollection<string> companyNames = new BlockingCollection<string>() {
            "AAPL", "AMZN", "GOOG", "FB", "AAXN", "MSFT",
            "T", "VZ", "GM", "OKE", "IRBT", "LULU", "NFLX"
        };

        DateTime start, end;

        private const int RealTimeInterval = 3000;
        private const int OneMinTimeInterval = 6000;
        private const int CurrentUserId = 3;

        public SearchStockUserControl()
        {

            InitializeComponent();


            InitListViewDataSource();



        }




        private void InitListViewDataSource()
        {
            GlobalVariables.DefaultUICompanyRows = new BlockingCollection<UIComapnyRow>();
            GlobalVariables.DefaultTaskTokenSource = new CancellationTokenSource();
            GlobalVariables.WatchListUICompanyRows = new BlockingCollection<UIComapnyRow>();
            GlobalVariables.WatchListTokenSourceDic = new ConcurrentDictionary<string, CancellationTokenSource>();

            Task.Run(() => LoadAndRefreshWatchListManager(CurrentUserId));
            Task.Run(() => LoadAndRefreshDefaultListManager());
        }

        private async void LoadAndRefreshWatchListManager(int currentUserId)
        {
            GlobalVariables.WatchListUICompanyRows = new BlockingCollection<UIComapnyRow>();
            List<Task<UIComapnyRow>> watchlistTasks = GUIDataHelper.GetWatchUICompanyRowTaskList(currentUserId);
            foreach (var task in watchlistTasks)
            {
                UIComapnyRow oneRow = await task;
                GlobalVariables.WatchListUICompanyRows.TryAdd(oneRow);
            }

            await Task.WhenAll(watchlistTasks.ToArray());

            this.Dispatcher.Invoke(() =>
            {
                lsvWatchList.ItemsSource = GlobalVariables.WatchListUICompanyRows.ToList();

                Console.Out.WriteLine($"{lsvMarketPreview.Items.Count}:{GlobalVariables.DefaultUICompanyRows.Count}");
            });

            foreach (var watchUICompanyRow in GlobalVariables.WatchListUICompanyRows)
            {
                Task.Run(() => LoadAndRefreshWatchListRow(watchUICompanyRow));
            }
        }



        private void LoadAndRefreshDefaultListManager()
        {

            foreach (string symbol in companyNames)
            {
                Task.Run(
                    async () => await LoadAndRefreshDefaultRow(symbol), GlobalVariables.DefaultTaskTokenSource.Token);
            }
        }

        private  async Task LoadAndRefreshDefaultRow(string symbol)
        {
            try
            {
                UIComapnyRow companyRow = await GUIDataHelper.GetUICompanyRowTaskBySymbol(symbol);
                GlobalVariables.DefaultUICompanyRows.TryAdd(companyRow);

                Console.Out.WriteLine(
                    $"\n%%%%% total:  {GlobalVariables.DefaultUICompanyRows.Count}, waited a task: " +
                    companyRow.ToString());
                this.Dispatcher.Invoke(() =>
                {
                    lsvMarketPreview.ItemsSource = GlobalVariables.DefaultUICompanyRows.ToList();

                    Console.Out.WriteLine($"{lsvMarketPreview.Items.Count}:{GlobalVariables.DefaultUICompanyRows.Count}");
                });

                 Task.Run(async () =>
                {
                    while (!GlobalVariables.DefaultTaskTokenSource.IsCancellationRequested)
                    {
                        RefreshRealTImePrice(companyRow);
                        await Task.Delay(RealTimeInterval);
                        Console.Out.WriteLine(
                            $"\n%%%total:  {GlobalVariables.DefaultUICompanyRows.Count}, refresh real data with cancel {GlobalVariables.DefaultTaskTokenSource.IsCancellationRequested}, id:{Thread.CurrentThread.ManagedThreadId},: {companyRow.ToString()}");
                    }
                }, GlobalVariables.DefaultTaskTokenSource.Token);

                 Task.Run(async () =>
                {
                    while (!GlobalVariables.DefaultTaskTokenSource.IsCancellationRequested)
                    {
                        Refresh1MinData(companyRow);
                        await Task.Delay(OneMinTimeInterval);
                        Console.Out.WriteLine(
                            $"\n >>>>>>total:  {GlobalVariables.DefaultUICompanyRows.Count}, refresh 1Min with cancel {GlobalVariables.DefaultTaskTokenSource.IsCancellationRequested},id:{Thread.CurrentThread.ManagedThreadId},: {companyRow.ToString()}");
                    }
                }, GlobalVariables.DefaultTaskTokenSource.Token);
            }
            catch (SystemException ex)
            {
                Console.Out.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: default task was cancelled! {ex.Message}");
            }
        }

        private void LoadAndRefreshWatchListRow(UIComapnyRow companyRow)
        {
            try
            {
                Console.Out.WriteLine(
                    $"\n%%%%% total:  {GlobalVariables.WatchListUICompanyRows.Count}, waited a task: " + companyRow.Symbol);


                CancellationTokenSource watchListRowSource = new CancellationTokenSource();
                GlobalVariables.WatchListTokenSourceDic.TryAdd(companyRow.Symbol, watchListRowSource);
                Task.Run(async () =>
                {
                    int curThreadId = Thread.CurrentThread.ManagedThreadId;


                    Console.Out.WriteLine($"\n&&&&&&&&&&&&&&&&&&&&& Add token source for watchlist, REAL time: {curThreadId}:{companyRow.Symbol}");

                    while (!watchListRowSource.IsCancellationRequested)
                    {
                        RefreshRealTImePrice(companyRow);
                        Console.Out.WriteLine($"\n REFRESH WATCHLIST REAL TIME: {curThreadId}:{companyRow.Symbol} {companyRow.Price}\n");
                        await Task.Delay(RealTimeInterval);
                        Console.Out.WriteLine(
                            $"\n%%%total:  {GlobalVariables.WatchListUICompanyRows.Count}, refresh Watch list real data with cancel {watchListRowSource.IsCancellationRequested}, id:{curThreadId},: {companyRow.ToString()}");
                    }
                }, watchListRowSource.Token);


                Task.Run(async () =>
               {
                   int curThreadId = Thread.CurrentThread.ManagedThreadId;

                   Console.Out.WriteLine($"\n&&&&&&&&&&&&&&&&&&&&& Add token source for watchlist, One min: {curThreadId}:{companyRow.Symbol}");
                   while (!(watchListRowSource.IsCancellationRequested))
                   {
                       Refresh1MinData(companyRow);
                       Console.Out.WriteLine($"\n REFRESH WATCHLIST One Min: {curThreadId}:{companyRow.Symbol} {companyRow.Price}\n");
                       await Task.Delay(OneMinTimeInterval);
                       Console.Out.WriteLine(
                           $"\n >>>>>>total:  {GlobalVariables.WatchListUICompanyRows.Count}, refresh Watch list 1Min with cancel {watchListRowSource.IsCancellationRequested},id:{curThreadId},: {companyRow.ToString()}");
                   }
               }, watchListRowSource.Token);

                /* oneMinCancellationTokenSource.Dispose();
                 realTimeCancellationTokenSource.Dispose();*/
            }
            catch (SystemException ex)
            {
                Console.Out.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: default task was cancelled! {ex.Message}");
            }
        }

        private async void Refresh1MinData(UIComapnyRow comapnyRow)
        {
            try
            {
                List<Fmg1MinQuote> quote1MinList = await RetrieveJsonDataHelper.RetrieveAllFmg1MinQuote(comapnyRow.Symbol);
                if (quote1MinList.Count > 0)
                {
                    comapnyRow.Volume = quote1MinList[0].Volume;
                }

                /**************************************************
                       following line simulate Volume change during close hours.
                       ****************************************************/
                comapnyRow.Volume += new Random().Next(50) * 1000;
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
                FmgQuoteOnlyPrice quote;
                if (!GlobalVariables.IsPseudoDataSource)
                {
                    quote = await RetrieveJsonDataHelper.RetrieveFmgQuoteOnlyPrice(comapnyRow.Symbol);
                }
                else
                {
                    quote = new FmgQuoteOnlyPrice() { Symbol = comapnyRow.Symbol, Price = comapnyRow.Price };
                }

                /**************************************************
                    following line simulate Price change during close hours.
                ****************************************************/
                Random rand = new Random();
                int randDirection = rand.Next(2) == 1 ? -1 : 1;
                quote.Price += rand.NextDouble() * randDirection * quote.Price / 50;

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
                        List<UIComapnyRow> tempList = GlobalVariables.WatchListUICompanyRows.ToList();
                        tempList.Remove(companyRow);

                        GlobalVariables.WatchListUICompanyRows = new BlockingCollection<UIComapnyRow>(new ConcurrentQueue<UIComapnyRow>(tempList));

                        this.Dispatcher.Invoke(() =>
                        {
                            lsvWatchList.ItemsSource = GlobalVariables.WatchListUICompanyRows;
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
                    List<UIComapnyRow> tempList = GlobalVariables.WatchListUICompanyRows.ToList();
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
                            GlobalVariables.WatchListUICompanyRows = new BlockingCollection<UIComapnyRow>(new ConcurrentQueue<UIComapnyRow>(tempList));

                            this.Dispatcher.Invoke(() =>
                            {
                                lsvWatchList.ItemsSource = GlobalVariables.WatchListUICompanyRows;

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
                tbSearchBox.Text = "";
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
            Task.Run(() => { this.Dispatcher.Invoke(() => { lsvMarketPreview.ItemsSource = GlobalVariables.DefaultUICompanyRows; }); });
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

        CancellationTokenSource chartTokenSource;
        private void lsvMarketPreview_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            UIComapnyRow selCompany = (UIComapnyRow)lsvMarketPreview.SelectedItem;

            if (selCompany == null) { return; }

            chartTokenSource = new CancellationTokenSource();

            try
            {
                RealTimePriceChart realTimeChart = new RealTimePriceChart(selCompany, chartTokenSource.Token);

                realTimeChart.ShowDialog();
            } catch (OperationCanceledException)
            {
                Console.WriteLine("RealTime Chart canceled");
                chartTokenSource.Dispose();

            }
        }

        private void btCancelDefaultThreads_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariables.DefaultTaskTokenSource.Cancel(true);
        }

        private void btCancelWatchlistThreads_Click(object sender, RoutedEventArgs e)
        {
            foreach (KeyValuePair<string, CancellationTokenSource> keyValuePair in GlobalVariables.WatchListTokenSourceDic)
            {
                keyValuePair.Value.Cancel(true);
            }
        }

        private void btRestartRefresh_Click(object sender, RoutedEventArgs e)
        {
            InitListViewDataSource();
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
