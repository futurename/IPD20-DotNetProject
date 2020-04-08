using StockMonitor.Helpers;
using StockMonitor.Models.UIClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GUI
{
    class StockTrader
    {
        public CancellationTokenSource TokenSource { set; get; }

        private CancellationToken _ct;
        public bool IsUpdated { set; get; }
        public int UserId { set; get; }

        public StockTrader(int userId)
        {
            TokenSource = new CancellationTokenSource();

            _ct = TokenSource.Token;

            IsUpdated = false;

            UserId = userId;

            Task.Factory.StartNew(TradeTask);
        }

        public void TradeTask()
        {
            while (true)
            {
                if (_ct.IsCancellationRequested) { return; }

                ReservedTrading reservedTrading = DatabaseHelper.GetReservedTradingList(UserId)[0];//MAYBE : Fatch only one from database is better?

                UIComapnyRow company = (from companyInWatchList in GlobalVariables.WatchListUICompanyRows
                                        where companyInWatchList.CompanyId == reservedTrading.CompanyId
                                        select companyInWatchList).FirstOrDefault();

                if (company == null)
                {
                    throw new InvalidOperationException("Reserved Trading is avaliable to companies in the WatchList");
                }

                long tickDiff = reservedTrading.DueDateTime.Ticks - DateTime.Now.Ticks;

                while (tickDiff > 0 && !IsUpdated)
                {
                    if (_ct.IsCancellationRequested) { return; }

                    //Check Price is in the range
                    if (reservedTrading.MinPrice < company.Price && company.Price < reservedTrading.MaxPrice)
                    {
                        GUIDataHelper.AddTradingRecord(new TradingRecord(reservedTrading, company.Price));
                        break;
                    }

                    if (IsUpdated)
                    {
                        reservedTrading = DatabaseHelper.GetReservedTradingList(UserId)[0];//MAYBE : Fatch only one from database is better?
                        IsUpdated = false;
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
                GUIDataHelper.DeleteReservedTrading(reservedTrading);
            }
        }

    }
}
