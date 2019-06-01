using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

using StockWatch.Model;
using StockWatch.Logic;
using LogWriterLib;

namespace StockWatch.ViewModel
{
    public class CashFlowDiscModelVM : BaseVM
    {
        private decimal _desiredRateOfReturn, _intrinsicSharePrice, _sharePrice;
        private decimal _expectedCashflowYear1, _expectedCashflowYear2, _expectedCashflowYear3, _expectedCashflowYear4, _expectedCashflowYear5;
        private decimal _terminalGrowthRate;
        private float _dividendSliderMax;
        private readonly StockInfo _stockInf;
        private readonly CashFlowDiscountModel _cfdm;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stock"></param>
        public CashFlowDiscModelVM(StockInfo stock)
        {
            try
            {
                _stockInf = stock;
                _cfdm = new CashFlowDiscountModel();
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
                _sharePrice = (decimal)_stockInf.StockData.WeeklyQuotes.First().Close;
                TerminalGrowthRate = 0;

                decimal growthRate = (decimal)_stockInf.CashflowGrowthRateInPercent;

                // capß at 20
                if (20 < growthRate)
                {
                    growthRate = 20;
                }

                decimal startCashFlow = _stockInf.AvgFreeCashflow5Y;
                CashflowSliderMax = 20;
                _expectedCashflowYear1 = startCashFlow / 100 * (100 + growthRate);
                // 5% slowdown y2
                _expectedCashflowYear2 = ExpectedCashflowYear1 / 100 * (100 + (growthRate * 0.95m));
                // 10% slowdown y3
                _expectedCashflowYear3 = ExpectedCashflowYear2 / 100 * (100 + (growthRate * 0.90m));
                // 20% slowdown y4
                _expectedCashflowYear4 = ExpectedCashflowYear3 / 100 * (100 + (growthRate * 0.80m));
                // 30% slowdown y4
                _expectedCashflowYear5 = ExpectedCashflowYear4 / 100 * (100 + (growthRate * 0.70m));

                if (CashflowSliderMax < (float)(_expectedCashflowYear5 * 1.5m))
                {
                    CashflowSliderMax = (float)(_expectedCashflowYear5 * 1.5m);
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
                List<decimal> cashflows = new List<decimal>()
                        {ExpectedCashflowYear1, ExpectedCashflowYear2, ExpectedCashflowYear3, ExpectedCashflowYear4, ExpectedCashflowYear5};
                IntrinsicSharePrice = _cfdm.Calculate(cashflows, DesiredRateOfReturn, TerminalGrowthRate);
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public float CashflowSliderMax
        {
            get
            {
                return _dividendSliderMax;
            }
            set
            {
                _dividendSliderMax = value;
                RaisePropertyChangedEvent("CashflowSliderMax");
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

        /// <summary>
        /// 
        /// </summary>
        public decimal ExpectedCashflowYear1
        {
            get
            {
                return _expectedCashflowYear1;
            }
            set
            {
                _expectedCashflowYear1 = value;
                SetIntrinsicSharePrice();
                RaisePropertyChangedEvent("ExpectedCashflowYear1");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal ExpectedCashflowYear2
        {
            get
            {
                return _expectedCashflowYear2;
            }
            set
            {
                _expectedCashflowYear2 = value;
                SetIntrinsicSharePrice();
                RaisePropertyChangedEvent("ExpectedCashflowYear2");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal ExpectedCashflowYear3
        {
            get
            {
                return _expectedCashflowYear3;
            }
            set
            {
                _expectedCashflowYear3 = value;
                SetIntrinsicSharePrice();
                RaisePropertyChangedEvent("ExpectedCashflowYear3");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal ExpectedCashflowYear4
        {
            get
            {
                return _expectedCashflowYear4;
            }
            set
            {
                _expectedCashflowYear4 = value;
                SetIntrinsicSharePrice();
                RaisePropertyChangedEvent("ExpectedCashflowYear4");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal ExpectedCashflowYear5
        {
            get
            {
                return _expectedCashflowYear5;
            }
            set
            {
                _expectedCashflowYear5 = value;
                SetIntrinsicSharePrice();
                RaisePropertyChangedEvent("ExpectedCashflowYear5");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal TerminalGrowthRate
        {
            get
            {
                return _terminalGrowthRate;
            }
            set
            {
                _terminalGrowthRate = value;
                SetIntrinsicSharePrice();
                RaisePropertyChangedEvent("TerminalGrowthRate");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Brush IntrinsicForeground
        {
            get
            {
                if (_intrinsicSharePrice <= _sharePrice)
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
            RaisePropertyChangedEvent("ExpectedCashflowYear1");
            RaisePropertyChangedEvent("ExpectedCashflowYear2");
            RaisePropertyChangedEvent("ExpectedCashflowYear3");
            RaisePropertyChangedEvent("ExpectedCashflowYear4");
            RaisePropertyChangedEvent("ExpectedCashflowYear5");
            RaisePropertyChangedEvent("TerminalGrowthRate");
        }
    }
}
