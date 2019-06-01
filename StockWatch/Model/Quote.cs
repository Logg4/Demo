using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LogWriterLib;

namespace StockWatch.Model
{
    public class Quote
    {

        public Quote()
        {
        }

        /// <summary>
        /// ctor based on quotes from alphavantage
        /// </summary>
        /// <param name="alphaQuote"></param>
        public Quote(AlphaVantageLib.QuoteAlphaV alphaQuote)
        {
            try
            {
                Open = alphaQuote.Open;
                Close = alphaQuote.Close;
                High = alphaQuote.High;
                Low = alphaQuote.Low;
                Volume = alphaQuote.Volume;
                Dividend = alphaQuote.Dividend;
                SplitCoefficient = alphaQuote.SplitCoefficient;
                Date = alphaQuote.Date;
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
        }

        public int Id
        {
            get;set;
        }

        public float Open
        {
            get; set;
        }

        public float Close
        {
            get; set;
        }

        public float High
        {
            get; set;
        }

        public float Low
        {
            get; set;
        }

        public long Volume
        {
            get; set;
        }

        public float Dividend
        {
            get; set;
        }

        public float SplitCoefficient
        {
            get; set;
        }

        public DateTime Date
        {
            get; set;
        }
    }
}
