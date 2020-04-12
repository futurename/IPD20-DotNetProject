﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StockMonitor.Models.UIClasses;
using ToastNotifications;

namespace GUI
{
    class GlobalVariables
    {
        public enum CurrentDataSource
        {
            Default,
            SearchResult,
            WatchList
        };

        public static ConcurrentDictionary<string, string> ConcurentDictionary = new ConcurrentDictionary<string, string>();
        public static BlockingCollection<UIComapnyRow> WatchListUICompanyRows ;
        public static ConcurrentDictionary<string, CancellationTokenSource> WatchListTokenSourceDic;
        public static CancellationTokenSource DefaultTaskTokenSource ;
        public static BlockingCollection<UIComapnyRow> DefaultUICompanyRows;
        public static bool IsPseudoDataSource = true;
        public static BlockingCollection<UIComapnyRow> SearchResultUICompanyRows;
        public static List<Company> SearchResultCompanies;
        public static CancellationTokenSource SearchResultCancellationTokenSource;

        public static Notifier Notifier { get; set; }
        public static SearchStockUserControl SearchStockUserControl { get; set; }
        public static WatchListUserControl WatchListUserControl { get; set; }
        public static CandleChartUserControl CandleChartUserControl { get; set; }
        public static MainWindow MainWindow { get; set; }
        public static StockTrader StockTrader { get; set; }
    }
    
}
