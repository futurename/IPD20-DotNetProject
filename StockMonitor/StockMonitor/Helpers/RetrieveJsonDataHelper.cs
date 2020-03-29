using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StockMonitor.Models.ApiModels;
using StockMonitor.Models.JSONModels;

namespace StockMonitor.Helpers
{
    public static class RetrieveJsonDataHelper
    {
        private const string FmgBaseUrl = "https://financialmodelingprep.com/api/v3";
        private const string FmgDataDailyUrl = "/historical-price-full/";
        private const string FmgCompanyProfileUrl = "/company/profile/";
        private const string FmgMajorIndexesUrl = "/majors-indexes/";
        private const string FmgQuoteOnlyPriceUrl = "/stock/real-time-price/";
        private const string Fmg1MinQuoteUrl = "/historical-chart/1min/";



        private const string FinnToken = "&token=bpvodjvrh5rddd65bseg";
        private const string FinnBaseUrl = "https://finnhub.io/api/v1";
        private const string FinnCompanyProfileUrl = "/stock/profile/?symbol=";
        private const string FinnQuoteUrl = "/quote?symbol=";
        


        public static List<FmgMajorIndex> RetrieveFmgMajorIndexes()
        {
            string url = FmgBaseUrl + FmgMajorIndexesUrl;
            string response = RetriveFromUrl(url).Result;
            List<FmgMajorIndex> result = ParseStringToFmgMajorIndexList(response);
            return result;
        }

        public static FmgQuoteOnlyPrice RetrieveFmgQuoteOnlyPrice(string symbol)
        {
            string url = FmgBaseUrl + FmgQuoteOnlyPriceUrl + symbol;
            string response = RetriveFromUrl(url).Result;
            FmgQuoteOnlyPrice result = ParseStringToFmgQuoteOnlyPrice(response);
            return result;
        }

        private static FmgQuoteOnlyPrice ParseStringToFmgQuoteOnlyPrice(string response)
        {
            return JsonConvert.DeserializeObject<FmgQuoteOnlyPrice>(response);
        }

        public static List<FmgQuoteOnlyPrice> RetrievAllFmgQuoteOnlyPrice()
        {
            string url = FmgBaseUrl + FmgQuoteOnlyPriceUrl;
            string response = RetriveFromUrl(url).Result;
            List<FmgQuoteOnlyPrice> result = ParseStringToAllFmgQuoteOnlyPrice(response);
            return result;
        }

        private static List<FmgQuoteOnlyPrice> ParseStringToAllFmgQuoteOnlyPrice(string response)
        {
            List<FmgQuoteOnlyPrice> fmgQuoteOnlyPriceList = JsonConvert.DeserializeObject<JObject>(response)
                .Value<JArray>("stockList").ToObject<List<FmgQuoteOnlyPrice>>();
            return fmgQuoteOnlyPriceList;
        }

        private static List<FmgMajorIndex> ParseStringToFmgMajorIndexList(string response)
        {
            List<FmgMajorIndex> majorIndexList = JObject.Parse(response).GetValue("majorIndexesList").ToObject<List<FmgMajorIndex>>();
            
            return majorIndexList;
        }

        public static FinnQuote RetrieveFinnQuote(string symbol)
        {
            string url = FinnBaseUrl + FinnQuoteUrl + symbol + FinnToken;

            string response = RetriveFromUrl(url).Result;
            FinnQuote result = ParseStringToFinnQuote(response);
            return result;
        }

        private static FinnQuote ParseStringToFinnQuote(string response)
        {
            return JsonConvert.DeserializeObject<FinnQuote>(response);
        }


        public static FmgCompanyProfile RetrieveFmgCompanyProfile(string companySymbol)
        {
            string url = FmgBaseUrl + FmgCompanyProfileUrl + companySymbol;
            string response = RetriveFromUrl(url).Result;
            FmgCompanyProfile result = ParseStringToFmgCompanyProfile(response);
            return result;
        }


        private static FmgCompanyProfile ParseStringToFmgCompanyProfile(string response)
        {
            var profile = JObject.Parse(response).GetValue("profile").ToObject<FmgCompanyProfile>();
            return profile;
        }

        public static FinnCompanyProfile RetrieveFinnCompanyProfile(string companySymbol)
        {
            string url = FinnBaseUrl + FinnCompanyProfileUrl + companySymbol + FinnToken;
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
            var jsonSet = JsonConvert.DeserializeObject<JObject>(response);
            var dataDailyList = jsonSet.Value<JArray>("historical").ToObject<List<FmgCandleDaily>>();
           
            return dataDailyList;
        }

        public static List<Fmg1MinQuote> RetrieveAllFmg1MinQuote(string symbol)
        {
            string url = FmgBaseUrl + Fmg1MinQuoteUrl + symbol;
            string response = RetriveFromUrl(url).Result;
            List<Fmg1MinQuote> fmg1MinQuoteList = ParseStringToFmg1MinQuoteList(response);
            return fmg1MinQuoteList;
        }

        private static List<Fmg1MinQuote> ParseStringToFmg1MinQuoteList(string response)
        {
            return JsonConvert.DeserializeObject<List<Fmg1MinQuote>>(response);
        }
    }
}
