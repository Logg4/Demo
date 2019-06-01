using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockWatch.ViewModel;
using LogWriterLib;

namespace StockWatch.Logic
{
    public class StocksRanking
    {
        /// <summary>
        /// Idea based on 'The little Book that beats the market'
        /// </summary>
        /// <param name="stocks"></param>
        public void SetRanks(List<StockDataVM> stocks)
        {
            try
            {
                List<StockDataVM> sortedByDivYield = stocks.OrderByDescending(x => x.DividendYield).ToList();
                List<StockDataVM> sortedByReturnOnCapital = stocks.OrderByDescending(x => x.ReturnOnCapital).ToList();
                List<StockDataVM> sortedByEarningsYield = stocks.OrderByDescending(x => x.EarningsYield).ToList();

                foreach (StockDataVM stock in stocks)
                {
                    int rankDivYield = sortedByDivYield.FindIndex(x => x.Ticker == stock.Ticker) + 1;
                    if (rankDivYield == -1)
                    {
                        stock.Rank = stocks.Count;
                        continue;
                    }
                    int rankROC = sortedByReturnOnCapital.FindIndex(x => x.Ticker == stock.Ticker) + 1;
                    if (rankROC == -1)
                    {
                        stock.Rank = stocks.Count;
                        continue;
                    }
                    int rankEarningsYield = sortedByEarningsYield.FindIndex(x => x.Ticker == stock.Ticker) + 1;
                    if (rankEarningsYield == -1)
                    {
                        stock.Rank = stocks.Count;
                        continue;
                    }

                    stock.Rank = (float)Math.Round((rankDivYield + rankROC + rankEarningsYield) / 3f, 1);
                    //stock.Rank = rankEarningsYield;
                }

                // 'normalize' ranks
                List<StockDataVM> sorted = stocks.OrderBy(x => x.Rank).ToList();
                for (int i = 0; i < sorted.Count; i++)
                {
                    sorted[i].Rank = i + 1;
                }

            }
            catch (Exception x)
            {
                Log.Error(x);
            }
        }

    }
}
