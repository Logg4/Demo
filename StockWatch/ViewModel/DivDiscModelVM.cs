using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;


using StockWatch.Model;
using StockWatch.Logic;
using LogWriterLib;

namespace StockWatch.ViewModel
{
    /// <summary>
    /// Handles Dividend Discount Model calculations
    /// </summary>
    public class DivDiscModelVM : BaseVM
    {
        private decimal _expectedDividend, _desiredRateOfReturn, _dividendGrowthRate, _intrinsicSharePrice, _sharePrice;
        private readonly DividendDiscountModel _ddm;
        private readonly StockInfo _stockInfo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stock"></param>
        public DivDiscModelVM(StockInfo stockInf)
        {
            try
            {
                _stockInfo = stockInf;
                _ddm = new DividendDiscountModel();
                // 10% (with inflation) s. https://www.investopedia.com/ask/answers/042415/what-average-annual-return-sp-500.asp
                // so we aim at 15
                DesiredRateOfReturn = 15;
                ExpectedDividend = CalcExpectedDividend();

                // dividend growth rate
                decimal growthRate = (decimal)_stockInfo.DividendGrowthRateInPercent;

                // 20 is max
                if (20 <= growthRate)
                {
                    growthRate = 20m;
                }
                DividendGrowthRate = growthRate;
                SharePrice = (decimal)_stockInfo.StockData.WeeklyQuotes.First().Close;
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private decimal CalcExpectedDividend()
        {
            // simply return to last dividend, maybe some averaging later
            return _stockInfo.GetLastDividend();
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetIntrinsicSharePrice()
        {
            try
            {
                IntrinsicSharePrice = _ddm.Calculate(_expectedDividend, _desiredRateOfReturn, _dividendGrowthRate);
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal ExpectedDividend
        {
            get
            {
                return _expectedDividend;
            }
            set
            {
                _expectedDividend = value;
                SetIntrinsicSharePrice();
                RaisePropertyChangedEvent("ExpectedDividend");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal DividendGrowthRate
        {
            get
            {
                return _dividendGrowthRate;
            }
            set
            {
                _dividendGrowthRate = value;
                SetIntrinsicSharePrice();
                RaisePropertyChangedEvent("DividendGrowthRate");
            }
        }

        /// <summary>
        /// Annual return rate in percent that is expected/desired
        /// </summary>
        public decimal DesiredRateOfReturn
        {
            get
            {
                return _desiredRateOfReturn;
            }
            set
            {
                _desiredRateOfReturn = value;
                SetIntrinsicSharePrice();
                RaisePropertyChangedEvent("DesiredRateOfReturn");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal IntrinsicSharePrice
        {
            get
            {
                return _intrinsicSharePrice;
            }
            set
            {
                _intrinsicSharePrice = value;
                RaisePropertyChangedEvent("IntrinsicSharePrice");
                RaisePropertyChangedEvent("IntrinsicForeground");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal SharePrice
        {
            get
            {
                return _sharePrice;
            }
            set
            {
                _sharePrice = value;
                RaisePropertyChangedEvent("SharePrice");
            }
        }

        public Brush IntrinsicForeground
        {
            get
            {
                if(_intrinsicSharePrice <= _sharePrice && 0 < _intrinsicSharePrice)
                {
                    return Brushes.Red;
                }
                else
                {
                    return Brushes.DarkGreen;
                }
            }
        }
    }
}
