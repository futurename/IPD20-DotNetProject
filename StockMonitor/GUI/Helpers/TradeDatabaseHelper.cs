using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.Helpers
{
    public static class TradeDatabaseHelper
    {
        public static void InsertReservedTrading(ReservedTrading reservedTrading)
        {//ex DateException, InvalidOperationException
            using (StockMonitorEntities _dbContext = new StockMonitorEntities())
            {
                var companyTrading = from trade in GetReservedTradingList(reservedTrading.UserId) //ex InvalidOperationException
                                     where trade.CompanyId == reservedTrading.CompanyId
                                     select trade;

                if (companyTrading.Count() == 0)
                {
                    _dbContext.ReservedTradings.Add(reservedTrading);
                    _dbContext.SaveChanges(); //ex DataException
                }
                else
                {
                    throw new InvalidOperationException("One Trade reserved already with this company");
                }
            }
        }

        public static List<ReservedTrading> GetReservedTradingList(int userId)//ex InvalidOperationException
        {
            StockMonitorEntities _dbContext = new StockMonitorEntities();

            User tradingUser = (from user in _dbContext.Users.Include("ReservedTradings")
                                    where user.Id == userId
                                    select user).FirstOrDefault();

            if (tradingUser == null)
            {
                throw new InvalidOperationException($"There is no User id[{userId}]");
            }

            return tradingUser.ReservedTradings.ToList();
        }

        public static void AddTradingRecord(TradingRecord tradingRecord)//ex DataException 
        {
            using (StockMonitorEntities _dbContext = new StockMonitorEntities())
            {
                _dbContext.TradingRecords.Add(tradingRecord);
                _dbContext.SaveChanges();//ex DataException 
            }
        }
        public static void DeleteReservedTrading(ReservedTrading reservedTrading)//ex DataException,InvalidOperationException
        {
            using (StockMonitorEntities _dbContext = new StockMonitorEntities())
            {
                ReservedTrading trading = _dbContext.ReservedTradings.Find(reservedTrading.Id);//ex InvalidOperationException
                if (trading == null) { throw new InvalidOperationException($"Cannot find reservedTrading[{reservedTrading.Id}]"); }
                _dbContext.ReservedTradings.Remove(trading);
                _dbContext.SaveChanges();//ex DataException,InvalidOperationException
            }
        }

        public static Dictionary<string, int> GetTradingRecordList(int userId)
        {
            StockMonitorEntities _dbContext = new StockMonitorEntities();

            Dictionary<string, int> tradeHistory = new Dictionary<string, int>();

            User tradingUser = (from user in _dbContext.Users.Include("ReservedTradings")
                                where user.Id == userId
                                select user).FirstOrDefault();

            foreach(var listItem in tradingUser.WatchListItems)
            {
                int count = (from reservedTrading in tradingUser.TradingRecords
                             where reservedTrading.CompanyId == listItem.CompanyId
                             select reservedTrading).Count();
                tradeHistory.Add(listItem.Company.Symbol, count);
            }

            var reservedTradingsList = tradingUser.ReservedTradings;




            return tradeHistory;
        }

        public static List<TradingRecord> GetTradingResordList(int userId)//ex InvalidOperationException
        {
            StockMonitorEntities _dbContext = new StockMonitorEntities();

            User tradingUser = (from user in _dbContext.Users.Include("ReservedTradings")
                                where user.Id == userId
                                select user).FirstOrDefault();

            if (tradingUser == null)
            {
                throw new InvalidOperationException($"There is no User id[{userId}]");
            }

            return tradingUser.TradingRecords.ToList();
        }

        public static void DeleteTradingRecord(TradingRecord tradingRecord)
        {//ex DataException,InvalidOperationException
            StockMonitorEntities _dbContext = new StockMonitorEntities();

            _dbContext.TradingRecords.Remove(tradingRecord);

            _dbContext.SaveChanges();//ex DataException,InvalidOperationException
        }
    }
}
