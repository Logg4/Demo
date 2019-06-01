using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

using StockWatch.Model;

namespace StockWatch.Logic.Tests
{
    [TestClass()]
    public class DataAccessTests
    {
        [TestMethod()]
        public void FullReadTest_1()
        {
            List<Stock> actual = DataAccess.Db.FullRead();
            Assert.AreNotEqual(0, actual.Count);
        }


        /// <summary>
        /// Not a test, but an exceptional Task.
        /// Update stocks financial data based on last csv data, 
        /// usefull to augment internal datastructure with new elements without going for a customized (financials need to be dropped) full update
        /// </summary>
        [TestMethod()]
        public void UpdateFinancialData()
        {
            //DataAccess.DbFile = @"D:\Programming\Trading\Stocks\Applications\StockWatch\StockWatch\bin\Debug\Stocks.db";
            string dbFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            dbFile = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(dbFile)));
            dbFile = Path.Combine(dbFile, "StockWatch", "bin", "Debug", "Stocks.db");
            DataAccess.DbFile = dbFile;
            List<Stock> actual = DataAccess.Db.FullRead();
            Assert.AreNotEqual(0, actual.Count);

            foreach (Stock stock in actual)
            {
                if (stock.KeyRatiosRawCSV == null || stock.KeyRatiosRawCSV.Count == 0)
                {
                    continue;
                }

                MorningstarCsvToStock csvToFinance = new MorningstarCsvToStock(stock.KeyRatiosRawCSV.Last(), stock.FinancialsRawCSV.Last());
                List<Financials> newFinancials = csvToFinance.FinancialData;
                if (newFinancials != null && 0 < newFinancials.Count)
                {
                    // keep ttm dividends, may have been added by the user
                    newFinancials.Last().Dividends = stock.Financials.Last().Dividends;
                    stock.Financials = newFinancials;
                    bool result = DataAccess.Db.UpdateStock(stock);
                    Assert.AreEqual(true, result);
                }
            }
        }

        /// <summary>
        /// Not a test, but an exceptional Task.
        /// Clear stocks of financial data informations
        /// </summary>
        [TestMethod()]
        public void ClearFinancialData()
        {
            List<Stock> actual = DataAccess.Db.FullRead();
            Assert.AreNotEqual(0, actual.Count);

            try
            {
                foreach (Stock stock in actual)
                {
                    stock.Financials.Clear();
                    stock.LastUpdate = new DateTime();
                    bool result = DataAccess.Db.UpdateStock(stock);
                    Assert.AreEqual(true, result);
                }
            }
            catch (Exception x)
            {
                Assert.AreEqual(x, null, $"Exception [{x.Message}]");
            }
        }


    }
}