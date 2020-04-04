using StockMonitor.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace StockMonitor.Models.UIClasses

{
    public class UIComapnyRow : INotifyPropertyChanged
    {
        public UIComapnyRow (//ex: FormatException
            string symbol, 
            double price,
            double openPrice,
            long volume,
            double changePercentage,
            double priceChange, 
            string marketCapitalStr, 
            string priceToEarningRatioStr, 
            string priceToSalesRatioStr, 
            string industry, 
            byte[] logo, 
            string description, 
            int companyId)
        {
            Symbol = symbol;
            Price = price;
            Open = openPrice;
            Volume = volume;
            ChangePercentage = changePercentage;
            PriceChange = priceChange;
            SetMarketCapital(marketCapitalStr); //ex: FormatException
            SetPriceToEarningRatio(priceToEarningRatioStr); //ex: FormatException
            SetPriceToSalesRatio(priceToSalesRatioStr); //ex: FormatException
            Industry = industry;
            Logo = logo;
            Description = description;
            CompanyId = companyId;
        }

        public UIComapnyRow( //FormatException
            Company company, 
            FmgQuoteOnlyPrice fmgQuoteOnlyPrice, 
            FmgSingleQuote singleQuote)
        {
            Symbol = company.Symbol;
            Price = fmgQuoteOnlyPrice.Price;
            Open = singleQuote.open;
            Volume = singleQuote.volume;
            PriceChange = Price - Open;
            ChangePercentage = PriceChange / Open * 100;
            SetMarketCapital(company.MarketCapital);//ex FormatException
            SetPriceToEarningRatio(company.PriceToEarningRatio);//ex FormatException
            SetPriceToSalesRatio(company.PriceToSalesRatio);//ex FormatException
            Industry = company.Industry;
            Logo = company.Logo;
            Description = company.Description;
            CompanyId = company.Id;
            CompanyName = company.CompanyName;
        }

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
        public double MarketCapital { get; set; }
        private void SetMarketCapital(string marketCapitalStr)
        {
            MarketCapital = double.Parse(marketCapitalStr, CultureInfo.InvariantCulture);//ex
        }
        public double PriceToEarningRatio { get; set; }
        private void SetPriceToEarningRatio(string priceToEarningRatioStr)
        {
            PriceToEarningRatio = double.Parse(priceToEarningRatioStr, CultureInfo.InvariantCulture);//ex
        }
        public double PriceToSalesRatio { get; set; }
        private void SetPriceToSalesRatio(string priceToSalesRatioStr)
        {
            PriceToSalesRatio = double.Parse(priceToSalesRatioStr, CultureInfo.InvariantCulture);//ex
        }
        public string Industry { get; set; }
        public byte[] Logo { get; set; }
        public string Description { get; set; }
        public string CompanyName { get; set; }

        public override string ToString()
        {
            return $"{Symbol}, {CompanyName}:{Price}, {ChangePercentage}%,{PriceChange}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (double.Parse(value.ToString()) < 0)
                {
                    return 1;
                }

                return 2;
            }

            return 3;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
