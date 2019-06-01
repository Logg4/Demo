using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlphaVantageLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaVantageLib.Tests
{
    [TestClass()]
    public class ReaderTests
    {
        [TestMethod()]
        public void GetWeeklyQuotesTest()
        {
            Reader reader = new Reader();
            List<QuoteAlphaV> actual = reader.GetWeeklyQuotes("BAYN");
            Assert.AreNotEqual(0, actual.Count);
        }

        [TestMethod()]
        public void GetDailyQuotesTest()
        {
            Reader reader = new Reader();
            List<QuoteAlphaV> actual = reader.GetDailyQuotes("ALV");
            Assert.AreNotEqual(0, actual.Count);
        }
    }
}