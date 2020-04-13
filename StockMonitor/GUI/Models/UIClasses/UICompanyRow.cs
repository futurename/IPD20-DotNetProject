using StockMonitor.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GUI;

namespace StockMonitor.Models.UIClasses

{
    public class UIComapnyRow : INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public double Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged("Price");
                OnPropertyChanged("PriceChange");
                OnPropertyChanged("ChangePercentage");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public UIComapnyRow(
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
            int companyId,
            string ceo)
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
            CEO = ceo;
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
            CEO = company.CEO;
           
        }

        public int Id { get; set; }
        public string Symbol { get; set; }
        private double _price;

      
        public int CompanyId { get; set; }
        public double ChangePercentage { get; set; }
        public double PriceChange { get; set; }
        private long _volume;

        public long Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                OnPropertyChanged("Volume");
            }
        }
        public double Open { get; set; }
        public double MarketCapital { get; set; }

        private double _notifyPriceLow;

        public double NotifyPriceLow
        {
            get => _notifyPriceLow;
            set
            {
                _notifyPriceLow = value;
                OnPropertyChanged("NotifyPriceLow");
            }
        }

        private double _notifyPriceHigh;

        public double NotifyPriceHigh
        {
            get => _notifyPriceHigh;
            set
            {
                _notifyPriceHigh = value;
                OnPropertyChanged("NotifyPriceHigh");
            }
        }

        private void SetMarketCapital(string marketCapitalStr)
        {
            try
            {
                MarketCapital = double.Parse(marketCapitalStr, CultureInfo.InvariantCulture); //ex
            }
            catch (SystemException ex)
            {
                Console.Out.WriteLine($"\n!!!!Parsing marketCapital string to double exception, {marketCapitalStr}. {ex.Message}");
            }
        }
        public double PriceToEarningRatio { get; set; }
        private void SetPriceToEarningRatio(string priceToEarningRatioStr)
        {
            try
            {
                PriceToEarningRatio = double.Parse(priceToEarningRatioStr, CultureInfo.InvariantCulture); //ex
            }
            catch (SystemException ex)
            {
                Console.Out.WriteLine($"\n!!!!Parsing priceToEarningRatio string to double exception, {priceToEarningRatioStr}. {ex.Message}");
            }
        }

        public ImageSource ByteToImage(byte[] imageData)
        {
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();

            ImageSource imgSrc = biImg as ImageSource;

            return imgSrc;
        }

        public double PriceToSalesRatio { get; set; }
        private void SetPriceToSalesRatio(string priceToSalesRatioStr)
        {
            try
            {
                PriceToSalesRatio = double.Parse(priceToSalesRatioStr, CultureInfo.InvariantCulture); //ex
            }
            catch (SystemException ex)
            {
                Console.Out.WriteLine($"\n!!!!Parsing priceToSalesRatio string to double exception, {priceToSalesRatioStr}. {ex.Message}");
            }
        }
        public string Industry { get; set; }
        public byte[] Logo { get; set; }
        public string Description { get; set; }
        public string CompanyName { get; set; }
        public string CEO { get; set; }

        public override string ToString()
        {
            return $"{Symbol}, {CompanyName}:{Price}, {ChangePercentage}%,{PriceChange}, High: {NotifyPriceHigh}, Low: {NotifyPriceLow}";
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
        
    }

    public class PriceColorValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (double.Parse(value.ToString()) < 0)
                {
                    return "RED";
                }
                else if (double.Parse(value.ToString()) > 0)
                {
                    return "GREEN";
                }
            }
            return "BLACK";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class NotifySettingValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (Math.Abs(double.Parse(value.ToString())) < 0.0001)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


}
