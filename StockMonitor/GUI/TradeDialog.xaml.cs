using StockMonitor.Helpers;
using StockMonitor.Models.UIClasses;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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

            try
            {
                lvReservedTrading.ItemsSource = GUIDataHelper.GetReservedList(UserId);//ex InvalidOperationException,IOException
                lvTradingRecord.ItemsSource = GUIDataHelper.GetTradingResordList(UserId);//ex InvalidOperationException
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Internel Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IOException)
            {
                MessageBox.Show("Cannot Connect Database", "Internel Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.DataContext = this;
        }

        private void btTrade_Click(object sender, RoutedEventArgs e)
        {
            try {
                TradeEnum trade;
                if (rbBuy.IsChecked == true) { trade = TradeEnum.Buy; }
                else if(rbSell.IsChecked == true) { trade = TradeEnum.Sell; }
                else { throw new SystemException("InnerError:Trade radio button"); }

                string quantityStr = tbQuantity.Text;

                string targetPriceStr = tbTargetPrice.Text;

                DateTime pickDate = dpDueDateTime.SelectedDate.GetValueOrDefault();
                DateTime pickTime = tpDueDateTime.SelectedTime.GetValueOrDefault();

                ReservedTrading newReservedTrading = new ReservedTrading(
                    Company.CompanyId, UserId, trade, quantityStr, targetPriceStr, pickDate, pickTime
                );

                GUIDataHelper.InsertReservedTrading(newReservedTrading);//ex DateException, InvalidOperationException

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
            catch (DataException ex)
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

        private void lvReservedTrading_miDelete_Click(object sender, RoutedEventArgs e)
        {
            ReservedTrading selTrading = (ReservedTrading)lvReservedTrading.SelectedItem;

            if(selTrading == null) { return; }

            try
            {
                GUIDataHelper.DeleteReservedTrading(selTrading); //ex InvalidOperationException

                lvReservedTrading.ItemsSource = GUIDataHelper.GetReservedList(UserId);//ex InvalidOperationException
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Internel Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lvTradingRecord_miDelete_Click(object sender, RoutedEventArgs e)
        {
            TradingRecord selRecord = (TradingRecord)lvTradingRecord.SelectedItem;
            if(selRecord == null) { return; }

            try
            {
                GUIDataHelper.DeleteTradingRecord(selRecord); //ex InvalidOperationException

                lvTradingRecord.ItemsSource = GUIDataHelper.GetTradingResordList(UserId);//ex InvalidOperationException
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Internel Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
