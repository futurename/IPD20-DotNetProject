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
        private bool IsTest { get; set; } 
        public StockTrader(int userId, bool isTest = false)
        {
            IsTest = isTest;

            TokenSource = new CancellationTokenSource();

            _ct = TokenSource.Token;

            IsUpdated = false;

            UserId = userId;
        }

        public void StartTrade()
        {
            while (GlobalVariables.WatchListUICompanyRows.Count == 0)
            {
                Thread.Sleep(1000);
            }

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

                var reservedTradingList = TradeDatabaseHelper.GetReservedTradingList(UserId); //ex InvalidOperationException

                if (reservedTradingList.Count == 0)
                {
                    while (!IsUpdated) { Thread.Sleep(3000); }
                    IsUpdated = false;
                    continue;
                }

                ReservedTrading reservedTrading = reservedTradingList[0];//MAYBE : Fatch only one from database is better?
                long tickDiff = reservedTrading.DueDateTime.Ticks - DateTime.Now.Ticks;

                UIComapnyRow company = (from companyInWatchList in GlobalVariables.WatchListUICompanyRows
                                        where companyInWatchList.CompanyId == reservedTrading.CompanyId
                                        select companyInWatchList).FirstOrDefault();

                if (company == null)
                {
                    throw new InvalidOperationException("[Internel Error]Reserved Trading is avaliable to companies in the WatchList");
                }


                while (tickDiff > -2000)//Take care of the case tickDiff is a little delayed(database response)
                {
                    if (_ct.IsCancellationRequested) { return; }

                    //Check Price is in the range
                    if (reservedTrading.MinPrice < company.Price && company.Price < reservedTrading.MaxPrice)
                    {
                        TradeStock(reservedTrading, company);
                        break;
                    }

                    if (IsUpdated)
                    {
                        break;
                    }
                    else
                    {
                        if (tickDiff > 5000) { Thread.Sleep(3000); }
                        else { Thread.Sleep(1000); }
                    }
                    tickDiff = reservedTrading.DueDateTime.Ticks - DateTime.Now.Ticks;
                }

                if (_ct.IsCancellationRequested) { return; }

                //Time Over
                if(!IsUpdated) {
                    GUIDataHelper.DeleteReservedTrading(reservedTrading); //ex DataException,InvalidOperationException
                }

                IsUpdated = false;
            }
        }

        void TradeStock(ReservedTrading reservedTrading, UIComapnyRow company)
        {
            TradingRecord tradingRecord = new TradingRecord(reservedTrading, company.Price);
            GUIDataHelper.AddTradingRecord(tradingRecord);

            //TODO: Alert trade information

            if (IsTest) { Console.WriteLine(tradingRecord); }
            else {
                GlobalVariables.MainWindow.Dispatcher.Invoke(() =>
                {
                    GlobalVariables.MainWindow.SnackbarMessage($"Trade Succeed!\n[{company.Symbol}] {tradingRecord.ToString()}");
                });
                
                //GlobalVariables.SearchStockUserControl.Dispatcher.Invoke(() =>
                //{
                //    GlobalVariables.Notifier.ShowSuccess($"Trade Succeed!\n[{company.Symbol}] {tradingRecord.ToString()}");
                //});
            }
        }
    }
}
