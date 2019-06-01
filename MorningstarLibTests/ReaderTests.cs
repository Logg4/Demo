using Microsoft.VisualStudio.TestTools.UnitTesting;
using MorningstarLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorningstarLib.Tests
{
    [TestClass()]
    public class ReaderTests
    {
        [TestMethod()]
        public void GetFinancialDataTest()
        {
            Reader reader = new Reader();
            bool actual = reader.GetFinancialData("BAYN", out string keyRatios, out string financaials);

            Assert.AreEqual(true, actual);
            Assert.AreNotEqual("", keyRatios);
            Assert.AreNotEqual("", financaials);
        }
    }
}