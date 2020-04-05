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
using System.Windows.Shapes;

namespace GUI
{
    /// <summary>
    /// Interaction logic for SetTargetPriceNotificationDialog.xaml
    /// </summary>
    public partial class SetTargetPriceNotificationDialog : Window
    {
        public SetTargetPriceNotificationDialog(StockMonitor.Models.UIClasses.UIComapnyRow companyRow)
        {
            InitializeComponent();
            spDialogPanel.DataContext = companyRow;
            //tbCompanyName.Text = companyRow.CompanyName;
            // tbPrice.Text = companyRow.Price.ToString();
            // tbSymbol.Text = companyRow.Symbol;
            // imgLogo.Source = companyRow.ByteToImage(companyRow.Logo);
        }
    }
}
