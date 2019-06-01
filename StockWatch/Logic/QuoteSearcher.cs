using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockWatch.Model;
using LogWriterLib;

namespace StockWatch.Logic
{
    public class QuoteSearcher
    {
        /// <summary>
        /// Traverse quote list and get the next matching quote based on date
        /// TODO: is not really the nearest just the previous neighbor, so think of something better
        /// </summary>
        /// <param name="quotes"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public Quote FindNearestQuote(List<Quote> quotes, DateTime date)
        {
            try
            {
                if (quotes == null)
                {
                    return null;
                }
                Quote result = null;
                if (date.Ticks == 0)
                {
                    // Date is not for TTm, so set it to now
                    date = DateTime.Now;
                }

                foreach (Quote quote in quotes)
                {
                    if (result == null || (date <= quote.Date && quote.Close != 0))
                    {
                        result = quote;
                    }
                    else if(quote.Close != 0)
                    {
                        break;
                    }

                }
                return result;
            }
            catch (Exception x)
            {
                Log.Error(x);
                return null;
            }
        }
    }
}
