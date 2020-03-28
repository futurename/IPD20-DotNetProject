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
using StockMonitor.Models.POCO;

namespace FirstWindowTest2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<CompanyDataRow> companyDataRowList;
        public MainWindow()
        {
            InitializeComponent();
            companyDataRowList = new List<CompanyDataRow>();
            companyDataRowList.Add(ExtractApiDataToPoCoHelper.GetCompanyDataRow("AAPL"));
            lsvMarketPreview.DataContext = companyDataRowList;
        }
    }
}
