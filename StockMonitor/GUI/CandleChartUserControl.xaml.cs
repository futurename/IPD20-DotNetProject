using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Events;
using LiveCharts.Wpf;
using StockMonitor;
using StockMonitor.Helpers;
using StockMonitor.Models.ApiModels;
using StockMonitor.Models.JSONModels;
using StockMonitor.Models.UIClasses;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GUI
{
    /// <summary>
    /// Interaction logic for CandleChartUserControl.xaml
    /// </summary>
    public partial class CandleChartUserControl : UserControl
    {
        public ChartValues<QuoteDaily> ChartValues { get; set; }
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
        public Func<double, string> DateTimeFormatter { get; set; }

        private string _symbol;
        public string Symbol {
            get { return _symbol; }
            set
            {
                _symbol = value;
                if(value == "")
                {
                    ChartValues.Clear();
                } else
                {
                    txtSymbol.Text = value;
                    DrawCandleChart();
                }
            }
        }
        public CandleChartUserControl()
        {
            GlobalVariables.CandleChartUserControl = this;

            InitializeComponent();


            // In DesignMode, it avoid read app.config fail exception(in this case, cannot read "connectionString" from app.config)
            // DesignMode use compiled UserControl which doesn't include app.config file, so every behavior that read/connect
            // app.config file data throws exception
            // This prevents program from continues when it is design mode
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }


            var mapper = Mappers.Financial<QuoteDaily>()
                .Open(model => model.Open)
                .Close(model => model.Close)
                .High(model => model.High)
                .Low(model => model.Low)
                .X(model => model.Date.Ticks);

            Charting.For<QuoteDaily>(mapper);

            ChartValues = new ChartValues<QuoteDaily>();

            DateTimeFormatter = value => new DateTime((long)value).ToString("yyyy-MM-dd");

            AxisStep = TimeSpan.FromDays(20).Ticks;

            AxisUnit = TimeSpan.TicksPerDay;

            SetAxisLimits(DateTime.Now, DateTime.Now.AddDays(-100));

            DataContext = this;
        }

        void SetAxisLimits(DateTime LastDay, DateTime FirstDay)
        {
            AxisMax = LastDay.Ticks;
            AxisMin = FirstDay.Ticks;
        }

        private void DrawCandleChart()
        {
            try {
                ChartValues.Clear();
                if(chartStockPrice.Series.Chart.AreComponentsLoaded) {
                    chartStockPrice.Series.Chart.ClearZoom();
                }

                var priceList = (from dailyPrice in GUIDataHelper.GetQuoteDailyListFromDb(Symbol)
                         orderby dailyPrice.Date descending
                         select dailyPrice).Take(100).ToList<QuoteDaily>();
                priceList.Reverse();

                ChartValues.AddRange(priceList);

                SetAxisLimits(priceList.Last().Date,priceList.First().Date);
            }
            catch (IOException)
            {
                MessageBox.Show("[Internel Error]Cannot load data");
                return;
            }
        }

        private void ChartMouseMove(object sender, MouseEventArgs e)
        {
            var pointChartVal = chartStockPrice.ConvertToChartValues(e.GetPosition(chartStockPrice));

            txtPrice.Text = pointChartVal.Y.ToString("N");

            if (!chartStockPrice.IsLoaded){ return; }// Check chart loaded
            if (chartStockPrice.Series == null) { return; }// Check chart loaded

            var chartMargin = chartStockPrice.Series.Chart.DrawMargin;

            lbl_X_Axis.Height = chartMargin.Height;
            lbl_Y_Axis.Width = chartMargin.Width;

            txt_X_Axis.Text =new DateTime((long)pointChartVal.X).ToString("yyyy-MM-dd");

            var pointMouse = e.GetPosition(chartStockPrice);

            lbl_X_Axis.Margin = new Thickness(pointMouse.X, chartMargin.Top , 0, 0);
            txt_X_Axis.Margin = new Thickness(pointMouse.X - chartMargin.Left, chartMargin.Top + lbl_X_Axis.Height, 0, 0);
            lbl_Y_Axis.Margin = new Thickness(chartMargin.Left, pointMouse.Y, 0, 0);

            if (pointMouse.X < chartMargin.Left || pointMouse.X > chartMargin.Width + chartMargin.Left
                || pointMouse.Y < chartMargin.Top || pointMouse.Y > chartMargin.Height + chartMargin.Top)
            {
                lbl_X_Axis.Visibility = Visibility.Hidden;
                lbl_Y_Axis.Visibility = Visibility.Hidden;
                txt_X_Axis.Visibility = Visibility.Hidden;
            }
            else
            {
                lbl_X_Axis.Visibility = Visibility.Visible;
                lbl_Y_Axis.Visibility = Visibility.Visible;
                txt_X_Axis.Visibility = Visibility.Visible;
            }
        }

        private void ChartMouseLeave(object sender, MouseEventArgs e)
        {
            lbl_X_Axis.Visibility = Visibility.Hidden;
            lbl_Y_Axis.Visibility = Visibility.Hidden;
            txt_X_Axis.Visibility = Visibility.Hidden;
        }

        private void ChartMouseEnter(object sender, MouseEventArgs e)
        {
            lbl_X_Axis.Visibility = Visibility.Visible;
            lbl_Y_Axis.Visibility = Visibility.Visible;
            txt_X_Axis.Visibility = Visibility.Visible;
        }


        private bool limitMin = false, limitMax = false;
        private void Axis_OnPreviewRangeChanged(PreviewRangeChangedEventArgs e)
        {
            if (chartStockPrice.Series == null) { return; } 
            //if less than -0.5, cancel
            limitMin = e.PreviewMinValue < AxisMin;

            //if greater than the number of items on our array plus a 0.5 offset, stay on max limit
            limitMax = e.PreviewMaxValue > AxisMax;

            //finally if the axis range is less than 1, cancel the event
            if (e.PreviewMaxValue - e.PreviewMinValue < AxisUnit * 20) e.Cancel = true;
        }
        private void Ax_RangeChanged(LiveCharts.Events.RangeChangedEventArgs eventArgs)
        {
            Axis ax = (Axis)eventArgs.Axis;
            if (limitMax)
            {
                ax.MaxValue = AxisMax;
            }
            if (limitMin)
            {
                ax.MinValue = AxisMin;
            }
        }


        private void btReload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                progBarChart.Visibility = Visibility.Visible;
                DrawCandleChart();
                progBarChart.Visibility = Visibility.Hidden;
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


        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
