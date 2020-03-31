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
using StockMonitor.Helpers;
using StockMonitor.Models.UIClasses;

namespace TestUIMain
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<UIComapnyRow> companyDataRowList;
        public MainWindow()
        {
            InitializeComponent();
            DateTime start = DateTime.Now;
            
            SetListView();
            
          /*  DateTime end = DateTime.Now;
            TimeSpan timeSpan = new TimeSpan();
            timeSpan = end - start;
            MessageBox.Show($"Loading time: {timeSpan.TotalMilliseconds} mills");*/
        }
        private void InitListView()
        {
            companyDataRowList = new List<UIComapnyRow>();
            companyDataRowList.Add(ExtractApiDataToPoCoHelper.GetCompanyDataRow("AAPL"));
            companyDataRowList.Add(ExtractApiDataToPoCoHelper.GetCompanyDataRow("AMZN"));
            companyDataRowList.Add(ExtractApiDataToPoCoHelper.GetCompanyDataRow("FB"));
            companyDataRowList.Add(ExtractApiDataToPoCoHelper.GetCompanyDataRow("GOOG"));
            companyDataRowList.Add(ExtractApiDataToPoCoHelper.GetCompanyDataRow("WMT"));

            //lsvMarketPreview.ItemsSource = companyDataRowList;
        }

        private async void SetListView()
        {
            await Task.Run(InitListView);
            lsvMarketPreview.ItemsSource = companyDataRowList;
        }
    }
}
