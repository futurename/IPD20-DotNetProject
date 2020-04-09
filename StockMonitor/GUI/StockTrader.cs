using StockMonitor.Helpers;
using StockMonitor.Models.UIClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GUI
{
    public class StockTrader
    {
        public CancellationTokenSource TokenSource { set; get; }

        private CancellationToken _ct;
        public bool IsUpdated { set; get; }
        public int UserId { set; get; }

        UserControl UserControl { get; set; }

        public StockTrader(UserControl userControl,int userId)
        {
            UserControl = userControl;

            TokenSource = new CancellationTokenSource();

            _ct = TokenSource.Token;

            IsUpdated = false;

            UserId = userId;
            while (GlobalVariables.WatchListUICompanyRows.Count == 0) {
                Thread.Sleep(1000);
            }
            Task.Factory.StartNew(TradeTask);
        }

        public void TradeTask()
        {
            while (true)
            {
                if (_ct.IsCancellationRequested) { return; }

                var reservedTradingList = DatabaseHelper.GetReservedTradingList(UserId);
                
                if(reservedTradingList.Count == 0)
                {
                    while (!IsUpdated) { Thread.Sleep(3000); }
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


                while (tickDiff > -1000)//Take care of the case tickDiff is a little delayed(database response)
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
                    GUIDataHelper.DeleteReservedTrading(reservedTrading);
                }

                IsUpdated = false;
            }
        }

        void TradeStock(ReservedTrading reservedTrading, UIComapnyRow company)
        {
            TradingRecord tradingRecord = new TradingRecord(reservedTrading, company.Price);
            GUIDataHelper.AddTradingRecord(tradingRecord);
            //TODO: Alert trade information
            if(UserControl != null) {
                //FIXME: Open Dialog with message
                MessageBox.Show(Application.Current.MainWindow, tradingRecord.ToString());
            }
        }
    }
}
