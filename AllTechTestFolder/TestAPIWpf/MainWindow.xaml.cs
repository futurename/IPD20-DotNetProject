using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Net.Http.Headers;
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
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ThreeFourteen.Finnhub.Client;

namespace TestAPIWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string filePath;

        public MainWindow()
        {
            InitializeComponent();
            //ApiHelper.InitializeClient();

            //tbResult.Text = GetPrice();


            //GetPrice();
            
           // tbResult.Text = GetPriceFromFinnhub().Result;
            GetPriceFromFinnhub();


        }

        public static async void GetPriceFromFinnhub()
        {
            var client = new FinnhubClient("bpuhd3nrh5rbbhoij1og");
            //var rawData = await client.GetRawDataAsync("/quote", new Field(FieldKeys.Symbol, "AAPL"));
            //string result = JValue.Parse(rawData.ToString()).ToString(Formatting.Indented);
            //MessageBox.Show(result);
            string result = await new HttpClient().GetStringAsync("http://news.sina.com.cn");
            MessageBox.Show(result);
            // return result;
        }


        public async void GetPrice()
        {
            //Define your baseUrl
            //string baseUrl = "https://financialmodelingprep.com/api/v3/stock/real-time-price/AAPL";
           // string baseUrl =
            //    " https://cloud.iexapis.com/stable/stock/AAPL/financials/2?token=pk_777ee770a8f0411a88daa91ccd90b52c ";

            string baseUrl = "https://finnhub.io/docs/api/quote?symbol=AAPL?token=bpuhd3nrh5rbbhoij1og";
            //Have your using statements within a try/catch block
            try
            {

                //We will now define your HttpClient with your first using statement which will use a IDisposable.
                using (HttpClient client = new HttpClient())
                {
                    //In the next using statement you will initiate the Get Request, use the await keyword so it will execute the using statement in order.
                    using (HttpResponseMessage res = await client.GetAsync(baseUrl))
                    {
                        //Then get the content from the response in the next using statement, then within it you will get the data, and convert it to a c# object.
                        using (HttpContent content = res.Content)
                        {
                            //Now assign your content to your data variable, by converting into a string using the await keyword.
                            var data = await content.ReadAsStringAsync();
                            //If the data isn't null return log convert the data using newtonsoft JObject Parse class method on the data.
                            if (data != null)
                            {
                                //Now log your data in the console
                                // MessageBox.Show(data);
                                //var quote = JsonConvert.DeserializeObject<Quote>(data);
                                //MessageBox.Show(quote.ToString());

                                SaveFileDialog saveFileDialog = new SaveFileDialog();
                                saveFileDialog.Filter = "Txt | *.txt";
                                if (saveFileDialog.ShowDialog() == true)
                                {
                                    filePath = saveFileDialog.FileName;
                                    File.WriteAllText(filePath, data);

                                }

                            }
                            else
                            {
                                Console.WriteLine("NO Data----------");

                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception Hit------------");
                Console.WriteLine(exception);
            }


        }


        private void BtLoad_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Txt |*.txt";
            if (fileDialog.ShowDialog() == true)
            {
                string result = File.ReadAllText(fileDialog.FileName);
                string formattedString = JValue.Parse(result).ToString(Formatting.Indented);
                tbResult.Text = formattedString;
            }
        }
    }


    public class Quote
    {
        public string Symbol { get; set; }
        public double Price { get; set; }

        public override string ToString()
        {
            return $"{Symbol}, {Price}";
        }
    }
}
