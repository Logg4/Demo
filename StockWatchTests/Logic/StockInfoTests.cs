using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockWatch.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockWatch.Model;

namespace StockWatch.Logic.Tests
{
    [TestClass()]
    public class StockInfoTests
    {
        [TestMethod()]
        public void GetCashflowGrowthRateInPercentTest_1()
        {
            Stock stock = new Stock();
            Financials item1 = new Financials
            {
                FreeCashFlowPerShare = 10
            };
            Financials item2 = new Financials
            {
                FreeCashFlowPerShare = 12
            };
            stock.Financials = new List<Financials>();
            stock.Financials.Add(item1);
            stock.Financials.Add(item2);

            StockInfo stckInf = new StockInfo(stock);
            Assert.AreEqual(20, stckInf.GetCashflowGrowthRateInPercent());
        }

        [TestMethod()]
        public void GetCashflowGrowthRateInPercentTest_2()
        {
            Stock stock = new Stock();
            Financials item1 = new Financials
            {
                FreeCashFlowPerShare = 10
            };
            Financials item2 = new Financials
            {
                FreeCashFlowPerShare = -5
            };
            Financials item3 = new Financials
            {
                FreeCashFlowPerShare = -15
            };
            Financials item4 = new Financials
            {
                FreeCashFlowPerShare = 4
            };
            stock.Financials = new List<Financials>();
            stock.Financials.Add(item1);
            stock.Financials.Add(item2);
            stock.Financials.Add(item3);
            stock.Financials.Add(item4);

            // -150 -> -200 -> 475 = 275
            StockInfo stckInf = new StockInfo(stock);
            float actual = stckInf.CashflowGrowthRateInPercent;
            Assert.AreEqual(41.67, Math.Round(actual, 2));
        }

        [TestMethod()]
        public void GetCashflowGrowthRateInPercentTest_3()
        {
            Stock stock = new Stock();
            Financials item1 = new Financials
            {
                FreeCashFlowPerShare = -5
            };
            Financials item2 = new Financials
            {
                FreeCashFlowPerShare = 1
            };
            Financials item3 = new Financials
            {
                FreeCashFlowPerShare = 15
            };
            Financials item4 = new Financials
            {
                FreeCashFlowPerShare = 23
            };
            stock.Financials = new List<Financials>();
            stock.Financials.Add(item1);
            stock.Financials.Add(item2);
            stock.Financials.Add(item3);
            stock.Financials.Add(item4);

            // 600 -> 1400 -> 53.33 
            StockInfo stckInf = new StockInfo(stock);
            float actual = stckInf.CashflowGrowthRateInPercent;
            Assert.AreEqual(684.44, Math.Round(actual, 2));
        }
    }
}