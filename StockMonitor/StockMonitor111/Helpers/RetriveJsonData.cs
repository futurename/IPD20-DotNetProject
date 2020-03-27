using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StockMonitor.Models.JSONModels;

namespace StockMonitor.Helpers
{
    public static class RetriveJsonData
    {
        private const string FmgBaseUrl = "https://financialmodelingprep.com/api/v3";
        private const string FmgDataDailyUrl = "/historical-price-full/";

        public static FMGDataDaily RetriveFmgDataDaily(string companySymbol)
        {
            string url = FmgBaseUrl + FmgDataDailyUrl + companySymbol;
            FMGDataDaily result = RequestFmgDataDaily(url);
            Console.Out.WriteLine(result.ToString());
            return result;
        }


        private static FMGDataDaily RequestFmgDataDaily(string url)
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

        private static FMGDataDaily ParseStringToFmgDataDaily(string response)
        {
            return JsonConvert.DeserializeObject<FMGDataDaily>(response);
        }

        
    }
}
