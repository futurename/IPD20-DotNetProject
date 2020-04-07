using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Events;
using LiveCharts.Wpf;
using StockMonitor;
using StockMonitor.Helpers;
using StockMonitor.Models.ApiModels;
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
        public static readonly DependencyProperty SelectedSymbolProperty =
        DependencyProperty.Register("SelectedSymbol", typeof(string), typeof(UserControl), new FrameworkPropertyMetadata(null));

        public string SelectedSymbol
        {
            get { return (string)GetValue(SelectedSymbolProperty); }
            set { SetValue(SelectedSymbolProperty, value); }
        }


        private const double NumberOfValuesPerPixel = 0.40;
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
        }

        private async void DrawCandleStick()// need to be async because it has Task(thread)
        {
            
            while (gridChartContainer.ActualWidth == 0) {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(200);  
            }

            List<Fmg1MinQuote> valueList;
            List<string> labelList;

            int numOfVal = (int)(gridChartContainer.ActualWidth * NumberOfValuesPerPixel);

            try
            {
                using (DbStockMonitor ctx = new DbStockMonitor())
                {
                    string symbol;
                    while(!GlobalVariables.ConcurentDictionary.TryGetValue("symbol",out symbol)) {
                        ct.ThrowIfCancellationRequested();
                        await Task.Delay(200); 
                    }
                    var minValueList = await RetrieveJsonDataHelper.RetrieveAllFmg1MinQuote(symbol); // Task(thread)

                    valueList = (from fmg1MinQuote in minValueList.Take(50)
                                 orderby fmg1MinQuote.Date
                                 select fmg1MinQuote
                                 ).ToList<Fmg1MinQuote>();
                    labelList = (from value in valueList select value.Date.ToString("hh:mm")).ToList<string>();
                    Labels = labelList.ToArray();
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show("Cannot open file");
                return;
            }

            this.Dispatcher.Invoke(() =>
            {
                chartStockPrice.Series.Clear();

                if (chartStockPrice.Model != null)
                {
                    //chartStockPrice.Model.ClearZoom();//FIXME
                }
                ct.ThrowIfCancellationRequested();
                chartStockPrice.Series.Add(
                    new CandleSeries()
                    {
                        Values = new ChartValues<OhlcPoint>(valueList)
                    }
                );
                this.DataContext = this;
            });
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

            txtPrice.Text = pointChartVal.Y.ToString("N");

            if(Labels == null) { return; }//FIXME : Clean code
            if (!chartStockPrice.IsLoaded){ return; }// Check chart loaded
            if (chartStockPrice.Series == null) { return; }// Check chart loaded

            var chartMargin = chartStockPrice.Series.Chart.DrawMargin;
            if (chartMargin == null) { return; }
            if (!isCrossLineSet)
            {
                lbl_X_Axis.Height = chartMargin.Height;
                lbl_Y_Axis.Width = chartMargin.Width;
            }

            if ((pointChartVal.X < Labels.Length - 1) && pointChartVal.X >= 0)//FIXME: exception when mouse already entered and move in chart loading
            {
                txt_X_Axis.Text = Labels[(int)Math.Round(pointChartVal.X)];
            }

            var pointMouse = e.GetPosition(chartStockPrice);
            var dfe = chartStockPrice.Model;

            lbl_X_Axis.Margin = new Thickness(pointMouse.X, chartMargin.Top , 0, 0);
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
        private const int MinLabels = 15;
        private void Axis_OnPreviewRangeChanged(PreviewRangeChangedEventArgs e)
        {
            if(Labels == null) { return; }// No Chart
            if (chartStockPrice.Series == null) { return; } 
            //if less than -0.5, cancel
            limitMin = e.PreviewMinValue < -0.5;

            //if greater than the number of items on our array plus a 0.5 offset, stay on max limit
            limitMax = e.PreviewMaxValue > Labels.Length + 0.5;

            // if screen is left-end and zoom-in max 
            if (e.PreviewMaxValue < MinLabels && e.PreviewMinValue < 0) {
                e.Cancel = true;//FIXME: range
            }
            else if (e.PreviewMinValue > Labels.Length - MinLabels && e.PreviewMaxValue > Labels.Length) 
            {// if screen is right-end and zoom-in max 
                e.Cancel = true;//FIXME: range
            }                                                                                                                


            //finally if the axis range is less than 1, cancel the event
            if (e.PreviewMaxValue - e.PreviewMinValue < MinLabels) e.Cancel = true;

            Console.WriteLine("limitMin:{0}|limitMax:{1}", limitMin, limitMax);
            Console.WriteLine("PreviewMaxValue:{0}| PreviewMinValue:{1}", e.PreviewMaxValue, e.PreviewMinValue);
            Console.WriteLine("ActualMaxValueX:{0}|ActualMinValue:{1}",
                chartStockPrice.AxisX[0].ActualMaxValue,
                chartStockPrice.AxisX[0].ActualMinValue);
        }
        private void Ax_RangeChanged(LiveCharts.Events.RangeChangedEventArgs eventArgs)
        {
            Axis ax = (Axis)eventArgs.Axis;
            if (limitMax)
            {
                ax.MaxValue = Labels.Length +0.5;
            }
            if (limitMin)
            {
                ax.MinValue = -0.5;
            }
        }


        public CancellationTokenSource tokenSource;
        public CancellationToken ct;
        private void btReload_Click(object sender, RoutedEventArgs e)
        {
            if(tokenSource != null)
            {
                tokenSource.Cancel();
            }

            try
            {
                tokenSource = new CancellationTokenSource();
                ct = tokenSource.Token;
                Task.Factory.StartNew(DrawCandleStick,tokenSource.Token);
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

        private void txtSymbol_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (tokenSource != null)
            {
                tokenSource.Cancel();
            }

            try
            {
                tokenSource = new CancellationTokenSource();
                ct = tokenSource.Token;
                Task.Factory.StartNew(DrawCandleStick, tokenSource.Token);
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
}
