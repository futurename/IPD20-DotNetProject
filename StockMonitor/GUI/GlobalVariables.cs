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
        public static BlockingCollection<UIComapnyRow> watchList;
        public static ConcurrentDictionary<int, string> taskManager = new ConcurrentDictionary<int, string>();
        public static ConcurrentDictionary<int, CancellationToken> DefaultTaskManager = new ConcurrentDictionary<int, CancellationToken>();
        public static ConcurrentDictionary<int, CancellationToken> WatchListTaskManager = new ConcurrentDictionary<int, CancellationToken>();
    }
}
