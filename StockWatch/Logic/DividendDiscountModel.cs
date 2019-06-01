using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LogWriterLib;

namespace StockWatch.Logic
{
    /// <summary>
    /// Dividend discount model is a method to determine the intrinsic value of a company
    /// </summary>
    public class DividendDiscountModel
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expextedDividend"></param>
        /// <param name="discountRate"></param>
        /// <param name="dividendGrowthRate"></param>
        /// <returns></returns>
        public decimal Calculate(decimal expextedDividend, decimal discountRateInPercent, decimal dividendGrowthRateInPercent)
        {
            try
            {
                decimal discountRate = 1 + (discountRateInPercent / 100);
                decimal dividendGrowthRate = 1 + (dividendGrowthRateInPercent / 100);
                decimal divider = (discountRate - dividendGrowthRate);
                if (0 < divider)
                {
                    return expextedDividend / divider;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception x)
            {
                Log.Error(x);
                return -1;
            }
        }

        /// <summary>
        /// Multi-period Dividend Discount Model
        /// </summary>
        /// <param name="expectedDividends"></param>
        /// <param name="discountRate"></param>
        /// <param name="expectedSellPrice"></param>
        /// <returns></returns>
        public decimal Calculate(List<decimal> expectedDividends, decimal discountRateInPercent, decimal expectedSellPrice)
        {
            try
            {
                if (expectedDividends == null || expectedDividends.Count == 0)
                {
                    return -1;
                }

                double discountRate = 1 + (double)(discountRateInPercent / 100);
                if (discountRate == 0)
                {
                    return -1;
                }

                decimal result = 0;
                for (int i = 0; i < expectedDividends.Count; i++)
                {
                    result += expectedDividends[i] / (decimal)Math.Pow(discountRate, i+1);
                }
                result += expectedSellPrice / (decimal)Math.Pow(discountRate, expectedDividends.Count); 

                return result;
            }
            catch (Exception x)
            {
                Log.Error(x);
                return -1;
            }
        }
    }
}
