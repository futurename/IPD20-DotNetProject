using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Events;
using LiveCharts.Wpf;
using StockMonitor;
using StockMonitor.Helpers;
using StockMonitor.Models.ApiModels;
using StockMonitor.Models.UIClasses;
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
using static GUI.Wrapper;

namespace GUI
{
    /// <summary>
    /// Interaction logic for CandleChartUserControl.xaml
    /// </summary>
    public partial class CandleChartUserControl : UserControl
    {
        private const double NumberOfValuesPerPixel = 0.25;
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }

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

            DrawCandleStick();
        }

        private async void DrawCandleStick()// need to be async because it has Task(thread)
        {
            List<Fmg1MinQuoteWapper> valueList;
            List<string> labelList;
            int numOfVal = (int)(NumberOfValuesPerPixel * gridChartContainer.Width);
            try
            {
                using (DbStockMonitor ctx = new DbStockMonitor())
                {
                    string symbol = "SGH";
                    var minValueList = await RetrieveJsonDataHelper.RetrieveAllFmg1MinQuote(symbol); // Task(thread)

                    valueList = (from fmg1MinQuote in minValueList.Take(numOfVal)
                                 orderby fmg1MinQuote.Date 
                                 select new Fmg1MinQuoteWapper(fmg1MinQuote)
                                 ).ToList<Fmg1MinQuoteWapper>();

                    labelList = (from value in valueList select value.Date.ToString("hh:mm")).ToList<string>();
                    Labels = labelList.ToArray();
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show("Cannot open file");
                return;
            }

            SeriesCollection = new SeriesCollection {
                new CandleSeries()
                {
                    Values = new ChartValues<OhlcPoint>(valueList),
                    Fill = Brushes.Transparent
                }
            };

            DataContext = this;

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

        bool isCrossLineSet = false;
        private void ChartMouseMove(object sender, MouseEventArgs e)
        {
            var pointChartVal = chartStockPrice.ConvertToChartValues(e.GetPosition(chartStockPrice));

            Y.Text = pointChartVal.Y.ToString("N");

            if ((pointChartVal.X < Labels.Length - 1) && pointChartVal.X >= 0)//FIXME: exception when mouse already entered and move in chart loading
            {
                txt_X_Axis.Text = Labels[(int)Math.Round(pointChartVal.X)];
            }

            var pointMouse = e.GetPosition(chartStockPrice);
            var chartMargin = chartStockPrice.Series.Chart.DrawMargin;
            if (chartMargin == null) { return; }
            if (!isCrossLineSet) {
                lbl_X_Axis.Height = chartMargin.Height;
                lbl_Y_Axis.Width = chartMargin.Width;
            }


            lbl_X_Axis.Margin = new Thickness(pointMouse.X, chartMargin.Top, 0, 0);
            txt_X_Axis.Margin = new Thickness(pointMouse.X, chartMargin.Top + lbl_X_Axis.Height, 0, 0);
            lbl_Y_Axis.Margin = new Thickness(chartMargin.Left, pointMouse.Y, 0, 0);
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
            //if less than -0.5, cancel
            limitMin = e.PreviewMinValue < -0.5;

            //if greater than the number of items on our array plus a 0.5 offset, stay on max limit
            limitMax = e.PreviewMaxValue > Labels.Length - 0.5;

            // if screen is left-end and zoom-in max 
            if (e.PreviewMaxValue < 15 && e.PreviewMinValue < 0) e.Cancel = true;

            //finally if the axis range is less than 1, cancel the event
            if (e.PreviewMaxValue - e.PreviewMinValue < 15) e.Cancel = true;

            Console.WriteLine("limitMin:{0}|limitMax:{1}", limitMin, limitMax);
            Console.WriteLine("PreviewMaxValue:{0}| PreviewMinValue:{1}", e.PreviewMaxValue, e.PreviewMinValue);
            Console.WriteLine("ActualMaxValueX:{0}|ActualMinValue:{1}",
                chartStockPrice.AxisX[0].ActualMaxValue,
                chartStockPrice.AxisX[0].ActualMinValue);

        }
        private void Ax_RangeChanged(LiveCharts.Events.RangeChangedEventArgs eventArgs)
        {
            Axis ax = (Axis)eventArgs.Axis;
            if (limitMax) ax.MaxValue = Labels.Length;
            if (limitMin) ax.MinValue = 0;
        }

        private void btReload_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = DateTime.Now;
            DrawCandleStick();
            DateTime end = DateTime.Now;
            TimeSpan timeSpan = new TimeSpan();
            timeSpan = end - start;
            Console.WriteLine($"Time spent: {timeSpan.TotalMilliseconds} mills");
        }
    }
}
