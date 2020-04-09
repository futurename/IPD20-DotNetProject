using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StockMonitor.Models.UIClasses;

namespace GUI
{
    public class GlobalVariables
    {
        public static ConcurrentDictionary<string, string> ConcurentDictionary = new ConcurrentDictionary<string, string>();
        public static BlockingCollection<UIComapnyRow> WatchListUICompanyRows ;
        public static ConcurrentDictionary<string, CancellationTokenSource> WatchListTokenSourceDic;
        public static CancellationTokenSource DefaultTaskTokenSource ;
        public static BlockingCollection<UIComapnyRow> DefaultUICompanyRows;
        public static bool IsPseudoDataSource = true;
        public static BlockingCollection<UIComapnyRow> SearchResultUICompanyRows;
        public static CancellationTokenSource SearchResultCancellationTokenSource;
    }
}
