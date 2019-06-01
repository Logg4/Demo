using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

using LiteDB;
using StockWatch.Model;
using LogWriterLib;


namespace StockWatch.Logic
{
    /// <summary>
    /// Singleton access
    /// </summary>
    public sealed class DataAccess
    {
        private static DataAccess _instance = null;
        private static readonly object _lock = new object();
        // TODO: configure at inital start?
        //private readonly string _dataBaseFile = @"E:\Programming\Dbs\Stocks.db";
        private readonly string _dataBaseFile;
        private List<Stock> _currentStockCollection = null;

        /// <summary>
        /// Make sure can't be build from outside
        /// </summary>
        private DataAccess()
        {
            _currentStockCollection = new List<Stock>();
            if (String.IsNullOrEmpty(DbFile))
            {
                string dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                _dataBaseFile = Path.Combine(dir, "Stocks.db");
            }
            else
            {
                _dataBaseFile = DbFile;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string DbFile
        {
            get;set;
        }

        /// <summary>
        /// Singleton access
        /// </summary>
        public static DataAccess Db
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DataAccess();
                        }
                    }
                }
                return _instance;
            }
        }
    
        /// <summary>
        /// Add stock and update references
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        public bool AddStock(Stock stock)
        {
            try
            {
                using (var db = new LiteDatabase(_dataBaseFile))
                {
                    // Get a collection (or create, if doesn't exist)
                    LiteCollection<Stock> currentCollection = db.GetCollection<Stock>("Stocks");
                    if (currentCollection == null)
                    {
                        throw new Exception("Update not possible data/collection does not exist");
                    }
                    if (!currentCollection.Upsert(stock))
                    {
                        throw new Exception($"Upsert for stock [{stock.Id}], [{stock.Company}] failed");
                    }
                    SetReferences(stock.Financials);
                }
                return true;
            }
            catch (Exception x)
            {
                Log.Error(x);
                return false;
            }
        }

        /// <summary>
        /// Add stocks and update references
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        public bool AddStocks(List<Stock> stocks)
        {
            try
            {
                using (var db = new LiteDatabase(_dataBaseFile))
                {
                    // Get a collection (or create, if doesn't exist)
                    LiteCollection<Stock> currentCollection = db.GetCollection<Stock>("Stocks");
                    if (currentCollection == null)
                    {
                        throw new Exception("Update not possible data/collection does not exist");
                    }
                    foreach (Stock stock in stocks)
                    {
                        if (!currentCollection.Upsert(stock))
                        {
                            Log.Error($"Upsert for stock [{stock?.Id ?? 0}], [{stock.Company}] failed");
                        }
                        SetReferences(stock.Financials);
                    }
                }
                return true;
            }
            catch (Exception x)
            {
                Log.Error(x);
                return false;
            }
        }

        /// <summary>
        /// Writes stock data to db and refreshes references
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        public bool UpdateStock(Stock stock)
        {
            try
            {
                using (var db = new LiteDatabase(_dataBaseFile))
                {
                    // Get a collection (or create, if doesn't exist)
                    LiteCollection<Stock> currentCollection = db.GetCollection<Stock>("Stocks");
                    if (currentCollection == null)
                    {
                        throw new Exception("Update not possible data/collection does not exist");
                    }
                    // not sure why, but in some cases upsert fails...
                    if (!currentCollection.Upsert(stock))
                    {
                        if (!currentCollection.Update(stock))
                        {
                            throw new Exception($"Update failed, stock [{stock.Id}], [{stock.Company}] not found");
                        }
                    }

                    SetReferences(stock.Financials);
                }

                return true;
            }
            catch (Exception x)
            {
                Log.Error(x);
                return false;
            }
        }

        /// <summary>
        /// Delete given stock, id must be set
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        public bool RemoveStock(Stock stock)
        {
            try
            {
                if (stock == null || stock.Id == 0)
                {
                    // do nothing if stock is not initialized
                    return true;
                }

                using (var db = new LiteDatabase(_dataBaseFile))
                {
                    // Get a collection (or create, if doesn't exist)
                    LiteCollection<Stock> currentCollection = db.GetCollection<Stock>("Stocks");
                    if (currentCollection == null)
                    {
                        throw new Exception("Delete not possible data/collection does not exist");
                    }
                    if (!currentCollection.Delete(stock.Id))
                    {
                        throw new Exception($"Delete failed, stock [{stock.Id}], [{stock?.Company ?? ""}] not found");
                    }
                }

                return true;
            }
            catch (Exception x)
            {
                Log.Error(x);
                return false;
            }
        }

        /// <summary>
        /// Read stock data from database and perform data-augmentations (sets references and share price)
        /// </summary>
        /// <returns></returns>
        public List<Stock> FullRead()
        {
            try
            {
                // get data from liteDb
                using (var db = new LiteDatabase(_dataBaseFile))
                {
                    // Get a collection (or create, if it doesn't exist)
                    LiteCollection<Stock> currentCollection = db.GetCollection<Stock>("Stocks");

                    // link financials (needed for some ratios)
                    foreach (Stock stock in currentCollection.FindAll().ToList())
                    {
                        if (stock.Financials != null)
                        {
                            bool updateDb = false;

                            SetReferences(stock.Financials);

                            // update share price in financials in case not set
                            foreach (Financials finance in stock.Financials)
                            {
                                if (finance.SharePrice == 0 && stock.WeeklyQuotes != null)
                                {
                                    Logic.QuoteSearcher quoteSearch = new Logic.QuoteSearcher();
                                    Quote quote = quoteSearch.FindNearestQuote(stock.WeeklyQuotes, finance.Date);
                                    finance.SharePrice = (decimal)(quote?.Close ?? 0);
                                    updateDb = true;
                                }
                            }

                            if (updateDb)
                            {
                                currentCollection.Update(stock);
                            }
                        }
                        _currentStockCollection.Add(stock);
                    }
                }
                return _currentStockCollection;
            }
            catch (Exception x)
            {
                Log.Error(x);
                return null;
            }
        }

        /// <summary>
        /// Retrieve stocks without checking back with the database (less overhead)
        /// </summary>
        /// <returns></returns>
        public List<Stock> GetCurrentStockList()
        {
            if (_currentStockCollection == null || _currentStockCollection.Count == 0)
            {
                return FullRead();
            }
            return _currentStockCollection;
        }


        /// <summary>
        /// Set PreviousFinancialData reference link
        /// </summary>
        /// <param name="financials"></param>
        private void SetReferences(List<Financials> financials)
        {
            if (financials == null || financials.Count < 1)
            {
                return;
            }
            for (int i = 1; i < financials.Count; i++)
            {
                financials[i].PreviousFinancialData = financials[i - 1];
            }
        }
    }
}
