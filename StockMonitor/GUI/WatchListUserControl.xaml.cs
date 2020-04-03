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
        List<Task<UIComapnyRow>> taskList;
        List<UICompanyRowWrapper> companyList;

        public WatchListUserControl()
        {
            InitializeComponent();

            Task.Factory.StartNew(LoadWatchList);
        }

        private async Task LoadWatchList()
        {
            string[] companyNames = { "VXUS", "AAPL", "AMZN" };

            taskList = new List<Task<UIComapnyRow>>();
            foreach (string name in companyNames)
            {
                taskList.Add(GUIDataHelper.GetCompanyDataRowTask(name));
            }

            foreach (Task<UIComapnyRow> task in taskList)
            {
                UIComapnyRow company = await task;
                companyList.Add(new UICompanyRowWrapper(company));
            }

            lstWatch.ItemsSource = companyList;
        }

    }
}
