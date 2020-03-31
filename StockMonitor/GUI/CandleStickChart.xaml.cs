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
using System.Windows.Shapes;

namespace GUI
{
    /// <summary>
    /// Interaction logic for CandleStickChart.xaml
    /// </summary>
    public partial class CandleStickChart : Window
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


        public CandleStickChart()
        {
            InitializeComponent();
            List<TestQuoteDaily> valueList;
            List<string> labelList;

            try
            {
                using (DbStockMonitor ctx = new DbStockMonitor())
                {
                    valueList = (from price in ctx.TestQuoteDailies select price).ToList<TestQuoteDaily>();
                    labelList = (from value in valueList select value.Date.ToString("yyyy-MM-dd")).ToList<string>();
                    Labels = labelList.ToArray();
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show(this, "Cannot open file");
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
            var point = chartStockPrice.ConvertToChartValues(e.GetPosition(chartStockPrice));

            Y.Text = point.Y.ToString("N");
        }


        private void Axis_OnPreviewRangeChanged(PreviewRangeChangedEventArgs e)
        {
            ////if less than -0.5, cancel
            //if (e.PreviewMinValue < -0.5) e.Cancel = true;
            ////if greater than the number of items on our array plus a 0.5 offset, stay on max limit
            //if (e.PreviewMaxValue > LineSeries.Values.Count - 0.5) e.Cancel = true;

            ////finally if the axis range is less than 1, cancel the event
            //if (e.PreviewMaxValue - e.PreviewMinValue < 1) e.Cancel = true;
            Console.WriteLine("Max Value: {0}, Min Value: {1}",e.PreviewMaxValue,e.PreviewMinValue);
        }
    }
}
