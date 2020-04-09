using GUI;
using NUnit.Framework;
using StockMonitor;
using StockMonitor.Helpers;
using StockMonitor.Models.UIClasses;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace UnitTestProject
{
    [TestFixture]
    public class UnitTest1
    {
        int userId = 3;
        StockMonitorEntities ctx;
        
        [SetUp]
        public void Setup()
        {
            ctx = new StockMonitorEntities();
            
            DateTime date = DateTime.Now.AddDays(1).Date;
            DateTime time = date.AddHours(9.5);

            int[] companyIdArr = { 7, 1376,190,3486 };
            ctx.ReservedTradings.Add(
                new ReservedTrading(companyIdArr[0], userId , TradeEnum.Buy, "100",
                        "10", "100", DateTime.Now.AddDays(-1), DateTime.Now)
            );
            ctx.ReservedTradings.Add(
                new ReservedTrading(companyIdArr[1], userId, TradeEnum.Buy, "50",
                        "120", "130", DateTime.Now.AddDays(-1), DateTime.Now)
            );
            ctx.ReservedTradings.Add(
                new ReservedTrading(companyIdArr[2], userId, TradeEnum.Sell, "20",
                        "200", "10000", DateTime.Now.AddDays(1), DateTime.Now)
            );
            ctx.ReservedTradings.Add(
                new ReservedTrading(companyIdArr[3], userId, TradeEnum.Sell, "10",
                        "10", "10000", DateTime.Now.AddDays(1), time.AddHours(1))
            );
            ctx.SaveChanges();

            GlobalVariables.DefaultUICompanyRows = new BlockingCollection<UIComapnyRow>();
            GlobalVariables.DefaultTaskTokenSource = new CancellationTokenSource();
            GlobalVariables.WatchListUICompanyRows = new BlockingCollection<UIComapnyRow>();
            GlobalVariables.WatchListTokenSourceDic = new ConcurrentDictionary<string, CancellationTokenSource>();

            GlobalVariables.WatchListUICompanyRows = new BlockingCollection<UIComapnyRow>();

            Init();
        }

        private async void Init()
        {
            List<Task<UIComapnyRow>> watchlistTasks = GUIDataHelper.GetWatchUICompanyRowTaskList(userId);
            foreach (var task in watchlistTasks)
            {
                UIComapnyRow oneRow = await task;
                GlobalVariables.WatchListUICompanyRows.TryAdd(oneRow);
            }

            await Task.WhenAll(watchlistTasks.ToArray());

            StockTrader stockTrader = new StockTrader(null, userId);

        }

        [TearDown]
        public void TearDown()
        {
            var list = from trading in ctx.ReservedTradings
                       select trading;
            ctx.ReservedTradings.RemoveRange(list.ToList<ReservedTrading>());
            ctx.SaveChanges();
        }

        [Test]
        public void IsCheckingPrice()
        {
            int count = 0;
            while (count < 20)
            {
                Thread.Sleep(1000);
                count++;
            }
        }
    }
}
