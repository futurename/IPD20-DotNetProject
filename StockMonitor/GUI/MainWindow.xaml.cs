﻿using System;
using System.Collections.Concurrent;
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
using MaterialDesignThemes.Wpf;
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

            StartTimer();
            
        }

        private async void StartTimer()
        {
            await Task.Run(() =>
            {
                try
                {
                    timerTokenSource = new CancellationTokenSource();
                    Timer(timerTokenSource.Token);
                }
                catch (SystemException e)
                {
                    Console.Out.WriteLine(
                        $"TIMER out thread is CANCELLED: {timerTokenSource.IsCancellationRequested} at: {DateTime.Now}: {e.Message}");
                }
            });
        }

        private Task Timer(CancellationToken ct)
        {
            isTimerRunning = true;
            while (true)
            {
                ct.ThrowIfCancellationRequested();

                Dispatcher.Invoke(() =>
                {
                    tbTimer.Text = DateTime.Now.ToString("HH:mm:ss");
                });
                Task.Delay(1000, ct);
            }
        }

        private  void BtTimerControl_OnClick(object sender, RoutedEventArgs e)
        {
            if (isTimerRunning == true)
            {
                timerTokenSource.Cancel(true);
                isTimerRunning = false;
                btTimerControl.Background = Brushes.Gray;
            }
            else
            {
                isTimerRunning = true;
                btTimerControl.Background = Brushes.Transparent;
                StartTimer();
            }

        }



        private void ChangeToLightTheme(object sender, RoutedEventArgs e)
        {
            ModifyTheme(theme => theme.SetBaseTheme(Theme.Light));
            GlobalVariables.SearchStockUserControl.IsThemeDark = false;
        }

        private void ChangeToDarkTheme(object sender, RoutedEventArgs e)
        {
            ModifyTheme(theme => theme.SetBaseTheme(Theme.Dark));
            GlobalVariables.SearchStockUserControl.IsThemeDark = true;
        }


        private static void ModifyTheme(Action<ITheme> modificationAction)
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();

            modificationAction?.Invoke(theme);

            paletteHelper.SetTheme(theme);
        }
    }
}