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
using static GUI.Wrapper;

namespace GUI
{
    /// <summary>
    /// Interaction logic for WatchListUserControl.xaml
    /// </summary>
    public partial class WatchListUserControl : UserControl
    {
        List<UICompanyRowWrapper> companyList;

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
            companyList = new List<UICompanyRowWrapper>();
            var watchListRowTasks = GUIDataHelper.GetWatchUICompanyRowTaskList(userId);
            foreach (Task<UIComapnyRow> task in watchListRowTasks)
            {
                UIComapnyRow comapnyRow = await task;

                companyList.Add(new UICompanyRowWrapper(comapnyRow));
            }

            this.Dispatcher.Invoke(() =>
            {
                lstWatch.ItemsSource = companyList;
                if(companyList.Count != 0)
                {
                    lstWatch.SelectedIndex = 0;
                    Symbol = ((UICompanyRowWrapper)lstWatch.Items[0]).Company.Symbol;
                }
            });
        }

        private void lstWatch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UICompanyRowWrapper selCompany = (UICompanyRowWrapper)lstWatch.SelectedItem;
            if (selCompany == null) { return; }

            //TODO: Set values, text on the components
            
            
            Symbol = selCompany.Company.Symbol;
            Global.ConcurentDictionary.AddOrUpdate("symbol", selCompany.Company.Symbol, (k, v)=> selCompany.Company.Symbol);
        }
    }
}
