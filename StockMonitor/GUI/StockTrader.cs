using GUI.Helpers;
using StockMonitor.Helpers;
using StockMonitor.Models.UIClasses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ToastNotifications.Messages;

namespace GUI
{
    public class StockTrader
    {
        public CancellationTokenSource TokenSource { get; set; }

        private CancellationToken _ct;
        public bool IsUpdated { get; set; }
        public int UserId { get; set; }
        public StockTrader(int userId)
        {
            TokenSource = new CancellationTokenSource();

            _ct = TokenSource.Token;

            IsUpdated = false;

            UserId = userId;
        }

        public void StartTrade()
        {
            try
            {
                Task.Factory.StartNew(TradeTask); //ex InvalidOperationException, OperationCanceledException, DataException
            }
            catch(InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Trading Canceled", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (OperationCanceledException ex)
            {
                MessageBox.Show(ex.Message, "Trading Canceled(Task Canceled)", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (DataException ex)
            {
                MessageBox.Show(ex.Message, "Trading Canceled(Database Error)", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void TradeTask()//ex InvalidOperationException, OperationCanceledException, DataException
        {
            while (true)
            {
                if (_ct.IsCancellationRequested) { return; }

                while (GlobalVariables.WatchListUICompanyRows.Count == 0)
                {
                    Thread.Sleep(1000);
                }

                var reservedTradingList = TradeDatabaseHelper.GetReservedTradingList(UserId); //ex InvalidOperationException

                if (reservedTradingList.Count == 0)
                {
                    while (!IsUpdated) { Thread.Sleep(3000); }
                    IsUpdated = false;
                    continue;
                }


                foreach (var resvTrading in reservedTradingList)
                {
                    if (resvTrading.DueDateTime.CompareTo(DateTime.Now) < 0)
                    {
                        GUIDataHelper.DeleteReservedTrading(resvTrading);
                        continue;
                    }

                    UIComapnyRow company = (from companyInWatchList in GlobalVariables.WatchListUICompanyRows
                                            where companyInWatchList.CompanyId == resvTrading.CompanyId
                                            select companyInWatchList).FirstOrDefault();

                    if (company == null)
                    {
                        throw new InvalidOperationException("[Internel Error]Reserved Trading is avaliable to companies in the WatchList");
                    }

                    if (resvTrading.TradeType.Equals(TradeEnum.Buy.ToString())
                        && company.Price <= resvTrading.TargetPrice)
                    {
                        TradeStock(resvTrading, company);
                        GUIDataHelper.DeleteReservedTrading(resvTrading); //ex DataException,InvalidOperationException
                    }
                    else if (resvTrading.TradeType.Equals(TradeEnum.Sell.ToString())
                        && company.Price >= resvTrading.TargetPrice)
                    {
                        TradeStock(resvTrading, company);
                        GUIDataHelper.DeleteReservedTrading(resvTrading); //ex DataException,InvalidOperationException
                    }
                }
            }
        }

        void TradeStock(ReservedTrading reservedTrading, UIComapnyRow company)
        {
            TradingRecord tradingRecord = new TradingRecord(reservedTrading, company.Price);
            GUIDataHelper.AddTradingRecord(tradingRecord);

            //TODO: Alert trade information

            GlobalVariables.MainWindow.Dispatcher.Invoke(() =>
            {
                GlobalVariables.MainWindow.SnackbarMessage($"Trade Succeed!\n[{company.Symbol}] {tradingRecord.ToString()}");
            });

            GlobalVariables.WatchListUserControl.Dispatcher.Invoke(() =>
            {
                GlobalVariables.WatchListUserControl.IsUpdated = true;
            });

            //GlobalVariables.SearchStockUserControl.Dispatcher.Invoke(() =>
            //{
            //    GlobalVariables.Notifier.ShowSuccess($"Trade Succeed!\n[{company.Symbol}] {tradingRecord.ToString()}");
            //});
        }
    }
}
