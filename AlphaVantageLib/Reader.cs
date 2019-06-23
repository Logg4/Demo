using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

using LogWriterLib;


namespace AlphaVantageLib
{
    /// <summary>
    /// Read quotes from Alphavantage-API
    /// </summary>
    public class Reader
    {
        private string _apiPath = "https://www.alphavantage.co/query";
        private List<string> _keys = new List<string> {"ZVJLJBBS6M72DXZ0", "MQ1VV3JRVXXQ76IC", "XRIBKOGUMM9CKTV4",
            "9CQII9AB8IUUWD66", "VVCT7X98MD3BCLK1", "Q94IIZTSPOWSGB9X", "27PU7RDZQ05L8Q69", "4OVZB0U8N2W3566E", "5IN343A9CJCDG5H5",
            "OZAF6XAV6MZSENUP", "B7D8W38F4JMDPS6G", "FB2CSECJCRTCAP4Z", "XS4ZCJFCLDD18FYC"};

        public List<QuoteAlphaV> GetDailyQuotes(string symbol)
        {
            List<QuoteAlphaV> result = new List<QuoteAlphaV>();
            try
            {
                bool expiredKey = false;
                do
                {
                    if (expiredKey)
                    {
                        Log.Info("waiting 30 sec to resume");
                        System.Threading.Thread.Sleep(30000);
                    }
                    string functionPath = _apiPath + $"?function=TIME_SERIES_DAILY_ADJUSTED&symbol={symbol}&outputsize=full&apikey={GetKey()}";

                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                        client.BaseAddress = new Uri(functionPath);
                        HttpResponseMessage response = client.GetAsync(functionPath).Result;
                        JObject subObj = GetTimeSeriesObject(response, out expiredKey, out bool error);
                        if (subObj != null)
                        {
                            result = GetQuotes(subObj);
                        }
                        if (error == true)
                        {
                            Log.Error($"Call [{functionPath}] failed, wrong symbol?");
                        }
                    }
                } while (expiredKey == true);
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
            return result;
        }

        public List<QuoteAlphaV> GetWeeklyQuotes(string symbol)
        {
            List<QuoteAlphaV> result = new List<QuoteAlphaV>();
            try
            {
                bool expiredKey = false;
                do
                {
                    if (expiredKey)
                    {
                        Log.Info("waiting 30 sec to resume");
                        System.Threading.Thread.Sleep(30000);
                    }
                    string functionPath = _apiPath + $"?function=TIME_SERIES_WEEKLY_ADJUSTED&symbol={symbol}&outputsize=full&apikey={GetKey()}";
                    Console.WriteLine("Calling: " + functionPath);
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                        client.BaseAddress = new Uri(functionPath);
                        HttpResponseMessage response = client.GetAsync(functionPath).Result;
                        JObject subObj = GetTimeSeriesObject(response, out expiredKey, out bool error);
                        if (subObj != null)
                        {
                            result = GetQuotes(subObj);
                        }
                        if (error == true)
                        {
                            Log.Error($"Call [{functionPath}] failed, wrong symbol?");
                        }
                    }
                } while (expiredKey == true);
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
            return result;
        }

        private JObject GetTimeSeriesObject(HttpResponseMessage message, out bool expiredKey, out bool error)
        {
            expiredKey = false;
            error = false;
            JObject result = null;
            if (message.IsSuccessStatusCode)
            {

                string jsonContent = message.Content.ReadAsStringAsync().Result;
                JObject parsedObject = JObject.Parse(jsonContent);
                if (parsedObject.ContainsKey("Error Message"))
                {
                    error = true;
                    return null;
                }
                if (parsedObject.ContainsKey("Note"))
                {
                    expiredKey = true;
                    return null;
                }

                result = (JObject)parsedObject.Properties().ElementAt(1).Value;
            }
            return result;
        }


        private string GetKey()
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            int idx = rnd.Next(0, _keys.Count - 1);
            string key = _keys[idx];

            return key;
        }

        public List<QuoteAlphaV> GetQuotes(JObject timeSeriesObject)
        {
            List<QuoteAlphaV> result = new List<QuoteAlphaV>();

            foreach (JProperty prop in timeSeriesObject.Properties())
            {
                QuoteAlphaV myQuote = new QuoteAlphaV();
                if (DateTime.TryParse(prop.Name, out DateTime date))
                {
                    myQuote.Date = date;
                }

                JObject quote = (JObject)prop.Value;

                myQuote.Open = (float)quote["1. open"];
                myQuote.High = (float)quote["2. high"];
                myQuote.Low = (float)quote["3. low"];
                myQuote.Close = (float)quote["4. close"];
                myQuote.Volume = (long)quote["6. volume"];
                myQuote.Dividend = (float)quote["7. dividend amount"];
                if (quote.ContainsKey("8. split coefficient"))
                {
                    myQuote.SplitCoefficient = (float)quote["8. split coefficient"];
                }

                result.Add(myQuote);
            }
            return result;
        }
    }
}
