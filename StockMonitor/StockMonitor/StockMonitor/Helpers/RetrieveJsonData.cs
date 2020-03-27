using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StockMonitor.Models.JSONModels;

namespace StockMonitor.Helpers
{
    public static class RetrieveJsonData
    {
        private const string FmgBaseUrl = "https://financialmodelingprep.com/api/v3";
        private const string FmgDataDailyUrl = "/historical-price-full/";

        private const string FinnToken = "token=bpuhd3nrh5rbbhoij1og";
        private const string FinnBaseUrl = "https://finnhub.io/api/v1";
        private const string FinnCompanyProfileUrl = "/stock/profile/?symbol=";

        public static FinnCompanyProfile RetriveFinnCompanyProfile(string companySymbol)
        {
            string url = FinnBaseUrl + FinnCompanyProfileUrl + companySymbol + "&" + FinnToken;
            string response = RetriveFromUrl(url).Result;
            FinnCompanyProfile result = ParseStringToFinnCompanyProfile(response);
            return result;
        }

        private static FinnCompanyProfile ParseStringToFinnCompanyProfile(string response)
        {
            return JsonConvert.DeserializeObject<FinnCompanyProfile>(response);
        }


        public static List<FmgCandleDaily> RetriveFmgDataDaily(string companySymbol)
        {
            string url = FmgBaseUrl + FmgDataDailyUrl + companySymbol;
            List<FmgCandleDaily> result = RequestFmgDataDaily(url);
           // Console.Out.WriteLine(result.ToString());
            return result;
        }


        private static List<FmgCandleDaily> RequestFmgDataDaily(string url)
        {
            string response = RetriveFromUrl(url).Result;
            return ParseStringToFmgDataDaily(response);
        }

        private static async Task<string> RetriveFromUrl(string url)
        {
            HttpClient client = new HttpClient();
            using (HttpResponseMessage response = await client.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
        }

        private static List<FmgCandleDaily> ParseStringToFmgDataDaily(string response)
        {
            //string filepath =
           //     "C:\\Users\\WW\\Desktop\\SourceTree\\IPD20_DotNet_WW\\IPD20-DotNetProject\\StockMonitor\\StockMonitor\\StockMonitor\\FmgDataDaily.txt";
            //File.WriteAllText(filepath, response);
            var jsonSet = JsonConvert.DeserializeObject<JObject>(response);
            var dataDailyList = jsonSet.Value<JArray>("historical").ToObject<List<FmgCandleDaily>>();
            //Console.Out.WriteLine(h.Select(p=>p.ToString()));
            return dataDailyList;
        }

        
    }
}
