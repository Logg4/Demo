using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

using StockWatch.Model;
using StockWatch.Logic;
using LogWriterLib;

namespace StockWatch.ViewModel
{
    public class ResidualIncomeModelVM : BaseVM
    {
        private decimal _desiredRateOfReturn, _intrinsicSharePrice, _sharePrice;
        private readonly StockInfo _stockInfo;
        private readonly ResidualIncomeModel _resIncModel;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stock"></param>
        public ResidualIncomeModelVM(StockInfo stockInf)
        {
            try
            {
                _stockInfo = stockInf;
                _resIncModel = new ResidualIncomeModel();
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
                SharePrice = (decimal)_stockInfo.StockData.WeeklyQuotes.First().Close;
                // 10% (with inflation) s. https://www.investopedia.com/ask/answers/042415/what-average-annual-return-sp-500.asp
                // so we aim at 15
                DesiredRateOfReturn = 15;
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
                IntrinsicSharePrice = _resIncModel.Calculate(_stockInfo.StockData.Financials.Last(), DesiredRateOfReturn);
            }
            catch (Exception x)
            {
                Log.Error(x);
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
    }
}
