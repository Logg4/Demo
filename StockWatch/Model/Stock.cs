using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LiteDB;

namespace StockWatch.Model
{

    public class Stock
    {
        public int Id
        {
            get; set;
        }

        public string TickerM
        {
            get; set;
        }

        public string TickerA
        {
            get; set;
        }

        public string TradingVenue
        {
            get;set;
        }

        public string Company
        {
            get; set;
        }

        public string Isin
        {
            get; set;
        }

        public List<Quote> WeeklyQuotes
        {
            get; set;
        }

        public List<Financials> Financials
        {
            get;set;
        }

        public DateTime LastUpdate
        {
            get;set;
        }

        public bool IsOnWatchList
        {
            get;set;
        }

        public bool IsInPortfolio
        {
            get;set;
        }

        public List<string> FinancialsRawCSV
        {
            get;set;
        }

        public List<string> KeyRatiosRawCSV
        {
            get;set;
        }

        /// <summary>
        /// How we rate this stock 
        /// TODO: ?? [0-100] 0-40 = bad (sell), 40-70 = okay (hold), 70-100 = nice (buy)  
        /// </summary>
        [BsonIgnore]
        public uint Rating
        {
            get;set;
        }

    }
}
