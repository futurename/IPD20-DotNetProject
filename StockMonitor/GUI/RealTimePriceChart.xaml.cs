using LiveCharts;
using LiveCharts.Configurations;
using StockMonitor.Helpers;
using StockMonitor.Models.ApiModels;
using StockMonitor.Models.UIClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GUI
{
    /// <summary>
    /// Interaction logic for RealTimePriceChart.xaml
    /// </summary>
    public partial class RealTimePriceChart : Window, INotifyPropertyChanged
    {
        public string Symbol;
        public ChartValues<FmgQuoteOnlyPriceWrapper> ChartValues { get; set; }
        public Func<double, string> DateTimeFormatter { get; set; }
        public double AxisStep { get; set; }
        public double AxisUnit { get; set; }

        double _axisMax;
        double _axisMin;

        public double AxisMax
        {
            get { return _axisMax; }
            set
            {
                _axisMax = value;
                OnPropertyChanged("AxisMax");
            }
        }
        public double AxisMin
        {
            get { return _axisMin; }
            set
            {
                _axisMin = value;
                OnPropertyChanged("AxisMin");
            }
        }
        public bool IsReading { get; set; }
        public RealTimePriceChart(UIComapnyRow selCompany)
        {
            InitializeComponent();

            Symbol = selCompany.Symbol;

            txtSymbol.Text = Symbol;

            var mappers = Mappers.Xy<FmgQuoteOnlyPriceWrapper>()
                .X(model => model.Time.Ticks)
                .Y(model => model.Price);

            Charting.For<FmgQuoteOnlyPriceWrapper>(mappers);

            ChartValues = new ChartValues<FmgQuoteOnlyPriceWrapper>();

            DateTimeFormatter = value => new DateTime((long) value).ToString("mm:ss");

            AxisStep = TimeSpan.FromSeconds(1).Ticks;

            AxisUnit = TimeSpan.TicksPerSecond;

            SetAxisLimits(DateTime.Now);

            IsReading = false;

            DataContext = this;
        }

        async Task Read(CancellationToken ct)
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(500);
                
                //FmgQuoteOnlyPrice realTimeQuote = await RetrieveJsonDataHelper.RetrieveFmgQuoteOnlyPrice(Symbol);
                var r = new Random();

                double random = r.NextDouble() * 50 + 50;

                DateTime now = DateTime.Now;
                ChartValues.Add(new FmgQuoteOnlyPriceWrapper() { Price = random, Time = now });

                SetAxisLimits(now);
                //txtPrice.Text = "$" + realTimeQuote.Price.ToString("N2");
                txtPrice.Text = "$" + random.ToString("N2");

                if (ChartValues.Count > 20) { ChartValues.RemoveAt(0); }
            }
        }

        void SetAxisLimits(DateTime now)
        {
            AxisMax = now.Ticks + TimeSpan.FromSeconds(1).Ticks;
            AxisMin = now.Ticks - TimeSpan.FromSeconds(13).Ticks;
        }


        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        CancellationTokenSource tokenSource;
        private async void btToggleReading_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IsReading = !IsReading;
                if (IsReading)
                {
                    tokenSource = new CancellationTokenSource();
                    btToggleReading.Content = "Stop";
                    await Read(tokenSource.Token);
                }
                else
                {
                    btToggleReading.Content = "Start";
                    tokenSource.Cancel();
                    tokenSource.Dispose();
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Drawing canceled.\r\n");
            }
            catch (Exception)
            {
                Console.WriteLine("Drawing failed.\r\n");
            }
        }
    }
    public class FmgQuoteOnlyPriceWrapper
    {
        public double Price { get; set; }
        public DateTime Time { get; set; }
    }
}
