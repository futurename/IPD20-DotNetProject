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

namespace GUI
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        List<Task<UIComapnyRow>> taskList;

        List<UIComapnyRow> companyDataRowList;

        DateTime start, end;
        public UserControl1()
        {
            start = DateTime.Now;

            LoadingCompanyDataOnSearchStockFromApi();

            InitializeComponent();

            DisplayCompanyDataOnSearchStock();

            end = DateTime.Now;

            TimeSpan timeSpan = new TimeSpan();
            timeSpan = end - start;
            Console.WriteLine("##############Total time:{0} milli####################", timeSpan);
        }

        private void LoadingCompanyDataOnSearchStockFromApi()
        {
            string[] companyNames = {"CASI", "MPO", "GBL", "INWK", "BOKF", "PVBC", "MRC", "NEWM", "ICON",
                "SLM", "DVCR", "PETX", "CODX", "LIVE", "SHEN", "TMK", "INTU", "VNOM", "NSYS", "EOLS" };

            taskList = new List<Task<UIComapnyRow>>();
            foreach (string name in companyNames)
            {
                taskList.Add(ExtractApiDataToPoCoHelper.GetCompanyDataRow(name));
            }
        }

        private async void DisplayCompanyDataOnSearchStock()
        {
            await Task.Run(SaveLoadedDataOnList);

            lsvMarketPreview.ItemsSource = companyDataRowList;
        }

        private async Task SaveLoadedDataOnList()
        {
            companyDataRowList = new List<UIComapnyRow>();

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
            }

        }
    }
}
