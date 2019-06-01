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
    public class ResidualIncomeModelTests
    {
        [TestMethod()]
        public void CalculateTest()
        {
            ResidualIncomeModel test = new ResidualIncomeModel();
            Financials financials = new Financials();
            financials.BookValuePerShare = 26.77m;
            financials.EarningsPerShare = 4.67m;
            decimal actual = test.Calculate(financials, 13.5m);
            Assert.AreEqual(34.59m, Decimal.Round(actual, 2));
        }
    }
}