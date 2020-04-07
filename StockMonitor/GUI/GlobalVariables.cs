using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StockMonitor.Models.UIClasses;

namespace GUI
{
    class GlobalVariables
    {
        public static ConcurrentDictionary<string, string> ConcurentDictionary = new ConcurrentDictionary<string, string>();
        public static BlockingCollection<UIComapnyRow> WatchListUICompanyRows = new BlockingCollection<UIComapnyRow>();
       
       
        public static ConcurrentDictionary<int, CancellationTokenSource> WatchListTokenSourceDic = new ConcurrentDictionary<int, CancellationTokenSource>();
        public static CancellationTokenSource DefaultTaskTokenSource = new CancellationTokenSource();
        public static BlockingCollection<UIComapnyRow> DefaultUICompanyRows = new BlockingCollection<UIComapnyRow>();
    }
}
