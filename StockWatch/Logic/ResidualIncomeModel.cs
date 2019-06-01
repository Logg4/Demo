using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockWatch.Model;
using LogWriterLib;

namespace StockWatch.Logic
{
    /// <summary>
    /// Intrisic value based on book value, earnings per share and opportunity cost (discount rate)
    /// </summary>
    public class ResidualIncomeModel
    {
        public decimal Calculate(Financials financealData, decimal discountRateInPercent)
        {
            try
            {
                decimal bookVal = financealData.BookValuePerShare;
                decimal eps = financealData.EarningsPerShare;
                decimal discountRate = discountRateInPercent / 100;

                decimal result = bookVal + (eps - (discountRate * bookVal)) / discountRate;
                return result;
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
            return 0;
        }
    }
}
