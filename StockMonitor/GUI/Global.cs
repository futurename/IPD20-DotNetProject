using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMonitor.Models.UIClasses;

namespace GUI
{
    class Global
    {
        public static ConcurrentDictionary<string, string> ConcurentDictionary = new ConcurrentDictionary<string, string>();
       public  static BlockingCollection<UIComapnyRow> watchList;
    }
}
