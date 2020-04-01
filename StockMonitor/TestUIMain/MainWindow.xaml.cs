using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
using StockMonitor.Helpers;
using StockMonitor.Models.UIClasses;

namespace TestUIMain
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        List<UIComapnyRow> companyDataRowList = new List<UIComapnyRow>();

        DateTime start, end;
        string[] companyNames = {"CASI", "GBL", "INWK", "BOKF", "PVBC", "MRC", "NEWM", "ICON",
            "SLM", "DVCR", "PETX", "CODX", "LIVE", "SHEN", "TMK", "INTU", "VNOM", "NSYS", "EOLS" };
        public MainWindow()
        {
            start = DateTime.Now;

            InitializeComponent();

            GetAsyncCompnayRow();

            DateTime end = DateTime.Now;
            TimeSpan timeSpan = new TimeSpan();
            timeSpan = end - start;
            MessageBox.Show($"Loading time: {timeSpan.TotalMilliseconds} mills");
        }

        private void GetAsyncCompnayRow()
        {
            List<Task> taskList = new List<Task>();
            for (int i = 0; i < companyNames.Length; i++)
            {

                string name = companyNames[i];
                //taskList.Add(ExtractApiDataToPoCoHelper.GetCompanyDataRow(name));
                Task task = new Task(() =>
                {
                    UIComapnyRow companyRow = ExtractApiDataToPoCoHelper.GetCompanyDataRowNo1MinData(name);
                    companyDataRowList.Add(companyRow);
                });
                taskList.Add(task);
                task.Start();
            }

            foreach (var task in taskList)
            {
                task.Wait();
            }

            lsvMarketPreview.ItemsSource = companyDataRowList;
        }

        private void SetListView()
        {
            GetAsyncCompnayRow();

            lsvMarketPreview.ItemsSource = companyDataRowList;


            end = DateTime.Now;
            TimeSpan timeSpan = new TimeSpan();
            timeSpan = end - start;
            Console.WriteLine("##############Total time:{0} milli####################", timeSpan);

        }
    }
}
