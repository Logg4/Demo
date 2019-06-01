using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using System.IO;

using StockWatch.Model;
using LogWriterLib;

namespace StockWatch.Logic
{
    public class Updater
    {
        /// <summary>
        /// Update stocks financials and quotes
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        public bool Update(Stock stock)
        {
            try
            {
                UpdateFinancialData(stock);

                AlphaVantageLib.Reader alphaVanReader = new AlphaVantageLib.Reader();

                if(String.IsNullOrEmpty(stock.TickerA))
                {
                    stock.TickerA = stock.TickerM + ".f";
                }
                List<AlphaVantageLib.QuoteAlphaV> quotes = alphaVanReader.GetWeeklyQuotes(stock.TickerA);

                // TODO: implement a faster (with less overhead, and minimum deleting) method
                if (quotes != null && 0 < quotes.Count)
                {
                    if (stock.WeeklyQuotes == null)
                    {
                        stock.WeeklyQuotes = new List<Quote>();
                    }
                    else
                    {
                        stock.WeeklyQuotes.Clear();
                    }
                    foreach (AlphaVantageLib.QuoteAlphaV newQuote in quotes)
                    {
                        Quote addMe = new Quote(newQuote);
                        stock.WeeklyQuotes.Add(addMe);
                    }
                }

                // update share price in financials
                foreach (Financials finance in stock.Financials)
                {
                    Logic.QuoteSearcher quoteSearch = new Logic.QuoteSearcher();
                    Quote quote = quoteSearch.FindNearestQuote(stock.WeeklyQuotes, finance.Date);
                    finance.SharePrice = (decimal)quote.Close;
                }

                stock.LastUpdate = DateTime.Now;
                DataAccess.Db.UpdateStock(stock);
                return true;
            }
            catch (Exception x)
            {
                Log.Error(x);
                return false;
            }
        }

        /// <summary>
        /// Update all financial data (currently using morningstar)
        /// </summary>
        /// <param name="stock"></param>
        private void UpdateFinancialData(Stock stock)
        {
            try
            {
                // only update financials if at least a month has passed
                if (stock.LastUpdate.Year < DateTime.Now.Year ||
                    (stock.LastUpdate.Year == DateTime.Now.Year && stock.LastUpdate.Month < DateTime.Now.Month) ||
                    stock.Financials == null ||
                    stock.Financials.Count == 0 ||
                    stock.FinancialsRawCSV == null ||
                    stock.KeyRatiosRawCSV == null || true)
                {
                    MorningstarLib.Reader morningReader = new MorningstarLib.Reader();
                    if (!morningReader.GetFinancialData(stock.TickerM, out string keyRatios, out string financialStatement))
                    {
                        throw new Exception("Acquiring financial data from morningstar-webpage failed");
                    }

                    if (stock.FinancialsRawCSV == null)
                    {
                        stock.FinancialsRawCSV = new List<string>();
                    }
                    if (stock.KeyRatiosRawCSV == null)
                    {
                        stock.KeyRatiosRawCSV = new List<string>();
                    }
                    stock.FinancialsRawCSV.Add(financialStatement);
                    stock.KeyRatiosRawCSV.Add(keyRatios);

                    // transform raw csv to internal financials representation
                    MorningstarCsvToStock csvToStock = new MorningstarCsvToStock(keyRatios, financialStatement);
                    List<Financials> financialData = csvToStock.FinancialData;
                    if (String.IsNullOrWhiteSpace(stock.Company))
                    {
                        stock.Company = csvToStock.CompanyName;
                    }

                    // make sure stocks financials isn't null
                    if (stock.Financials == null)
                    {
                        stock.Financials = new List<Financials>();
                    }

                    // iterate over financial data and upadate or add appropriate db-entries
                    foreach (Financials financePerYear in financialData)
                    {
                        // TTM is always overwritten
                        if (financePerYear.IsTTM)
                        {
                            Financials ttmInDb = stock.Financials.Find(x => x.IsTTM);
                            if (ttmInDb != null)
                            {
                                // do not overwrite dividend, may have been altered by user
                                decimal dividend = ttmInDb.Dividends; 
                                // in db => overwrite
                                ttmInDb = financePerYear;
                                ttmInDb.Dividends = dividend;
                            }
                            else
                            {
                                 // not in db => add
                                stock.Financials.Add(financePerYear);
                            }
                        }
                        else
                        {
                            if (!stock.Financials.Exists(x => x.Date == financePerYear.Date))
                            {
                                // financial data for selected date isn't in the databse, so we store it
                                stock.Financials.Add(financePerYear);
                            }
                        }
                    }
                }
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
        }
    }
}
