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

namespace GUI
{
    /// <summary>
    /// Interaction logic for CandleChartUserControl.xaml
    /// </summary>
    public partial class CandleChartUserControl : UserControl
    {

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

            try
            {
                using (DbStockMonitor ctx = new DbStockMonitor())
                {
                    string symbol = "SGH";
                    var minValueList = await RetrieveJsonDataHelper.RetrieveAllFmg1MinQuote(symbol); // Task(thread)

                    valueList = (from fmg1MinQuote in minValueList.Take(200)
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

            SeriesCollection = new SeriesCollection
            {
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

        private void ChartMouseMove(object sender, MouseEventArgs e)
        {
            var pointChartVal = chartStockPrice.ConvertToChartValues(e.GetPosition(chartStockPrice));

            Y.Text = pointChartVal.Y.ToString("N");

            if(pointChartVal.X < Labels.Length  && pointChartVal.X >= 0)
            {
                txt_X_Axis.Text = Labels[(int)pointChartVal.X];
            }
            
            var pointMouse = e.GetPosition(chartStockPrice);
            var chartMargin = chartStockPrice.Series.Chart.DrawMargin;

            lbl_X_Axis.Margin = new Thickness(pointMouse.X, chartMargin.Top, 0, 0);
            txt_X_Axis.Margin = new Thickness(pointMouse.X, chartMargin.Top + lbl_X_Axis.Height, 0, 0);
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

        private void btReload_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = DateTime.Now;
            DrawCandleStick();
            DateTime end = DateTime.Now;
            TimeSpan timeSpan = new TimeSpan();
            timeSpan = end - start;
            Console.WriteLine($"Time spent: {timeSpan.TotalMilliseconds} mills");
        }

        private void chartStockPrice_MouseEnter(object sender, MouseEventArgs e)
        {
            if(chartStockPrice == null) { return; }
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
    
    class Fmg1MinQuoteWapper : OhlcPoint
    {
        public Fmg1MinQuoteWapper(Fmg1MinQuote fmg1MinQuote)
        {
            Open = fmg1MinQuote.Open;
            Low = fmg1MinQuote.Low;
            High = fmg1MinQuote.High;
            Close = fmg1MinQuote.Close;
            Date = fmg1MinQuote.Date;
        }

        public DateTime Date { get; set; }
        public long Volume { get; set; }
    }

}
