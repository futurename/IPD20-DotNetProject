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
        public UIComapnyRow Company { get; set; }
        private int UserId { get; set; }
        public ReservedTrading NewReservedTrading { get; set; }
        public TradeDialog(int userId, UIComapnyRow company)
        {
            UserId = userId;
            Company = company;

            InitializeComponent();

            lvReservedTrading.ItemsSource = GUIDataHelper.GetReservedList(UserId);

            this.DataContext = this;
        }

        private void btTrade_Click(object sender, RoutedEventArgs e)
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

                ReservedTrading newReservedTrading = new ReservedTrading(
                    Company.CompanyId, UserId, trade, quantityStr, minPriceStr, maxPriceStr, pickDate, pickTime
                );

                GUIDataHelper.InsertReservedTrading(newReservedTrading);
                //ex IOException, InvalidOperationException

                this.DialogResult = true;
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
