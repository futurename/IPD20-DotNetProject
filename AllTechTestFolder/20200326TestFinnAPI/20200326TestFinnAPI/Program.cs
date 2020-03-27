using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ThreeFourteen.Finnhub.Client;

namespace _20200326TestFinnAPI
{
    class Program
    {

        static void Main(string[] args)
        {

           // Console.Out.WriteLine(JValue.Parse(GetInfo().Result).ToString(Formatting.Indented));

           Console.Out.WriteLine(JValue.Parse(GetWithHttpClient().Result).ToString(Formatting.Indented));
            Console.ReadKey();
        }

        public static async Task<string> GetInfo()
        {
            var client = new FinnhubClient("bpuhd3nrh5rbbhoij1og");
            var rawData = await client.GetRawDataAsync("stock/profile", new Field(FieldKeys.Symbol, "AAPL"));
            //Console.Out.WriteLine("Result: \n"+ rawData.ToString() );
            return rawData;

        }

        public static async Task<string> GetWithHttpClient()
        {
            const string baseUrlFinn = "https://finnhub.io/api/v1/stock/metric?symbol=AAPL&metric=margin&token=bpuhd3nrh5rbbhoij1og";
            const string baseUrlFMP = "https://financialmodelingprep.com/api/v3/historical-price-full/AAPL";
            const string filepath =
                "C:\\Users\\WW\\Desktop\\SourceTree\\IPD20_DotNet_WW\\IPD20-DotNetProject\\AllTechTestFolder\\20200326TestFinnAPI\\FMP.txt";
            HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage response = await
                    client.GetAsync(baseUrlFMP);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                File.WriteAllText(filepath, responseBody);

                return responseBody;
            }
            catch (Exception ex)
            {
                throw new SystemException(ex.Message);
            }

        }
    }
}
