using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using StockMonitor.Helpers;
using StockMonitor.Models.POCO;
using Timer = System.Timers.Timer;

namespace FirstWindowTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<CompanyDataRow> companyDataRowList;
        public MainWindow()
        {
           
            
           
            InitializeComponent();
            
           /* Timer timer= new System.Timers.Timer();
            timer.Interval = 5000;
            timer.Elapsed += OnTimeEvent;
            timer.Enabled = true;*/

           SetListView();



        }

        private async void OnTimeEvent(object sender, ElapsedEventArgs e)
        {
            await Task.Run( InitListView);
            lsvMarketPreview.ItemsSource = companyDataRowList;
            Console.Out.WriteLine("refresh...");
        }

        private void InitListView( )
        {
            companyDataRowList = new List<CompanyDataRow>();
            companyDataRowList.Add(ExtractApiDataToPoCoHelper.GetCompanyDataRow("AAPL"));
            companyDataRowList.Add(ExtractApiDataToPoCoHelper.GetCompanyDataRow("AMZN"));
            companyDataRowList.Add(ExtractApiDataToPoCoHelper.GetCompanyDataRow("FB"));
            companyDataRowList.Add(ExtractApiDataToPoCoHelper.GetCompanyDataRow("GOOG"));
            companyDataRowList.Add(ExtractApiDataToPoCoHelper.GetCompanyDataRow("WMT"));
      
            //lsvMarketPreview.ItemsSource = companyDataRowList;
        }

        private async void SetListView( )
        {
            await Task.Run(InitListView);
            lsvMarketPreview.ItemsSource = companyDataRowList;
        }
    }
}
