using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for SearchStockUserControl.xaml
    /// </summary>
    public partial class SearchStockUserControl : UserControl
    {
        List<Task<UIComapnyRow>> taskList;
        List<Task<UIComapnyRow>> watchTaskList;

        List<UICompanyRowWraperForListView> companyDataRowList;
        public static List<UICompanyRowWraperForListView> CompanyWatchListDataRowList { get; set; }

        DateTime start, end;
        public SearchStockUserControl()
        {
            start = DateTime.Now;
            InitializeComponent();

            // In DesignMode, it avoid read app.config fail exception(in this case, cannot read "connectionString" from app.config)
            // DesignMode use compiled UserControl which doesn't include app.config file, so every behavior that read/connect
            // app.config file data throws exception
            // This prevents program from continues when it is design mode
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            LoadingCompanyDataOnSearchStockFromApi();

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

            string[] watchList = { "SDRL", "DRD", "HSY", "LHO" };

            taskList = new List<Task<UIComapnyRow>>();

            foreach (string name in companyNames)
            {
                taskList.Add(GUIHelper.GetCompanyDataRowTask(name));
            }

            watchTaskList = new List<Task<UIComapnyRow>>();
            foreach (string name in watchList)
            {
                watchTaskList.Add(GUIHelper.GetCompanyDataRowTask(name));
            }
        }

        private async void DisplayCompanyDataOnSearchStock()
        {
            await Task.Run(SaveLoadedDataOnList);

            lsvMarketPreview.ItemsSource = companyDataRowList;
            lsvWatch.ItemsSource = CompanyWatchListDataRowList;
        }

        private async Task SaveLoadedDataOnList()
        {
            companyDataRowList = new List<UICompanyRowWraperForListView>();
            foreach (Task<UIComapnyRow> task in taskList)
            {
                try
                {
                    UIComapnyRow company = await task;
                    companyDataRowList.Add(new UICompanyRowWraperForListView(company));
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Console.Out.WriteLine("!!!!! Failed: " + ex.Message);
                }
                catch (SystemException ex)
                {
                    Console.Out.WriteLine("[Parse Error] " + ex.Message);
                }
            }

            CompanyWatchListDataRowList = new List<UICompanyRowWraperForListView>();
            foreach (Task<UIComapnyRow> task in watchTaskList)
            {
                try
                {
                    UIComapnyRow company = await task;
                    CompanyWatchListDataRowList.Add(new UICompanyRowWraperForListView(company));
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Console.Out.WriteLine("!!!!! Failed: " + ex.Message);
                }
                catch (SystemException ex)
                {
                    Console.Out.WriteLine("[Parse Error] " + ex.Message);
                }
            }
        }
    }

    public class UICompanyRowWraperForListView
    {
        public UIComapnyRow Company { get; set; }

        public double MarketCapital { get; set; }
        public double PriceToEarningRatio { get; set; }
        public double PriceToSalesRatio { get; set; }
        public UICompanyRowWraperForListView(UIComapnyRow company)//ex FormatException
        {
            Company = company;
            MarketCapital = double.Parse(company.MarketCapital, CultureInfo.InvariantCulture);//ex
            PriceToEarningRatio = double.Parse(company.PriceToEarningRatio, CultureInfo.InvariantCulture);//ex
            PriceToSalesRatio = double.Parse(company.PriceToSalesRatio, CultureInfo.InvariantCulture);//ex
        }
    }
}
