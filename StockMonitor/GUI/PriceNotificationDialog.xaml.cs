using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using StockMonitor.Models.UIClasses;

namespace GUI
{
    /// <summary>
    /// Interaction logic for SetTargetPriceNotificationDialog.xaml
    /// </summary>
    public partial class PriceNotificationDialog : Window
    {
        private StockMonitor.Models.UIClasses.UIComapnyRow _curCompanyRow;
        public PriceNotificationDialog(StockMonitor.Models.UIClasses.UIComapnyRow companyRow)
        {
            InitializeComponent();
            spDialogPanel.DataContext = companyRow;
            if (companyRow.NotifyPriceHigh == 0)
            {
                tbTargetHigh.Text = companyRow.Price.ToString("N2");
            }
            else
            {
                tbTargetHigh.Text = companyRow.NotifyPriceHigh.ToString("N2");
            }

            if (companyRow.NotifyPriceLow == 0)
            {
                tbTargetLow.Text = companyRow.Price.ToString("N2");
            }
            else
            {
                tbTargetLow.Text = companyRow.NotifyPriceLow.ToString("N2");
            }

            _curCompanyRow = companyRow;
        }


        private void BtClearSetting_OnClick(object sender, RoutedEventArgs e)
        {
            _curCompanyRow.NotifyPriceHigh = 0;
            _curCompanyRow.NotifyPriceLow = 0;
            this.DialogResult = true;
        }

        private void btSaveSetting_Click(object sender, RoutedEventArgs e)
        {
            double curPrice = double.Parse(tbPrice.Text);
            double highPrice = double.Parse(tbTargetHigh.Text);
            double lowPrice = double.Parse(tbTargetLow.Text);
            if (lowPrice >= highPrice)
            {
                MessageBox.Show(this, "Low target is equal or higher than high target!", "Price targets error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (curPrice > highPrice)
            {
                MessageBox.Show(this, "Current price has been HIGHER than high target!", "High price setting error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (curPrice < lowPrice)
            {
                MessageBox.Show(this, "Current price has been LOWER than low target!", "Low price setting error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _curCompanyRow.NotifyPriceHigh = double.Parse(tbTargetHigh.Text);
            _curCompanyRow.NotifyPriceLow = double.Parse(tbTargetLow.Text);
            this.DialogResult = true;
        }
        
        private void BtHighRoundDown_OnClick(object sender, RoutedEventArgs e)
        {
            double price = double.Parse(tbTargetHigh.Text);
            tbTargetHigh.Text = Math.Round(price * 99 / 100).ToString("N2");
        }

        private void BtHighRoundup_OnClick(object sender, RoutedEventArgs e)
        {
            double price = double.Parse(tbTargetHigh.Text);
            tbTargetHigh.Text = Math.Round(price * 101 / 100).ToString("N2");
        }

        private void BtLowRoundUp_OnClick(object sender, RoutedEventArgs e)
        {
            double price = double.Parse(tbTargetLow.Text);
            tbTargetLow.Text = Math.Round(price * 101 / 100).ToString("N2");
        }

        private void BtLowRoundDown_OnClick(object sender, RoutedEventArgs e)
        {
            double price = double.Parse(tbTargetLow.Text);
            tbTargetLow.Text = Math.Round(price * 99 / 100).ToString("N2");
        }
    }
}
