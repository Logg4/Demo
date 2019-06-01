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
    /// Some common informations about the stock
    /// </summary>
    public class StockInfo
    {
        private readonly Stock _stock;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="stock"></param>
        public StockInfo(Stock stock)
        {
            _stock = stock;
            SetAverages();
            DividendGrowthRateInPercent = GetDividendGrowthRateInPercent();
            CashflowGrowthRateInPercent = GetCashflowGrowthRateInPercent();
        }

        /// <summary>
        /// Calc averages and set properties
        /// </summary>
        private void SetAverages()
        {
            try
            {
                decimal sumDiv = 0, sumDiv5 = 0, sumDiv3 = 0;
                decimal sumFreeCshFlow = 0, sumFreeCshFlow5 = 0, sumFreeCshFlow3 = 0;
                decimal sumSharePrice = 0, sumSharePrice5 = 0, sumSharePrice3 = 0;
                decimal sumNetIncome = 0, sumNetIncome5 = 0, sumNetIncome3 = 0;

                foreach (Financials finance in _stock.Financials)
                {
                    if (finance.IsTTM)
                    {
                        sumSharePrice5 += finance.SharePrice;
                        sumSharePrice3 += finance.SharePrice;
                    }
                    else
                    {
                        sumDiv += finance.Dividends;
                        sumFreeCshFlow += finance.FreeCashFlowPerShare;
                        sumNetIncome += finance.NetIncome;
                        if (DateTime.Now.Year <= finance.Date.Year + 5)
                        {
                            sumDiv5 += finance.Dividends;
                            sumFreeCshFlow5 += finance.FreeCashFlowPerShare;
                            sumNetIncome5 += finance.NetIncome;
                            sumSharePrice5 += finance.SharePrice;
                        }
                        if (DateTime.Now.Year <= finance.Date.Year + 3)
                        {
                            sumDiv3 += finance.Dividends;
                            sumFreeCshFlow3 += finance.FreeCashFlowPerShare;
                            sumNetIncome3 += finance.NetIncome;
                            sumSharePrice3 += finance.SharePrice;
                        }
                    }
                    sumSharePrice += finance.SharePrice;
                }

                // TTM is excluded, therefore - 1
                AvgDividends = sumDiv / (_stock.Financials.Count - 1);
                AvgFreeCashflow = sumFreeCshFlow / (_stock.Financials.Count - 1);
                AvgNetIncome = sumNetIncome / (_stock.Financials.Count - 1);
                AvgSharePrice = sumSharePrice / _stock.Financials.Count;

                AvgDividends5Y = sumDiv5 / 5;
                AvgFreeCashflow5Y = sumFreeCshFlow5 / 5;
                AvgNetIncome5Y = sumNetIncome5 / 5;
                // TTM is included
                AvgSharePrice5Y = sumSharePrice5 / 6;

                AvgDividends3Y = sumDiv3 / 3;
                AvgFreeCashflow3Y = sumFreeCshFlow3 / 3;
                AvgNetIncome3Y = sumNetIncome3 / 3;
                // TTM is included
                AvgSharePrice3Y = sumSharePrice3 / 4;
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
        }

        public decimal AvgDividends
        {
            get;set;
        }

        public decimal AvgFreeCashflow
        {
            get; set;
        }

        public decimal AvgNetIncome
        {
            get; set;
        }

        public decimal AvgSharePrice
        {
            get; set;
        }

        public decimal AvgDividends5Y
        {
            get; set;
        }

        public decimal AvgFreeCashflow5Y
        {
            get; set;
        }

        public decimal AvgNetIncome5Y
        {
            get; set;
        }

        public decimal AvgSharePrice5Y
        {
            get; set;
        }

        public decimal AvgDividends3Y
        {
            get; set;
        }

        public decimal AvgFreeCashflow3Y
        {
            get; set;
        }

        public decimal AvgNetIncome3Y
        {
            get; set;
        }

        public decimal AvgSharePrice3Y
        {
            get; set;
        }

        public float DividendGrowthRateInPercent
        {
            get;set;
        }

        public float CashflowGrowthRateInPercent
        {
            get; set;
        }


        /// <summary>
        /// Averaging dividend growth over all available data points
        /// </summary>
        /// <returns></returns>
        private float GetDividendGrowthRateInPercent()
        {

            try
            {
                decimal prevYearDiv = 0;
                decimal growth = 0;
                // TTM is not ignored, so we get a result which is more on the low side
                foreach (Financials finance in _stock.Financials)
                {
                    if (prevYearDiv == 0)
                    {
                        prevYearDiv = finance.Dividends;
                        continue;
                    }
                    if (0 < finance.Dividends)
                    {
                        decimal growthTmp = (finance.Dividends / prevYearDiv) - 1;
                        growth += growthTmp;
                    }
                    prevYearDiv = finance.Dividends;
                }

                if (growth <= 0)
                {
                    return 0;
                }

                float result = (float)(growth / _stock.Financials.Count) * 100;
                return result;
            }
            catch (Exception x)
            {
                Log.Error(x);
                return 0;
            }
        }

        /// <summary>
        /// Averaging cashflow growth over all available data points
        /// </summary>
        /// <returns></returns>
        public float GetCashflowGrowthRateInPercent()
        {
            try
            {
                decimal prevYearCashFlow = 0;
                decimal growth = 0;
                int yearsNotCounted = 0;

                foreach (Financials finance in _stock.Financials)
                {
                    if (prevYearCashFlow == 0)
                    {
                        prevYearCashFlow = finance.FreeCashFlowPerShare;
                        yearsNotCounted++;
                        continue;
                    }

                    decimal growthTmp = 0;
                    if (prevYearCashFlow < 0 && 0 < finance.FreeCashFlowPerShare)
                    {
                        // special case
                        growthTmp = (finance.FreeCashFlowPerShare - prevYearCashFlow) / finance.FreeCashFlowPerShare;
                    }
                    else
                    {
                        // year(i) - year(i-1) / year(i-1) => growth rate
                        growthTmp = (finance.FreeCashFlowPerShare - prevYearCashFlow) / Math.Abs(prevYearCashFlow);
                    }
                    growth += growthTmp;
                    prevYearCashFlow = finance.FreeCashFlowPerShare;
                }

                float result = (float)(growth / (_stock.Financials.Count - yearsNotCounted)) * 100;
                return result;
            }
            catch (Exception x)
            {
                Log.Error(x);
                return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public decimal GetLastDividend()
        {
            return _stock.Financials.Last().Dividends;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public decimal GetLastFreeCashFlow()
        {
            if (_stock.Financials.Last().FreeCashFlowPerShare == 0)
            {
                return _stock.Financials[_stock.Financials.Count - 2].FreeCashFlowPerShare;
            }
            else
            {
                return _stock.Financials.Last().FreeCashFlowPerShare;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Stock StockData
        {
            get
            {
                return _stock;
            }
        }
    }
}
