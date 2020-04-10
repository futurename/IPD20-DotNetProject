using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using Newtonsoft.Json;
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
        
        public BlockingCollection<string> companyNames = new BlockingCollection<string>()
        {
            "AAPL", "AMZN", "GOOG", "FB", "AAXN", "MSFT",
            "T", "VZ", "GM", "OKE", "IRBT", "NFLX"
        };
        

        private const int RealTimeInterval = 3000;
        private const int OneMinTimeInterval = 6000;
        private const int CurrentUserId = 3;

        StockTrader StockTrader { get; set; }
        public SearchStockUserControl()
        {
            InitListViewDataSource();

            InitializeComponent();
            GlobalVariables.IsPseudoDataSource = tgbDataSourceSwitch.IsChecked == true;
            StockTrader = new StockTrader(CurrentUserId);
            StockTrader.StartTrade();
            GlobalVariables.Notifier = notifier;
            GlobalVariables.SearchStockUserControl = this; 
        }




        private void InitListViewDataSource()
        {
            GlobalVariables.DefaultUICompanyRows = new BlockingCollection<UIComapnyRow>();
            GlobalVariables.DefaultTaskTokenSource = new CancellationTokenSource();
            GlobalVariables.WatchListTokenSourceDic = new ConcurrentDictionary<string, CancellationTokenSource>();
            GlobalVariables.WatchListUICompanyRows = new BlockingCollection<UIComapnyRow>();
            Task.Run(() => LoadAndRefreshWatchListManager(CurrentUserId));
            Task.Run(() => LoadAndRefreshDefaultListManager());
        }

        private async void LoadAndRefreshWatchListManager(int currentUserId)
        {
            List<Task<UIComapnyRow>> watchlistTasks = GUIDataHelper.GetWatchUICompanyRowTaskList(currentUserId);
            foreach (var task in watchlistTasks)
            {
                UIComapnyRow oneRow = await task;
                GlobalVariables.WatchListUICompanyRows.TryAdd(oneRow);
            }

            await Task.WhenAll(watchlistTasks.ToArray());

            Dispatcher.Invoke(() =>
            {
                lsvWatchList.ItemsSource = GlobalVariables.WatchListUICompanyRows.ToList();

                Console.Out.WriteLine($"{lsvMarketPreview.Items.Count}:{GlobalVariables.DefaultUICompanyRows.Count}");
            });

            foreach (var watchUICompanyRow in GlobalVariables.WatchListUICompanyRows)
            {
                await Task.Run(() => LoadAndRefreshWatchListRow(watchUICompanyRow));
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

        private async Task LoadAndRefreshDefaultRow(string symbol)
        {
            try
            {
                UIComapnyRow companyRow = await GUIDataHelper.GetUICompanyRowTaskBySymbol(symbol);
                GlobalVariables.DefaultUICompanyRows.TryAdd(companyRow);
                Dispatcher.Invoke(() =>
                {
                    lsvMarketPreview.ItemsSource = GlobalVariables.DefaultUICompanyRows.ToList();

                    Console.Out.WriteLine(
                        $"\n%%%%% total: {lsvMarketPreview.Items.Count}:{GlobalVariables.DefaultUICompanyRows.Count}, cur: {companyRow.ToString()}");
                });

                 RefreshOneDefaultCompanyRow(companyRow);
            }
            catch (SystemException ex)
            {
                Console.Out.WriteLine(
                    $"{Thread.CurrentThread.ManagedThreadId}: default task was cancelled! {ex.Message}");
            }
        }

        private void RefreshOneDefaultCompanyRow(UIComapnyRow companyRow)
        {
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

        private void LoadAndRefreshWatchListRow(UIComapnyRow companyRow)
        {
            try
            {
                Console.Out.WriteLine(
                    $"\n%%%%% total:  {GlobalVariables.WatchListUICompanyRows.Count}, waited a task: " +
                    companyRow.Symbol);

                CancellationTokenSource watchListRowSource = new CancellationTokenSource();
                GlobalVariables.WatchListTokenSourceDic.TryAdd(companyRow.Symbol, watchListRowSource);
                Task.Run(async () =>
                {
                    int curThreadId = Thread.CurrentThread.ManagedThreadId;


                    Console.Out.WriteLine(
                        $"\n&&&&&&&&&&&&&&&&&&&&& Add token source for watchlist, REAL time: {curThreadId}:{companyRow.Symbol}");

                    while (!watchListRowSource.IsCancellationRequested)
                    {
                        RefreshRealTImePrice(companyRow);
                        Console.Out.WriteLine(
                            $"\n REFRESH WATCHLIST REAL TIME: {curThreadId}:{companyRow.Symbol} {companyRow.Price}\n");
                        await Task.Delay(RealTimeInterval);
                        Console.Out.WriteLine(
                            $"\n%%%total:  {GlobalVariables.WatchListUICompanyRows.Count}, refresh Watch list real data with cancel {watchListRowSource.IsCancellationRequested}, id:{curThreadId},: {companyRow.ToString()}");
                    }
                }, watchListRowSource.Token);


                Task.Run(async () =>
                {
                    int curThreadId = Thread.CurrentThread.ManagedThreadId;

                    Console.Out.WriteLine(
                        $"\n&&&&&&&&&&&&&&&&&&&&& Add token source for watchlist, One min: {curThreadId}:{companyRow.Symbol}");
                    while (!(watchListRowSource.IsCancellationRequested))
                    {
                        Refresh1MinData(companyRow);
                        Console.Out.WriteLine(
                            $"\n REFRESH WATCHLIST One Min: {curThreadId}:{companyRow.Symbol} {companyRow.Price}\n");
                        await Task.Delay(OneMinTimeInterval);
                        Console.Out.WriteLine(
                            $"\n >>>>>>total:  {GlobalVariables.WatchListUICompanyRows.Count}, refresh Watch list 1Min with cancel {watchListRowSource.IsCancellationRequested},id:{curThreadId},: {companyRow.ToString()}");
                    }
                }, watchListRowSource.Token);
            }
            catch (SystemException ex)
            {
                Console.Out.WriteLine(
                    $"{Thread.CurrentThread.ManagedThreadId}: Watch list threads was cancelled! {ex.Message}");
            }
        }


        private void LoadAndRefreshSearchResultRows()
        {
            GlobalVariables.SearchResultCancellationTokenSource = new CancellationTokenSource();
            foreach (UIComapnyRow companyRow in GlobalVariables.SearchResultUICompanyRows)
            {
                try
                {
                    Task.Run(async () =>
                    {
                        int curThreadId = Thread.CurrentThread.ManagedThreadId;

                        while (!GlobalVariables.SearchResultCancellationTokenSource.IsCancellationRequested)
                        {
                            RefreshRealTImePrice(companyRow);
                            Console.Out.WriteLine(
                                $"\n REFRESH Search result REAL TIME: {curThreadId}:{companyRow.Symbol} {companyRow.Price}\n");
                            await Task.Delay(RealTimeInterval);
                        }
                    }, GlobalVariables.SearchResultCancellationTokenSource.Token);


                    Task.Run(async () =>
                    {
                        int curThreadId = Thread.CurrentThread.ManagedThreadId;

                        while (!(GlobalVariables.SearchResultCancellationTokenSource.IsCancellationRequested))
                        {
                            Refresh1MinData(companyRow);
                            Console.Out.WriteLine(
                                $"\n REFRESH Search result One Min: {curThreadId}:{companyRow.Symbol} {companyRow.Price}\n");
                            await Task.Delay(OneMinTimeInterval);
                        }
                    }, GlobalVariables.SearchResultCancellationTokenSource.Token);
                }
                catch (SystemException ex)
                {
                    Console.Out.WriteLine(
                        $"{Thread.CurrentThread.ManagedThreadId}: Search result threads was cancelled! {ex.Message}");
                }
            }
        }

        private async void Refresh1MinData(UIComapnyRow comapnyRow)
        {
            try
            {
                if (!GlobalVariables.IsPseudoDataSource)
                {
                    List<Fmg1MinQuote> quote1MinList =
                        await RetrieveJsonDataHelper.RetrieveAllFmg1MinQuote(comapnyRow.Symbol);
                    if (quote1MinList.Count > 0)
                    {
                        comapnyRow.Volume = quote1MinList[0].Volume;
                    }
                }
                else
                {
                    /**************************************************
                        following line simulate Volume change during close hours.
                    ****************************************************/
                    comapnyRow.Volume += new Random().Next(50) * 1000;
                }
            }
            catch (JsonSerializationException)
            {
                MessageBox.Show("API response is weak. Please try later", "API Connection Fail", MessageBoxButton.OK, MessageBoxImage.Error);
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
                            notifier.ShowSuccess(
                                $"Higher price warning:\n{comapnyRow.CompanyName} : {comapnyRow.Symbol} \nNow: {comapnyRow.Price:N2} Target high: {comapnyRow.NotifyPriceHigh:N2}\nTime: {DateTime.Now:HH: mm: ss}");
                        });
                    }

                    if ((comapnyRow.NotifyPriceLow != 0 && comapnyRow.Price < comapnyRow.NotifyPriceLow))
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            notifier.ShowError(
                                $"Lower price warning:\n{comapnyRow.CompanyName} : {comapnyRow.Symbol} \nNow: {comapnyRow.Price:N2} Target low: {comapnyRow.NotifyPriceLow:N2}\nTime: {DateTime.Now:HH:mm:ss}");
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

                        GlobalVariables.WatchListUICompanyRows =
                            new BlockingCollection<UIComapnyRow>(new ConcurrentQueue<UIComapnyRow>(tempList));

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
                UIComapnyRow comapnyRow = (UIComapnyRow) (item as UIComapnyRow).Clone();
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
                    }
                    else
                    {
                        Task.WhenAll(GUIDataHelper.AddItemToWatchListTast(CurrentUserId, comapnyRow.CompanyId));
                        GlobalVariables.WatchListUICompanyRows.Add(comapnyRow);
                         Task.Run(() =>
                        {
                            LoadAndRefreshWatchListRow(comapnyRow); 
                            Dispatcher.Invoke(() =>
                                {
                                    lsvWatchList.ItemsSource = GlobalVariables.WatchListUICompanyRows.ToList();
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
        
       

        private void lsvMarketPreview_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = VisualTreeHelper.HitTest(lsvMarketPreview, Mouse.GetPosition(lsvMarketPreview)).VisualHit;

            // find ListViewItem (or null)
            while (item != null && !(item is ListBoxItem))
                item = VisualTreeHelper.GetParent(item);

            if (item != null)
            {
                int i = lsvMarketPreview.Items.IndexOf(((ListViewItem)item).DataContext);

                UIComapnyRow companyRow = lsvMarketPreview.Items.ToDynamicList()[i];
                CompanyDetailDialog detailDialog = new CompanyDetailDialog(companyRow);
                detailDialog.Owner = Application.Current.MainWindow;
                detailDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (detailDialog.ShowDialog() == true)
                {

                }
            }
        }

        private void btCancelDefaultThreads_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariables.DefaultTaskTokenSource.Cancel(true);
        }

        private void btCancelWatchlistThreads_Click(object sender, RoutedEventArgs e)
        {
            foreach (KeyValuePair<string, CancellationTokenSource> keyValuePair in GlobalVariables
                .WatchListTokenSourceDic)
            {
                keyValuePair.Value.Cancel(true);
            }
        }

        private async void btRestartRefresh_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariables.DefaultTaskTokenSource = new CancellationTokenSource();
            GlobalVariables.WatchListTokenSourceDic = new ConcurrentDictionary<string, CancellationTokenSource>();

            foreach (var companyRow in GlobalVariables.DefaultUICompanyRows)
            {
                await Task.Run(() => { RefreshOneDefaultCompanyRow(companyRow); });
            }

            foreach (var companyRow in GlobalVariables.WatchListUICompanyRows)
            {
                await Task.Run(()=>{ LoadAndRefreshWatchListRow(companyRow); });
            }
        }

        private void LsvMkt_miSetTargetPrice_OnClick(object sender, RoutedEventArgs e)
        {
            var item = lsvMarketPreview.SelectedItem;
            if (item != null)
            {
                UIComapnyRow companyRow = item as UIComapnyRow;
                PriceNotificationDialog priceDialog = new PriceNotificationDialog(companyRow);
                priceDialog.Owner = Application.Current.MainWindow;
                priceDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (priceDialog.ShowDialog() == true)
                {

                }
            }
        }

        private void TbSearchBox_OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            string searchString = tbSearchBox.Text;

            if (e.Key == Key.Enter)
            {
                if (searchString.StartsWith("@") && !searchString.EndsWith(";"))
                {
                    MessageBoxResult selResult = MessageBox.Show("Did you forget colon?", "@ query format error", MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    if (selResult == MessageBoxResult.Yes)
                    {
                        tbSearchBox.Text = searchString + ";";
                        tbSearchBox.Select(tbSearchBox.Text.Length, 0);
                    }
                }
                else
                {
                    tbSearchBox.Text = "Search here...";
                    cbSearchType.Text = "Symbol";
                    BlockingCollection<UIComapnyRow> searchUIcompanyRows = new BlockingCollection<UIComapnyRow>();
                    tbSearchBox.Text = "";
                    lbSearchResult.Visibility = Visibility.Hidden;

                    if (Regex.IsMatch(searchString, @"^[A-Z]{1,5}$") || Regex.IsMatch(searchString,
                        @"^@(CN|CEO|DS):[A-Za-z][A-Za-z ]{1,20};$"))
                    {
                        Task t = Task.Run(async () =>
                        {
                            //  GlobalVariables.SearchResultCompanies = await GUIDataHelper.GetSearchCompanyListTask(searchString.Trim());
                            foreach (Company comapny in GlobalVariables.SearchResultCompanies)
                            {
                                Task<UIComapnyRow> subTask = GUIDataHelper.GetUICompanyRowTaskBySymbol(comapny.Symbol);
                                UIComapnyRow companyRow = await subTask;
                                searchUIcompanyRows.Add(companyRow);
                            }
                        });
                        Task.WhenAll(t).ContinueWith(p =>
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                GlobalVariables.SearchResultUICompanyRows = searchUIcompanyRows;
                                lsvMarketPreview.ItemsSource = GlobalVariables.SearchResultUICompanyRows;
                            });
                            LoadAndRefreshSearchResultRows();
                        });
                    }
                    else
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show("Search string format error\n" + searchString, "No search result",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                        });
                    }
                }
            }
            else if (string.IsNullOrWhiteSpace(searchString))
            {
                lbSearchResult.Visibility = Visibility.Hidden;
            }
            else if (Regex.IsMatch(searchString, @"^[A-Z]{1,5}$") || Regex.IsMatch(searchString,
                @"^@(CN|CEO|DS):[A-Za-z][A-Za-z ]{1,20};$"))
            {
                Task.Run(async () =>
                {
                    GlobalVariables.SearchResultCompanies = await GUIDataHelper.GetSearchCompanyListTask(searchString.Trim());
                    if (GlobalVariables.SearchResultCompanies != null)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            lbSearchResult.ItemsSource = GlobalVariables.SearchResultCompanies;
                            lbSearchResult.Height = GlobalVariables.SearchResultCompanies.Count * 39;
                            lbSearchResult.Visibility = Visibility.Visible;
                        });
                    }
                    else
                    {
                        this.Dispatcher.Invoke(() => { lbSearchResult.Visibility = Visibility.Hidden; });
                    }
                });
            }
            else
            {
                lbSearchResult.Visibility = Visibility.Hidden;
            }
        }


        private void BrClearSearch_OnClick(object sender, RoutedEventArgs e)
        {
            tbSearchBox.Text = "Search here...";
            cbSearchType.Text = "Symbol";
            Task.Run(() =>
            {
                this.Dispatcher.Invoke(() => { lsvMarketPreview.ItemsSource = GlobalVariables.DefaultUICompanyRows; });
            });
        }

        private void TbSearchBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            tbSearchBox.Text = "";
        }

        private void TbSearchBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            tbSearchBox.Text = "Search here...";
        }

        private void LsvWatch_miTradeStock_OnClick(object sender, RoutedEventArgs e)
        {
            var item = (UIComapnyRow)lsvWatchList.SelectedItem;
            if (item == null) { return; }

            TradeDialog tradeDialog = new TradeDialog(CurrentUserId, item);
            if (tradeDialog.ShowDialog() == true)
            {
                StockTrader.IsUpdated = true;
            }
        }


        private void CbSearchType_cbCompanyName_OnSelected(object sender, RoutedEventArgs e)
        {
            tbSearchBox.Focus();
            tbSearchBox.Text = "@CN:";
            tbSearchBox.Select(tbSearchBox.Text.Length, 0);
        }

        private void CbSearchType_cbCEO_OnSelected(object sender, RoutedEventArgs e)
        {
            tbSearchBox.Focus();
            tbSearchBox.Text = "@CEO:";
            tbSearchBox.Select(tbSearchBox.Text.Length, 0);
        }

        private void CbSearchType_cbDescription_OnSelected(object sender, RoutedEventArgs e)
        {
            tbSearchBox.Focus();
            tbSearchBox.Text = "@DS:";
            tbSearchBox.Select(tbSearchBox.Text.Length, 0);
        }

        private void LsvMkt_miShowStockDetails_OnClick(object sender, RoutedEventArgs e)
        {
            var item = lsvMarketPreview.SelectedItem;
            if (item != null)
            {
                UIComapnyRow companyRow = item as UIComapnyRow;
                CompanyDetailDialog detailDialog = new CompanyDetailDialog(companyRow);
                detailDialog.Owner = Application.Current.MainWindow;
                detailDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (detailDialog.ShowDialog() == true)
                {

                }
            }
        }

        
     private void TgbDataSourceSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            GlobalVariables.IsPseudoDataSource = tgbDataSourceSwitch.IsChecked == true;
            MessageBox.Show("Data mocking: " + GlobalVariables.IsPseudoDataSource.ToString());
        }

        private void BtNotification_OnClick(object sender, RoutedEventArgs e)
        {
            var item = VisualTreeHelper.HitTest(lsvMarketPreview, Mouse.GetPosition(lsvMarketPreview)).VisualHit;

            // find ListViewItem (or null)
            while (item != null && !(item is ListBoxItem))
                item = VisualTreeHelper.GetParent(item);

            if (item != null)
            {
                int i = lsvMarketPreview.Items.IndexOf(((ListViewItem)item).DataContext);

                UIComapnyRow companyRow = GlobalVariables.DefaultUICompanyRows.ToList()[i];
                PriceNotificationDialog notificationDialog = new PriceNotificationDialog(companyRow);
                notificationDialog.Owner = Application.Current.MainWindow;
                notificationDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (notificationDialog.ShowDialog() == true)
                {

                }
            }
        }

        CancellationTokenSource chartTokenSource;
        private void LsvMkt_miRealTimeGraph_OnClick(object sender, RoutedEventArgs e)
        {
            UIComapnyRow selCompany = (UIComapnyRow)lsvMarketPreview.SelectedItem;

            if (selCompany == null)
            {
                return;
            }

            chartTokenSource = new CancellationTokenSource();

            try
            {
                RealTimePriceChart realTimeChart = new RealTimePriceChart(selCompany, chartTokenSource.Token);

                realTimeChart.Owner = Application.Current.MainWindow;
                realTimeChart.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                realTimeChart.ShowDialog();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("RealTime Chart canceled");
                chartTokenSource.Dispose();
            }
        }
    }



    

  
}
