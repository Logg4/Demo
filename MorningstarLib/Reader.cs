using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;

using LogWriterLib;

namespace MorningstarLib
{
    public class Reader
    {

        /// <summary>
        /// Scrape the Morningstar webpage for 2 csv files:
        /// 1) KeyRatios
        /// 2) Financials
        /// Marketplace is XETR
        /// </summary>
        /// <param name="keyRatiosCSV"></param>
        /// <param name="financialsCSV"></param>
        /// <returns></returns>
        public bool GetFinancialData(string ticker, out string keyRatiosCSV, out string financialsCSV)
        {
            keyRatiosCSV = "";
            financialsCSV = "";
            try
            {
                // KeyRatios
                string url = $"http://financials.morningstar.com/finan/ajax/exportKR2CSV.html?&callback=?&t=XETR:{ticker}&region=deu&culture=en-US";
                // there seems to be some kind of download prevention if referer is not set
                string referer = $"http://financials.morningstar.com/ratios/r.html?t=XETR:{ticker}&region=deu&culture=en-US";
                keyRatiosCSV = DownloadContent(url, referer);

                // Financials in three parts
                url = $" http://financials.morningstar.com/ajax/ReportProcess4CSV.html?t=XETR:{ticker}&reportType=is&period=12&dataType=A&order=asc&columnYear=10&number=3";
                string incomeStatement = DownloadContent(url, referer);

                url = $" http://financials.morningstar.com/ajax/ReportProcess4CSV.html?t=XETR:{ticker}&reportType=bs&period=12&dataType=A&order=asc&columnYear=10&number=3";
                string balanceSheet = DownloadContent(url, referer);

                url = $" http://financials.morningstar.com/ajax/ReportProcess4CSV.html?t=XETR:{ticker}&reportType=cf&period=12&dataType=A&order=asc&columnYear=10&number=3";
                string cashflow = DownloadContent(url, referer);

                financialsCSV += incomeStatement;
                financialsCSV += Environment.NewLine + "----- END" + Environment.NewLine;
                financialsCSV += balanceSheet;
                financialsCSV += Environment.NewLine + "----- END" + Environment.NewLine;
                financialsCSV += cashflow;
                financialsCSV += Environment.NewLine + "----- END" + Environment.NewLine;

                return true;
            }
            catch (Exception x)
            {
                Log.Error(x);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string DownloadContent(string url, string referrer = "")
        {
            try
            {
                string result = "";
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.BaseAddress = new Uri(url);
                    if (!String.IsNullOrWhiteSpace(referrer))
                    {
                        client.DefaultRequestHeaders.Referrer = new Uri(referrer);
                    }

                    HttpResponseMessage response = client.GetAsync(url).Result;

                    if (response.IsSuccessStatusCode && 0 < response.Content.Headers.ContentLength)
                    {
                        result = response.Content.ReadAsStringAsync().Result;
                    }

                    return result;
                }
            }
            catch (Exception x)
            {
                Log.Error(x);
                return "";
            }
        }
    }
}
