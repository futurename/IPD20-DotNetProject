using StockMonitor.Helpers;
using StockMonitor.Models.UIClasses;
using System;
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

namespace GUI
{
    /// <summary>
    /// Interaction logic for WatchListUserControl.xaml
    /// </summary>
    public partial class WatchListUserControl : UserControl
    {
        List<UIComapnyRow> companyList;

        public static readonly DependencyProperty SymbolProperty =
        DependencyProperty.Register("Symbol", typeof(string), typeof(UserControl), new FrameworkPropertyMetadata(null));

        private string Symbol
        {
            get { return (string)GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }

        public WatchListUserControl()
        {
            InitializeComponent();

            Task.Factory.StartNew(LoadWatchList);

            this.DataContext = this;
        }

        private async void LoadWatchList()// TODO: sync -> async
        {
            int userId = 3;
            companyList = new List<UIComapnyRow>();
            var watchListRowTasks = GUIDataHelper.GetWatchUICompanyRowTaskList(userId);
            foreach (Task<UIComapnyRow> task in watchListRowTasks)
            {
                UIComapnyRow comapnyRow = await task;

                companyList.Add(comapnyRow);
            }

            this.Dispatcher.Invoke(() =>
            {
                lstWatch.ItemsSource = companyList;
                if(companyList.Count != 0)
                {
                    lstWatch.SelectedIndex = 0;
                    Symbol = ((UIComapnyRow)lstWatch.Items[0]).Symbol;
                }
            });
        }

        private void lstWatch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UIComapnyRow selCompany = (UIComapnyRow)lstWatch.SelectedItem;
            if (selCompany == null) { return; }

            Symbol = selCompany.Symbol;
            Global.ConcurentDictionary.AddOrUpdate("symbol", selCompany.Symbol, (k, v)=> selCompany.Symbol);

            txtOpenPrice.Text = selCompany.Open.ToString("N2");
            txtMarketCapital.Text = selCompany.MarketCapital.ToString("#,##0,,M");
            txtEarningRatio.Text = selCompany.PriceToEarningRatio.ToString("0.00");
            txtSalesRatio.Text = selCompany.PriceToSalesRatio.ToString("0.00");
            txtCompanyName.Text = selCompany.CompanyName;
            txtIndustry.Text = selCompany.Industry;
            txtDescription.Text = selCompany.Description;
        }
    }
}
