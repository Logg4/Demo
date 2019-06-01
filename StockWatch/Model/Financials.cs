using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LiteDB;

namespace StockWatch.Model
{
    public class Financials
    {
        public DateTime Date
        {
            get;set;
        }

        public bool IsTTM
        {
            get;set;
        }

        public string Currency
        {
            get; set;
        }

        public decimal Revenue
        {
            get; set;
        }

        public decimal NetIncome
        {
            get; set;
        }

        [BsonField("EarningsperShare")]
        public decimal EarningsPerShare
        {
            get; set;
        }

        public decimal BookValuePerShare
        {
            get; set;
        }

        public decimal FreeCashFlow
        {
            get; set;
        }

        public decimal FreeCashFlowPerShare
        {
            get;set;
        }

        public decimal Dividends
        {
            get;set;
        }

        public decimal SharePrice
        {
            get;set;
        }

        public long Shares
        {
            get; set;
        }

        public float DebtToEquityRatio
        {
            get;set;
        }

        /// <summary>
        /// Operating income = EBIT
        /// </summary>
        public decimal OperatingIncome
        {
            get;set;
        }

        public decimal TotalAssets
        {
            get;set;
        }

        public decimal TotalLiabilities
        {
            get; set;
        }

        public decimal CurrentLiabilities
        {
            get; set;
        }

        public decimal LongTermDebt
        {
            get; set;
        }

        public decimal ShortTermDebt
        {
            get; set;
        }

        public decimal TotalCash
        {
            get;set;
        }

        public decimal CashAndCashEquivalents
        {
            get;set;
        }

        public decimal WorkingCapital
        {
            get;set;
        }

        [BsonIgnore]
        public Financials PreviousFinancialData
        {
            get;
            set;
        }

        /// <summary>
        /// Measures the market price of a company’s stock relative to its corporate earnings, which can then be compared with other (similar) companies
        /// </summary>
        [BsonIgnore]
        public decimal PriceToEarningsRatio
        {
            get
            {
                if (EarningsPerShare == 0)
                {
                    return 0;
                }
                decimal result = SharePrice / EarningsPerShare;
                return result;
            }
        }

        /// <summary>
        /// The P/B ratio measures the market's valuation of a company relative to its book value.
        /// </summary>
        [BsonIgnore]
        public decimal PriceToBookRatio
        {
            get
            {
                if (BookValuePerShare == 0)
                {
                    return 0;
                }
                decimal result = SharePrice / BookValuePerShare;
                return result;
            }
        }

        /// <summary>
        /// Dividends in percent
        /// </summary>
        [BsonIgnore]
        public float DividendYield
        {
            get
            {
                if (SharePrice == 0)
                {
                    return 0;
                }
                float result = (float)(Dividends / SharePrice);
                return result;
            }
        }

        /// <summary>
        /// How much (in percent) did earnings grow compared to previous measurement
        /// </summary>
        [BsonIgnore]
        public decimal EarningsPerShareGrowthRatio
        {
            get
            {
                decimal result = 0;
                if (PreviousFinancialData != null && PreviousFinancialData.EarningsPerShare != 0)
                {
                    result = ((EarningsPerShare - PreviousFinancialData.EarningsPerShare) /
                                                            Math.Abs(PreviousFinancialData.EarningsPerShare)) * 100;
                }

                return result;
            }
        }

        /// <summary>
        /// price/earnings to growth ratio is a valuation metric for determining the relative trade-off between the price of a stock, 
        /// the earnings generated per share (EPS), and the company's expected growth
        /// </summary>
        [BsonIgnore]
        public decimal PEGRatio
        {
            get
            {
                decimal result = 0;
                if (PreviousFinancialData != null && EarningsPerShareGrowthRatio != 0)
                {
                    result = PriceToEarningsRatio / EarningsPerShareGrowthRatio;
                }

                return result;
            }
        }

        /// <summary>
        /// 1)  Return on capital
        /// (Return on capital = ROC)
        /// ROC = Operating Income (EBIT) / (Total Assets - Current Liabilities)
        /// </summary>
        [BsonIgnore]
        public decimal ReturnOnCapital
        {
            get
            {
                decimal result;

                decimal capital = TotalAssets - CurrentLiabilities;
                if (capital == 0)
                {
                    return 0;
                }
                if (OperatingIncome != 0)
                {
                    result = OperatingIncome / capital;
                }
                else
                {
                    result = NetIncome / capital;
                }

                return result;
            }
        }

        /// <summary>
        /// enterprise value = (share price * shares outstanding) + (Long-term debt + Short-term debt) - CashAndCashEquivalents 
        /// s. https://www.investopedia.com/terms/e/enterprisevalue.asp
        /// </summary>
        public decimal EnterpriseValue
        {
            get
            {
                decimal result = SharePrice * Shares + (LongTermDebt + ShortTermDebt) - CashAndCashEquivalents;

                return result;
            }
        }

        /// <summary>
        /// Eernings Yield = Operating Income (EBIT) / enterprise value
        /// </summary>
        public decimal EerningsYield
        {
            get
            {
                if (EnterpriseValue == 0)
                {
                    return 0;
                }

                decimal result;
                if (OperatingIncome != 0)
                {
                    result = OperatingIncome / EnterpriseValue;
                }
                else
                {
                    result = NetIncome / EnterpriseValue;
                }

                return result;
            }
        }
    }
}
