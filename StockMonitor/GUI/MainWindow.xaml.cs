using System;
using System.Collections.Generic;
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
using StockMonitor.Helpers;
using StockMonitor.Models.UIClasses;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;


namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isTimerRunning;
        private CancellationTokenSource timerTokenSource;
        public void SnackbarMessage(string message)
        {
            //use the message queue to send a message.
            var messageQueue = SnackbarTrading.MessageQueue;

            //the message queue can be called from any thread
            Task.Factory.StartNew(() => messageQueue.Enqueue(message));
        }
        public MainWindow()
        {
            GlobalVariables.MainWindow = this;

            InitializeComponent();

            timerTokenSource = new CancellationTokenSource();

            StartTimer();
        }

        private void StartTimer()
        {
            try
            {
                Task.Run(() =>
                {
                    isTimerRunning = true;
                    while (!timerTokenSource.IsCancellationRequested)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                tbTimer.Text = DateTime.Now.ToString("HH:mm:ss");

                                Task.Delay(1000, timerTokenSource.Token);
                            }
                            catch (SystemException e)
                            {
                                Console.Out.WriteLine(
                                    $"TIMER inner thread is CANCELLED: {timerTokenSource.IsCancellationRequested} at: {DateTime.Now}: {e.Message}");
                            }
                        });
                    }
                }, timerTokenSource.Token);
            }catch (SystemException e)
            {
                Console.Out.WriteLine(
                    $"TIMER out thread is CANCELLED: {timerTokenSource.IsCancellationRequested} at: {DateTime.Now}: {e.Message}");
            }
        }

        private void BtTimerControl_OnClick(object sender, RoutedEventArgs e)
        {
            if (isTimerRunning == true)
            {
                timerTokenSource.Cancel(true);
                isTimerRunning = false;
                btTimerControl.Background = Brushes.Gray;
            }
            else
            {
                timerTokenSource = new CancellationTokenSource();
                isTimerRunning = true;
                btTimerControl.Background = Brushes.Transparent;
                StartTimer();
            }

        }
    }
}