using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using StockWatch.Model;
using StockWatch.Logic;
using LogWriterLib;

namespace StockWatch.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class IntrinsicOverviewVM : BaseVM
    {
        private readonly ObservableCollection<StockPerYearVM> _data;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stockData"></param>
        public IntrinsicOverviewVM(StockInfo stockInf)
        {
            try
            {
                _data = new ObservableCollection<StockPerYearVM>() {};

                // information to show:
                // free cashflow per year, avrg and 5y avrg
                // dividends per year, avrg and 5y avrg
                // stock price per year, avrg and 5y avrg
                foreach (Financials finance in stockInf.StockData.Financials)
                {
                    StockPerYearVM yearEntry = new StockPerYearVM
                    {
                        Dividends = finance.Dividends,
                        FreeCashFlowPerShare = finance.FreeCashFlowPerShare,
                        SharePrice = finance.SharePrice,
                        NetIncome = finance.NetIncome
                    };

                    if (finance.IsTTM)
                    {
                        yearEntry.YearOrAvg = "TTM";
                    }
                    else
                    {
                        yearEntry.YearOrAvg = finance.Date.Year.ToString();
                    }

                    Data.Add(yearEntry);
                }

                // add averages
                StockPerYearVM avgEntry = new StockPerYearVM
                {
                    YearOrAvg = "Average",
                    Dividends = stockInf.AvgDividends,
                    FreeCashFlowPerShare = stockInf.AvgFreeCashflow,
                    NetIncome = stockInf.AvgNetIncome,
                    SharePrice = stockInf.AvgSharePrice
                };
                Data.Add(avgEntry);

                StockPerYearVM avgEntry5 = new StockPerYearVM
                {
                    YearOrAvg = "Average (5Y)",
                    Dividends = stockInf.AvgDividends5Y,
                    FreeCashFlowPerShare = stockInf.AvgFreeCashflow5Y,
                    NetIncome = stockInf.AvgNetIncome5Y,
                    SharePrice = stockInf.AvgSharePrice5Y
                };
                Data.Add(avgEntry5);

                StockPerYearVM avgEntry3 = new StockPerYearVM
                {
                    YearOrAvg = "Average (3Y)",
                    Dividends = stockInf.AvgDividends3Y,
                    FreeCashFlowPerShare = stockInf.AvgFreeCashflow3Y,
                    NetIncome = stockInf.AvgNetIncome3Y,
                    SharePrice = stockInf.AvgSharePrice3Y
                };
                Data.Add(avgEntry3);

            }
            catch (Exception x)
            {
                Log.Error(x);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<StockPerYearVM> Data
        {
            get
            {
                return _data;
            }
            set
            {
            }
        }
    }
}
