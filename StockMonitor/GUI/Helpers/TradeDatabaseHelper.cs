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
        {//ex IOException, InvalidOperationException
            using (StockMonitorEntities _dbContext = new StockMonitorEntities())
            {
                var companyTrading = from trade in GetReservedTradingList(reservedTrading.UserId)
                                     where trade.CompanyId == reservedTrading.CompanyId
                                     select trade;

                if (companyTrading.Count() == 0)
                {
                    _dbContext.ReservedTradings.Add(reservedTrading);
                    _dbContext.SaveChanges();
                }
                else
                {
                    throw new InvalidOperationException("One Trade reserved already with this company");
                }
            }
        }

        public static List<ReservedTrading> GetReservedTradingList(int userId)
        {
            using (StockMonitorEntities _dbContext = new StockMonitorEntities())
            {
                User tradingUser = (from user in _dbContext.Users.Include("ReservedTradings")
                                    where user.Id == userId
                                    select user).FirstOrDefault();

                if (tradingUser == null)
                {
                    throw new InvalidOperationException($"There is no User id[{userId}]");
                }

                var tradingList = tradingUser.ReservedTradings.ToList();

                tradingList.Sort((t1, t2) => t1.DueDateTime.CompareTo(t2.DueDateTime));

                return tradingList;
            }
        }

        public static void AddTradingRecord(TradingRecord tradingRecord)
        {
            using (StockMonitorEntities _dbContext = new StockMonitorEntities())
            {
                _dbContext.TradingRecords.Add(tradingRecord);
                _dbContext.SaveChanges();
            }
        }
        public static void DeleteReservedTrading(ReservedTrading reservedTrading)
        {
            using (StockMonitorEntities _dbContext = new StockMonitorEntities())
            {
                ReservedTrading trading = _dbContext.ReservedTradings.Find(reservedTrading.Id);
                if (trading == null) { throw new SystemException($"Cannot find reservedTrading[{reservedTrading.Id}]"); }
                _dbContext.ReservedTradings.Remove(trading);
                _dbContext.SaveChanges();
            }
        }
    }
}
