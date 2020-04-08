using StockMonitor.Helpers;
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
    /// Interaction logic for TradeDialog.xaml
    /// </summary>
    public enum TradeEnum { Buy, Sell }
    public enum OrderType { Limit, Stop }

    public partial class TradeDialog : Window
    {
        
        public TradeDialog()
        {
            InitializeComponent();
        }

        private async void btTrade_Click(object sender, RoutedEventArgs e)
        {
            try {
            //TODO: validations
                TradeEnum trade;
                if (rbBuy.IsChecked == true) { trade = TradeEnum.Buy; }
                else if(rbSell.IsChecked == true) { trade = TradeEnum.Sell; }
                else { throw new SystemException("InnerError:Trade radio button"); }

                string quantityStr = tbQuantity.Text;

                string minPriceStr = tbMixPrice.Text;
                string maxPriceStr = tbMaxPrice.Text;

                DateTime pickDate = dpDueDateTime.SelectedDate.GetValueOrDefault();
                DateTime pickTime = tpDueDateTime.SelectedTime.GetValueOrDefault();

                int companyId = 1;
                int userId = 3;

                ReservedTrading newReservedTrading = new ReservedTrading(
                    companyId, userId, trade, quantityStr, minPriceStr, maxPriceStr, pickDate, pickTime
                );

                GUIDataHelper.InsertReservedTrading(newReservedTrading);
                //ex IOException, InvalidOperationException
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(this, ex.Message, "Validation Error");
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(this, ex.Message, "Trade Reservation Fail");
            }
            catch (SystemException ex)
            {
                MessageBox.Show(this,
                    "Internal Error",
                    ex.Message,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
