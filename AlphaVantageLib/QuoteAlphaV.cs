using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaVantageLib
{
    public class QuoteAlphaV
    {
        public float Open
        {
            get; set;
        }

        public float Close
        {
            get; set;
        }

        public float High
        {
            get; set;
        }

        public float Low
        {
            get; set;
        }

        public long Volume
        {
            get; set;
        }

        public float Dividend
        {
            get; set;
        }

        public float SplitCoefficient
        {
            get; set;
        }

        public DateTime Date
        {
            get; set;
        }
    }
}
