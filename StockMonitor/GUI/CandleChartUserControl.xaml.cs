using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Events;
using LiveCharts.Wpf;
using StockMonitor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
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

        public SeriesCollection SeriesCollection { get; set; }
        private string[] _labels;
        public string[] Labels
        {
            get { return _labels; }
            set
            {
                _labels = value;
                OnPropertyChanged("Labels");
            }
        }

        public CandleChartUserControl()
        {
            InitializeComponent();

            // In DesignMode, it avoid read app.config fail exception(in this case, cannot read "connectionString" from app.config)
            // DesignMode use compiled UserControl which doesn't include app.config file, so every behavior that read/connect
            // app.config file data throws exception
            // This prevents program from continues when it is design mode
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            DateTime start = DateTime.Now;
            reloadWindow();





            DateTime end = DateTime.Now;
            TimeSpan timeSpan = new TimeSpan();
            timeSpan = end - start;
            MessageBox.Show($"Time spent: {timeSpan.TotalMilliseconds} mills");
        }

        private void reloadWindow()
        {
            List<QuoteDaily> valueList;
            List<string> labelList;

            try
            {
                using (DbStockMonitor ctx = new DbStockMonitor())
                {
                    string symbol = "SGH";
                    valueList = (from price in ctx.QuoteDailies
                                 orderby price.Date
                                 where price.Symbol == symbol
                                 select price
                                 )
                                 .ToList<QuoteDaily>();

                    labelList = (from value in valueList select value.Date.ToString("yyyy-MM-dd")).ToList<string>();
                    Labels = labelList.ToArray();
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show("Cannot open file");
                return;
            }

            SeriesCollection = new SeriesCollection
            {
                new CandleSeries()
                {
                    Values = new ChartValues<OhlcPoint>(valueList)
                }
            };

            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ChartOnDataClick(object sender, ChartPoint p)
        {
            var asPixels = chartStockPrice.ConvertToPixels(p.AsPoint());
            Console.WriteLine("[EVENT] You clicked (" + p.X + ", " + p.Y + ") in pixels (" +
                            asPixels.X + ", " + asPixels.Y + ")");
        }

        private void Chart_OnDataHover(object sender, ChartPoint p)
        {
            Console.WriteLine("[EVENT] you hovered over " + p.X + ", " + p.Y);
        }

        private void ChartMouseMove(object sender, MouseEventArgs e)
        {
            var pointChartVal = chartStockPrice.ConvertToChartValues(e.GetPosition(chartStockPrice));

            Y.Text = pointChartVal.Y.ToString("N");

            if(pointChartVal.X <= Labels.Length)
            {
                txt_X_Axis.Text = Labels[(int)pointChartVal.X];
            }

            var pointMouse = e.GetPosition(chartStockPrice);
            lbl_X_Axis.Margin = new Thickness(pointMouse.X, chartStockPrice.Series.Chart.DrawMargin.Top, 0, 0);
            txt_X_Axis.Margin = new Thickness(pointMouse.X, chartStockPrice.Series.Chart.DrawMargin.Top + lbl_X_Axis.Height, 0, 0);
            lbl_Y_Axis.Margin = new Thickness(chartStockPrice.Series.Chart.DrawMargin.Left, pointMouse.Y, 0, 0);

            var chartMargin = chartStockPrice.Series.Chart.DrawMargin;
            if (pointMouse.X < chartMargin.Left || pointMouse.X > chartMargin.Width
                || pointMouse.Y < chartMargin.Top || pointMouse.Y > chartMargin.Height)
            {
                lbl_X_Axis.Visibility = Visibility.Hidden;
                lbl_Y_Axis.Visibility = Visibility.Hidden;
            }
            else
            {
                lbl_X_Axis.Visibility = Visibility.Visible;
                lbl_Y_Axis.Visibility = Visibility.Visible;
            }
        }

        private bool limitMin = false, limitMax = false;
        private void Axis_OnPreviewRangeChanged(PreviewRangeChangedEventArgs e)
        {
            //if less than -0.5, cancel
            limitMin = e.PreviewMinValue < -0.5;
            //if greater than the number of items on our array plus a 0.5 offset, stay on max limit
            limitMax = e.PreviewMaxValue > Labels.Length - 0.5;

            //finally if the axis range is less than 1, cancel the event
            if (e.PreviewMaxValue - e.PreviewMinValue < 1) e.Cancel = true;
            Console.WriteLine("Max Value: {0}, Min Value: {1}", e.PreviewMaxValue, e.PreviewMinValue);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = DateTime.Now;
            reloadWindow();
            DateTime end = DateTime.Now;
            TimeSpan timeSpan = new TimeSpan();
            timeSpan = end - start;
            MessageBox.Show($"Time spent: {timeSpan.TotalMilliseconds} mills");
        }

        private void chartStockPrice_MouseEnter(object sender, MouseEventArgs e)
        {
            lbl_X_Axis.Height = chartStockPrice.Series.Chart.DrawMargin.Height;
            lbl_Y_Axis.Width = chartStockPrice.Series.Chart.DrawMargin.Width;
        }

        private void Ax_RangeChanged(LiveCharts.Events.RangeChangedEventArgs eventArgs)
        {
            Axis ax = (Axis)eventArgs.Axis;
            if (limitMax) ax.MaxValue = Labels.Length + 0.5;
            if (limitMin) ax.MinValue = -0.5;
        }
    }
}
