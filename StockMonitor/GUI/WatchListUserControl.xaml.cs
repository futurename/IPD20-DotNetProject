 using LiveCharts;
using LiveCharts.Wpf;
using StockMonitor.Helpers;
using StockMonitor.Models.UIClasses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using MaterialDesignThemes.Wpf;

namespace GUI
{
    /// <summary>
    /// Interaction logic for WatchListUserControl.xaml
    /// </summary>
    public partial class WatchListUserControl : UserControl
    {
        public Func<ChartPoint, string> PointLabel { get; set; }

        private bool _isUpdated;
        public bool IsUpdated
        {
            set
            {
                _isUpdated = value;

                if(GlobalVariables.WatchListUICompanyRows.Count == 0) 
                {
                    gridGrayOut.Visibility = Visibility.Visible;
                    GlobalVariables.CandleChartUserControl.Symbol = ""; 
                }
                else
                {
                    DrawPieChart();
                    lstWatch.ItemsSource = GlobalVariables.WatchListUICompanyRows.ToList();
                    gridGrayOut.Visibility = Visibility.Collapsed;
                    lstWatch.SelectedIndex = 0;
                }
                _isUpdated = false;
            }
        }
        
        private int UserId { get; set; }
        public WatchListUserControl()
        {
            GlobalVariables.WatchListUserControl = this;
            
            UserId = 3;//For Test

            InitializeComponent();

            //Task.Factory.StartNew(LoadWatchList); //For Test

            this.DataContext = this;

            if (GlobalVariables.WatchListUICompanyRows.Count == 0) { 
                gridGrayOut.Visibility = Visibility.Visible;
                return; 
            }

            lstWatch.ItemsSource = GlobalVariables.WatchListUICompanyRows;
            gridGrayOut.Visibility = Visibility.Collapsed;

            lstWatch.SelectedIndex = 0;

            GlobalVariables.CandleChartUserControl.Symbol = ((UIComapnyRow)lstWatch.Items[0]).Symbol;

            DrawPieChart();
            
        }

        private void DrawPieChart()
        {
            PointLabel = chartPoint =>
                string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);

            var tradingDictionary = GUIDataHelper.GetTradingRecourdList(UserId);
            pieChartTrading.Series.Clear();
            foreach(var trading in tradingDictionary)
            {
                if(trading.Value == 0) { continue; }

                var value = new ChartValues<int>();
                value.Add(trading.Value);

                pieChartTrading.Series.Add(
                    new PieSeries()
                    {
                        Title = trading.Key,
                        Values = value,
                        DataLabels = true,
                        LabelPoint = PointLabel,
                        ToolTip = null,
                        Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0))
                    }
                );
            }

        }

        private void lstWatch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UIComapnyRow selCompany = (UIComapnyRow)lstWatch.SelectedItem;
            if (selCompany == null) {
                txtOpenPrice.Text = "-";
                txtMarketCapital.Text = "-";
                txtEarningRatio.Text = "-";
                txtSalesRatio.Text = "-";
                txtCompanyName.Text ="Company Name";
                txtIndustry.Text = "Industry";
                txtDescription.Text = "Description";
                GlobalVariables.CandleChartUserControl.Symbol = "";
                return; 
            }

            GlobalVariables.CandleChartUserControl.Symbol = selCompany.Symbol;

            txtOpenPrice.Text = selCompany.Open.ToString("N2");
            txtMarketCapital.Text = selCompany.MarketCapital.ToString("#,##0,,M");
            txtEarningRatio.Text = selCompany.PriceToEarningRatio.ToString("0.00");
            txtSalesRatio.Text = selCompany.PriceToSalesRatio.ToString("0.00");
            txtCompanyName.Text = selCompany.CompanyName;
            txtIndustry.Text = selCompany.Industry;
            txtDescription.Text = selCompany.Description;
        }


        //Same Method from SearchStockUserControl
        private void LsvWatch_miDeleteFromWatchList_OnClick(object sender, RoutedEventArgs e)
        {
            var item = lstWatch.SelectedItem;
            if (item != null)
            {
                UIComapnyRow companyRow = item as UIComapnyRow;
                try
                {
                    Task t = GUIDataHelper.DeleteFromWatchListTask(UserId, companyRow.CompanyId);
                    Task.WhenAll(t).ContinueWith(p =>
                    {
                        List<UIComapnyRow> tempList = GlobalVariables.WatchListUICompanyRows.ToList();
                        tempList.Remove(companyRow);

                        GlobalVariables.WatchListUICompanyRows =
                            new BlockingCollection<UIComapnyRow>(new ConcurrentQueue<UIComapnyRow>(tempList));

                        this.Dispatcher.Invoke(() =>
                        {
                            IsUpdated = true;
                            GlobalVariables.SearchStockUserControl.lsvWatchList.ItemsSource = GlobalVariables.WatchListUICompanyRows;
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

        private void LsvWatch_miTradeStock_OnClick(object sender, RoutedEventArgs e)
        {
            var item = (UIComapnyRow)lstWatch.SelectedItem;
            if (item == null) { return; }

            TradeDialog tradeDialog = new TradeDialog(UserId, item);
            if (tradeDialog.ShowDialog() == true)
            {
                GlobalVariables.StockTrader.IsUpdated = true;
            }
        }

    }
}
