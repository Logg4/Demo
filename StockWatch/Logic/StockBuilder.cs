using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockWatch.Model;
using LogWriterLib;

namespace StockWatch.Logic
{
    public class StockBuilder
    {


        public Stock BuildStock(Stock stock)
        {
            try
            {
                List<Stock> currentStock = DataAccess.Db.GetCurrentStockList();
                Stock stockInList = currentStock.Find(x => x.TickerM.Equals(stock.TickerM, StringComparison.OrdinalIgnoreCase));
                // do nothing if stock already in list
                if (stockInList != null)
                {
                    return stockInList;
                }

                if (!DataAccess.Db.AddStock(stock))
                {
                    return null;
                }
                Updater updater = new Updater();
                if (updater.Update(stock))
                {
                    return stock;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception x)
            {
                Log.Error(x);
                return null;
            }
        }


        /// <summary>
        /// Build stock based on ticker.
        /// If all goes well stock is added to database
        /// </summary>
        /// <param name="ticker"></param>
        /// <returns></returns>
        public Stock BuildStockFromTicker(string ticker)
        {
            try
            {
                List<Stock> currentStock = DataAccess.Db.GetCurrentStockList();
                Stock stockInList = currentStock.Find(x => x.TickerM.Equals(ticker, StringComparison.OrdinalIgnoreCase));
                // do nothing if stock already in list
                if (stockInList != null)
                {
                    return stockInList;
                }

                Stock result = new Stock
                {
                    TickerM = ticker
                };

                if (!DataAccess.Db.AddStock(result))
                {
                    return null;
                }
                Updater updater = new Updater();
                if (updater.Update(result))
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception x)
            {
                Log.Error(x);
                return null;
            }
        }

        
    }
}
