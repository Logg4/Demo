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
    /// Handles Multi Period Dividend Discount Model calculations
    /// </summary>
    public class MpDivDiscModelVM : BaseVM
    {
        private decimal _desiredRateOfReturn, _intrinsicSharePrice, _sharePrice, _expectedSellPrice;
        private decimal _expectedDividendYear1, _expectedDividendYear2, _expectedDividendYear3, _expectedDividendYear4, _expectedDividendYear5;
        private float _dividendSliderMax, _sellPriceSliderMax;
        private readonly StockInfo _stockInfo;
        private readonly DividendDiscountModel _ddm;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stock"></param>
        public MpDivDiscModelVM(StockInfo stockInfo)
        {
            try
            {
                _stockInfo = stockInfo;
                _ddm = new DividendDiscountModel();
                SetFields();
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SetFields()
        {
            try
            {

                _sharePrice = (decimal)_stockInfo.StockData.WeeklyQuotes.First().Close;
                _expectedSellPrice = SharePrice;
                SellPriceSliderMax = (float)_expectedSellPrice * 2.0f;

                decimal growthRate = (decimal)_stockInfo.DividendGrowthRateInPercent;
                // 15 is max
                if (15 <= growthRate)
                {
                    growthRate = 14.99m;
                }

                decimal lastDividend = _stockInfo.GetLastDividend();
                DividendSliderMax = 20;
                _expectedDividendYear1 = lastDividend/100 * (100+growthRate);
                // 5% slowdown y2
                _expectedDividendYear2 = ExpectedDividendYear1/100 * (100+ (growthRate * 0.95m));
                // 10% slowdown y3
                _expectedDividendYear3 = ExpectedDividendYear2/100 * (100 + (growthRate * 0.90m));
                // 20% slowdown y4
                _expectedDividendYear4 = ExpectedDividendYear3/100 * (100 + (growthRate * 0.80m));
                // 30% slowdown y4
                _expectedDividendYear5 = ExpectedDividendYear4/100 * (100 + (growthRate * 0.70m));

                if (DividendSliderMax < (float)(_expectedDividendYear5 * 1.5m))
                {
                    DividendSliderMax = (float)(_expectedDividendYear5 * 1.5m);
                }

                // 10% (with inflation) s. https://www.investopedia.com/ask/answers/042415/what-average-annual-return-sp-500.asp
                // so we aim at 15
                _desiredRateOfReturn = 15;

                SetIntrinsicSharePrice();
                RaiseAllPropertyChangedEvents();
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        private void SetIntrinsicSharePrice()
        {
            try
            {
                List<decimal> dividends = new List<decimal>()
                        { ExpectedDividendYear1, ExpectedDividendYear2, ExpectedDividendYear3, ExpectedDividendYear4, ExpectedDividendYear5 };
                IntrinsicSharePrice = _ddm.Calculate(dividends, DesiredRateOfReturn, ExpectedSellPrice);

            }
            catch (Exception x)
            {
                Log.Error(x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float DividendSliderMax
        {
            get
            {
                return _dividendSliderMax;
            }
            set
            {
                _dividendSliderMax = value;
                RaisePropertyChangedEvent("DividendSliderMax");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float SellPriceSliderMax
        {
            get
            {
                return _sellPriceSliderMax;
            }
            set
            {
                _sellPriceSliderMax = value;
                RaisePropertyChangedEvent("SellPriceSliderMax");
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

        public decimal ExpectedDividendYear1
        {
            get
            {
                return _expectedDividendYear1;
            }
            set
            {
                _expectedDividendYear1 = value;
                SetIntrinsicSharePrice();
                RaisePropertyChangedEvent("ExpectedDividendYear1");
            }
        }

        public decimal ExpectedDividendYear2
        {
            get
            {
                return _expectedDividendYear2;
            }
            set
            {
                _expectedDividendYear2 = value;
                SetIntrinsicSharePrice();
                RaisePropertyChangedEvent("ExpectedDividendYear2");
            }
        }

        public decimal ExpectedDividendYear3
        {
            get
            {
                return _expectedDividendYear3;
            }
            set
            {
                _expectedDividendYear3 = value;
                SetIntrinsicSharePrice();
                RaisePropertyChangedEvent("ExpectedDividendYear3");
            }
        }

        public decimal ExpectedDividendYear4
        {
            get
            {
                return _expectedDividendYear4;
            }
            set
            {
                _expectedDividendYear4 = value;
                SetIntrinsicSharePrice();
                RaisePropertyChangedEvent("ExpectedDividendYear4");
            }
        }

        public decimal ExpectedDividendYear5
        {
            get
            {
                return _expectedDividendYear5;
            }
            set
            {
                _expectedDividendYear5 = value;
                SetIntrinsicSharePrice();
                RaisePropertyChangedEvent("ExpectedDividendYear5");
            }
        }

        public decimal ExpectedSellPrice
        {
            get
            {
                return _expectedSellPrice;
            }
            set
            {
                _expectedSellPrice = value;
                SetIntrinsicSharePrice();
                RaisePropertyChangedEvent("ExpectedSellPrice");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Brush IntrinsicForeground
        {
            get
            {
                if (_intrinsicSharePrice <= _sharePrice && 0 < _intrinsicSharePrice)
                {
                    return Brushes.Red;
                }
                else
                {
                    return Brushes.DarkGreen;
                }
            }
        }

        /// <summary>
        /// Used to bypass SetIntrinsicSharePrice() calculation
        /// </summary>
        private void RaiseAllPropertyChangedEvents()
        {
            RaisePropertyChangedEvent("DesiredRateOfReturn");
            RaisePropertyChangedEvent("IntrinsicSharePrice");
            RaisePropertyChangedEvent("SharePrice");
            RaisePropertyChangedEvent("ExpectedDividendYear1");
            RaisePropertyChangedEvent("ExpectedDividendYear2");
            RaisePropertyChangedEvent("ExpectedDividendYear3");
            RaisePropertyChangedEvent("ExpectedDividendYear4");
            RaisePropertyChangedEvent("ExpectedDividendYear5");
            RaisePropertyChangedEvent("ExpectedSellPrice");
        }
    }
}
