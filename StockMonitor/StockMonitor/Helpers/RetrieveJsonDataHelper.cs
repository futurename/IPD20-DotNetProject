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
        private const string FmgInvestmentValuationRatiosUrl = "/financial-ratios/";


        public static List<FmgMajorIndex> RetrieveFmgMajorIndexes()
        {
            string url = FmgBaseUrl + FmgMajorIndexesUrl;
            string response = RetrieveFromUrl(url).Result;
            List<FmgMajorIndex> result = ParseStringToFmgMajorIndexList(response);
            return result;
        }

        public static FmgQuoteOnlyPrice RetrieveFmgQuoteOnlyPrice(string symbol)
        {
            string url = FmgBaseUrl + FmgQuoteOnlyPriceUrl + symbol;
            string response = RetrieveFromUrl(url).Result;
            FmgQuoteOnlyPrice result = ParseStringToFmgQuoteOnlyPrice(response);
            return result;
        }

        private static FmgQuoteOnlyPrice ParseStringToFmgQuoteOnlyPrice(string response)
        {
            return JsonConvert.DeserializeObject<FmgQuoteOnlyPrice>(response);
        }

        public static List<FmgQuoteOnlyPrice> RetrieveAllFmgQuoteOnlyPrice()
        {
            string url = FmgBaseUrl + FmgQuoteOnlyPriceUrl;
            string response = RetrieveFromUrl(url).Result;
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


        public static FmgCompanyProfile RetrieveFmgCompanyProfile(string companySymbol)
        {
            string url = FmgBaseUrl + FmgCompanyProfileUrl + companySymbol;
            string response = RetrieveFromUrl(url).Result;
            FmgCompanyProfile result = ParseStringToFmgCompanyProfile(response);
            return result;
        }


        private static FmgCompanyProfile ParseStringToFmgCompanyProfile(string response)
        {
            var profile = JObject.Parse(response).GetValue("profile").ToObject<FmgCompanyProfile>();
            return profile;
        }


        public static List<FmgCandleDaily> RetrieveFmgDataDaily(string companySymbol)
        {
            string url = FmgBaseUrl + FmgDataDailyUrl + companySymbol;
            List<FmgCandleDaily> result = RequestFmgDataDaily(url);

            return result;
        }


        private static List<FmgCandleDaily> RequestFmgDataDaily(string url)
        {
            string response = RetrieveFromUrl(url).Result;
            return ParseStringToFmgDataDaily(response);
        }

        private static async Task<string> RetrieveFromUrl(string url)
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
            string response = RetrieveFromUrl(url).Result;
            List<Fmg1MinQuote> fmg1MinQuoteList = ParseStringToFmg1MinQuoteList(response);
            return fmg1MinQuoteList;
        }

        private static List<Fmg1MinQuote> ParseStringToFmg1MinQuoteList(string response)
        {
            return JsonConvert.DeserializeObject<List<Fmg1MinQuote>>(response);
        }

        public static FmgInvestmentValuationRatios RetrieveFmgInvestmentValuationRatios(string symbol)
        {
            string url = FmgBaseUrl + FmgInvestmentValuationRatiosUrl + symbol;
            string response = RetrieveFromUrl(url).Result;
            FmgInvestmentValuationRatios fmgInvRatioList = ParseStringToFmgInvestmentValuationRatios(response);
            return fmgInvRatioList;
        }

        private static FmgInvestmentValuationRatios ParseStringToFmgInvestmentValuationRatios(string response)
        {

            var jsonSet = JsonConvert.DeserializeObject<JObject>(response).Value<JArray>("ratios");
            var tmp = jsonSet[0].Value<JObject>("investmentValuationRatios");
            var result = tmp.ToObject<FmgInvestmentValuationRatios>();
            return result;
        }
    }
}
