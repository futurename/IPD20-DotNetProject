using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace StockMonitor.Models.UIClasses

{
    public class UIComapnyRow : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        private double _price;

        public double Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged("Price");
                OnPropertyChanged("PriceChange");
                OnPropertyChanged("ChangePercentage");
                OnPropertyChanged("Volume");
            }
        }
        public int CompanyId { get; set; }
        public double ChangePercentage { get; set; }

        public double PriceChange { get; set; }
        public long Volume { get; set; }
        public double Open { get; set; }
        public string MarketCapital { get; set; }
        public string PriceToEarningRatio { get; set; }
        public string PriceToSalesRatio { get; set; }
        public string Industry { get; set; }
        public string Sector { get; set; }
        public byte[] Logo { get; set; }
        public string Description { get; set; }
        public string CompanyName { get; set; }


        public override string ToString()
        {
            return $"{Symbol}, {CompanyName}:{Price}, {ChangePercentage}%,{PriceChange},{Sector}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
