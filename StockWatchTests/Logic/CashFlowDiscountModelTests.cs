using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockWatch.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWatch.Logic.Tests
{
    [TestClass()]
    public class CashFlowDiscountModelTests
    {
        [TestMethod()]
        public void CalculateTest_1()
        {
            List<decimal> cashFlows = new List<decimal>() { 10, 11, 12, 14, 16 };
            CashFlowDiscountModel discModel = new CashFlowDiscountModel();
            decimal result = discModel.Calculate(cashFlows, 10, 5);

            // 10/(1+0.1)^1 + 11/(1+0.1)^2.... = 46.69...
            // terminal value = 16 * 1.05 / (1.1 - 1.05) = 336
            Assert.AreEqual(382.69m, Decimal.Round(result, 2));

            
        }
    }
}