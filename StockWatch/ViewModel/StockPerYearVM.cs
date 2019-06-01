using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWatch.ViewModel
{
    public class StockPerYearVM : BaseVM
    {

        public string YearOrAvg
        {
            get;set;
        }

        public decimal FreeCashFlowPerShare
        {
            get; set;
        }

        public decimal Dividends
        {
            get; set;
        }

        public decimal SharePrice
        {
            get; set;
        }

        public decimal NetIncome
        {
            get;set;
        }
    }
}
