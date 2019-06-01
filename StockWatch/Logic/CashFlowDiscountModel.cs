using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LogWriterLib;

namespace StockWatch.Logic
{
    /// <summary>
    /// Discounted Cash Flow is a method to determine the intrinsic value of a company
    /// </summary>
    public class CashFlowDiscountModel
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="expectedCashFlow"></param>
        /// <param name="discountRate"></param>
        /// <param name="expectedSellPrice"></param>
        /// <returns></returns>
        public decimal Calculate(List<decimal> expectedCashFlow, decimal discountRateInPercent, decimal terminalGrowthRateInPercent)
        {
            try
            {
                if (expectedCashFlow == null || expectedCashFlow.Count == 0)
                {
                    return -1;
                }

                double discountRate = (double)(discountRateInPercent / 100);

                decimal result = 0;
                for (int i = 0; i < expectedCashFlow.Count; i++)
                {
                    // dcf = cf / (1 + discountRate)^i+1
                    result += expectedCashFlow[i] / (decimal)Math.Pow(1 + discountRate, i + 1);
                }

                decimal growthRate = terminalGrowthRateInPercent / 100;
                decimal devisor = (decimal)discountRate - growthRate;
                if (devisor == 0)
                {
                    // making sure no 0
                    devisor = 0.00001m;
                }
                // terminal value = cashflow final year * (1 + cashflow grow rate) / (discount rate - cashflow grow rate)
                decimal terminalValue = (expectedCashFlow[expectedCashFlow.Count-1] * (1 + growthRate)) / devisor;

                return result + terminalValue;
            }
            catch (Exception x)
            {
                Log.Error(x);
                return -1;
            }
        }
    }
}
