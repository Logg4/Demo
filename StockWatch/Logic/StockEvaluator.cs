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
    /// 
    /// </summary>
    public class StockEvaluator
    {
        Stock _stockUnderEvaluation;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stock"></param>
        public StockEvaluator(Stock stock)
        {
            _stockUnderEvaluation = stock;
        }

        /// <summary>
        ///  Benjamin Graham - The Intelligent Investor - on stock selection:
        /// 
        ///  1. Adequate Size of the Enterprise
        ///  All our minimum figures must be arbitrary and especially in the
        ///  matter of size required. Our idea is to exclude small companies
        ///  which may be subject to more than average vicissitudes especially
        ///  in the industrial field. (There are often good possibilities in such
        ///  enterprises but we do not consider them suited to the needs of the
        ///  defensive investor.) Let us use round amounts: not less than $100
        ///  million of annual sales for an industrial company and, not less than
        ///  $50 million of total assets for a public utility.
        /// 
        ///  2. A Sufficiently Strong Financial Condition
        ///  For industrial companies current assets should be at least twice
        ///  current liabilities—a so-called two-to-one current ratio. Also, longterm
        ///  debt should not exceed the net current assets (or “working
        ///  capital”). For public utilities the debt should not exceed twice the
        ///  stock equity (at book value).
        /// 
        ///  3. Earnings Stability
        ///  Some earnings for the common stock in each of the past ten
        ///  years.
        /// 
        ///  4. Dividend Record
        ///  Uninterrupted payments for at least the past 20 years.
        /// 
        ///  5. Earnings Growth
        ///  A minimum increase of at least one-third in per-share earnings
        ///  in the past ten years using three-year averages at the beginning
        ///  and end.
        /// 
        ///  6. Moderate Price/Earnings Ratio
        ///  Current price should not be more than 15 times average earnings
        ///  of the past three years.
        /// 
        ///  7. Moderate Ratio of Price to Assets
        ///  Current price should not be more than 11⁄2 times the book value last
        ///  reported. However, a multiplier of earnings below 15 could justify a
        ///  correspondingly higher multiplier of assets. As a rule of thumb we
        ///  suggest that the product of the multiplier times the ratio of price to
        ///  book value should not exceed 22.5. (This figure corresponds to 15
        ///  times earnings and 11⁄2 times book value. It would admit an issue selling
        ///  at only 9 times earnings and 2.5 times asset value, etc.)
        ///  
        /// 
        /// TODO: for more precision we need an evaluation based on sector (e.g. technology, industry and so on)
        /// </summary>
        /// <returns></returns>
        public short GrahamPoints(List<string> positives)
        {
            short result = 0;
            try
            {
                if (_stockUnderEvaluation == null || _stockUnderEvaluation.Financials == null)
                {
                    return 0;
                }
                if (IsLargeEnough())
                {
                    result++;
                    positives.Add("IsLargeEnough");
                }

                if (IsFinanciallyStrong())
                {
                    result++;
                    positives.Add("IsFinanciallyStrong");
                }

                if (HasEarningsStability())
                {
                    result++;
                    positives.Add("HasEarningsStability");
                }

                if (HasDividendStability())
                {
                    result++;
                    positives.Add("HasDividendStability");
                }

                if (HasEarningsGrowth())
                {
                    result++;
                    positives.Add("HasEarningsGrowth");
                }
                if (HasBellowMedianPe())
                {
                    result++;
                    positives.Add("HasBellowMedianPe");
                }
                if (HasModerateRatioPriceToAssets())
                {
                    result++;
                    positives.Add("HasModerateRatioPriceToAssets");
                }
            }
            catch(Exception x)
            {
                Log.Error(x);
            }
            return result;
        }


        /// <summary>
        /// Current price should not be more than 11⁄2 times the book value last
        /// reported. 
        /// However, a multiplier of earnings below 15 could justify a
        /// correspondingly higher multiplier of assets. As a rule of thumb we
        /// suggest that the product of the multiplier times the ratio of price to
        /// book value should not exceed 22.5
        /// </summary>
        /// <returns></returns>
        private bool HasModerateRatioPriceToAssets()
        {
            Financials lastStatement = _stockUnderEvaluation.Financials.Last();

            if (lastStatement.SharePrice < (lastStatement.BookValuePerShare * 11m / 2m))
            {
                return true;
            }

            decimal ratio = lastStatement.PriceToEarningsRatio * (lastStatement.SharePrice / lastStatement.BookValuePerShare);

            // adjusted just like in HasBellowMedianPe()
            if (ratio <= 25.5m)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///  Current price should not be more than 15 times average earnings
        ///  of the past three years. 
        ///  15 seems a little bit to low, will go with 18
        /// s. https://www.multpl.com/s-p-500-pe-ratio
        /// </summary>
        /// <returns></returns>
        private bool HasBellowMedianPe()
        {
            if (_stockUnderEvaluation.Financials.Count < 4)
            {
                // not enough data
                return false;
            }

            int count = _stockUnderEvaluation.Financials.Count;
            // ignore ttm (last one)
            decimal currentPe = (_stockUnderEvaluation.Financials[count - 2].PriceToEarningsRatio +
                _stockUnderEvaluation.Financials[count - 3].PriceToEarningsRatio +
                _stockUnderEvaluation.Financials[count - 4].PriceToEarningsRatio) / 3;
            if (currentPe < 18)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// A minimum increase of at least one-third in per-share earnings
        /// in the past ten years using three-year averages at the beginning
        /// and end.
        private bool HasEarningsGrowth()
        {
            if (_stockUnderEvaluation.Financials.Count < 8)
            {
                // not enough data
                return false;
            }

            decimal pastEarnings = (_stockUnderEvaluation.Financials[0].EarningsPerShare +
                            _stockUnderEvaluation.Financials[1].EarningsPerShare +
                            _stockUnderEvaluation.Financials[2].EarningsPerShare) / 3;

            int count = _stockUnderEvaluation.Financials.Count;
            // ignore ttm (last one)
            decimal currentEarnings = (_stockUnderEvaluation.Financials[count-2].EarningsPerShare +
                _stockUnderEvaluation.Financials[count-3].EarningsPerShare +
                _stockUnderEvaluation.Financials[count - 4].EarningsPerShare) / 3;

            if ((pastEarnings * 1.33m) < currentEarnings)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///  Uninterrupted payments for at least the past 20 years.
        /// </summary>
        /// <returns></returns>
        private bool HasDividendStability()
        {
            foreach (Financials statement in _stockUnderEvaluation.Financials)
            {
                if (statement.Dividends <= 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Some earnings for the common stock in each of the past ten
        /// years.
        /// <returns></returns>
        private bool HasEarningsStability()
        {

            foreach (Financials statement in _stockUnderEvaluation.Financials)
            {
                if (statement.EarningsPerShare <= 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///  For industrial companies current assets should be at least twice
        ///  current liabilities—a so-called two-to-one current ratio. 
        ///  Also, longterm debt should not exceed the net current assets (or “working
        ///  capital”). 
        ///  For public utilities the debt should not exceed twice the
        ///  stock equity (at book value).
        /// </summary>
        /// <returns></returns>
        private bool IsFinanciallyStrong()
        {
            Financials lastStatement = _stockUnderEvaluation.Financials.Last();

            // soem companies have lastStatement.TotalLiabilities = 0 not sure why, therefore ignoring those
            if (lastStatement.TotalLiabilities * 2 <= lastStatement.TotalAssets)
            {
                return true;
            }

            if (lastStatement.LongTermDebt < lastStatement.TotalAssets)
            {
                return true;
            }

            if (lastStatement.LongTermDebt < (lastStatement.BookValuePerShare * lastStatement.Shares) * 2)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///  Not less than $100 million of annual sales for an industrial company and, 
        ///  not less than $50 million of total assets for a public utility.
        /// </summary>
        /// <returns></returns>
        private bool IsLargeEnough()
        {
            Financials lastStatement = _stockUnderEvaluation.Financials.Last();
            if (100000000 < lastStatement.Revenue)
            {
                return true;
            }
            if (50000000 < lastStatement.TotalAssets)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Calculate stock score based on different criteria
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        public float GetRating()
        {
            try
            {
                //int ratings = 0;



                return 0f;
                //float result = NetIncomeRating();
                //++ratings;
                //result += PriceToEarningsRating();
                //++ratings;
                //result += DividendsRating();
                //++ratings;
                //result += EarningsPerShareGrowthRating();
                //++ratings;
                //// TODO: rating for financial health 

                //return result/ratings;
            }
            catch (Exception x)
            {
                Log.Error(x);
                return 0f;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private float DividendsRating()
        {
            try
            {
                float result = 0f;

                bool dividendsEveryYear = true;
                int years = 0;
                decimal average = 0;
                foreach (Financials finance in _stockUnderEvaluation.Financials)
                {
                    if (finance.Dividends <= 0)
                    {
                        dividendsEveryYear = false;
                    }
                    average += finance.Dividends;
                    years++;
                }

                average = average / years;
                decimal currentDividends = _stockUnderEvaluation.Financials.Last().Dividends;

                if (average <= 0)
                {
                    result = -100;
                }
                else if (currentDividends <= 0)
                {
                    result = 0;
                }
                else
                {
                    result = (float)((currentDividends / average)/(1m + (currentDividends / average)) *100);
                }


                if (dividendsEveryYear)
                {
                    result += 25;    
                }
                else
                {
                    result -= 25;
                }

                // cap at -100 & 100
                if (result < -100)
                {
                    result = -100;
                }
                else if (100 < result)
                {
                    result = 100;
                }

                return result;
            }
            catch (Exception x)
            {
                Log.Error(x);
                return 0f;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private float PriceToEarningsRating()
        {
            try
            {
                float result = 0;

                // PSE of 21 is current S&P average
                float sAndPAverage = 21;
                float current = (float)_stockUnderEvaluation.Financials.Last().PriceToEarningsRatio;

                if (current < 0)
                {
                    // map negative pse directly to result
                    result = current;
                }
                else 
                {
                    result = 100 - (current / sAndPAverage) * 50;
                }

                // cap at -100 & 100
                if (result < -100)
                {
                    result = -100;
                }
                else if (100 < result)
                {
                    result = 100;
                }

                return result;
            }
            catch (Exception x)
            {
                Log.Error(x);
                return 0f;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private float NetIncomeRating()
        {
            try
            {
                float result = 0;
                decimal overallNetIncomeExceptLast = 0;
                int years = 0;
                // last years average except ttm
                for (int i = 0; i < _stockUnderEvaluation.Financials.Count; i++)
                {
                    if (_stockUnderEvaluation.Financials[i].IsTTM)
                    {
                        break;
                    }

                    // if ttm and this years netincome is the same then don't add it 
                    if (_stockUnderEvaluation.Financials[i + 1].IsTTM &&
                        _stockUnderEvaluation.Financials[i + 1].NetIncome == _stockUnderEvaluation.Financials[i].NetIncome)
                    {
                        break;
                    }

                    overallNetIncomeExceptLast += _stockUnderEvaluation.Financials[i].NetIncome;
                    ++years;
                }

                decimal average = overallNetIncomeExceptLast / years;
                decimal current = _stockUnderEvaluation.Financials.Last().NetIncome;


                decimal distanceToAverage = 0;
                if (0 < current)
                {
                    result = 50;
                }
                else
                {
                    result = -50;
                }

                distanceToAverage = current - average;


                // asymptotic function (-100 - 100)
                if (0 < distanceToAverage)
                {
                    result += (float)((distanceToAverage / average) / (1 + (distanceToAverage / average)) * 50);
                }
                else
                {
                    result -= (float)(Math.Abs(distanceToAverage / average) / (1 + Math.Abs(distanceToAverage / average)) * 50);
                }

                return result;
            }
            catch (Exception x)
            {
                Log.Error(x);
                return 0f;
            }
        }


        /// <summary>
        /// TODO: think of a better metric
        /// </summary>
        /// <returns></returns>
        private float EarningsPerShareGrowthRating()
        {
            try
            {
                float result = 0f;
                int years = 0;
                decimal average = 0;
                decimal currentRatio = 0;

                // first year is 0 -> ignore
                for (int i = 1; i < _stockUnderEvaluation.Financials.Count; i++)
                {
                    if (_stockUnderEvaluation.Financials[i].IsTTM)
                    {
                        if (_stockUnderEvaluation.Financials[i].EarningsPerShareGrowthRatio != 0)
                        {
                            currentRatio = _stockUnderEvaluation.Financials[i].EarningsPerShareGrowthRatio;
                        }
                        break;
                    }

                    // if ttm and this years is the same then don't add it 
                    if (_stockUnderEvaluation.Financials[i + 1].IsTTM &&
                        _stockUnderEvaluation.Financials[i + 1].EarningsPerShareGrowthRatio == _stockUnderEvaluation.Financials[i].EarningsPerShareGrowthRatio)
                    {
                        break;
                    }

                    average += _stockUnderEvaluation.Financials[i].EarningsPerShareGrowthRatio;
                    currentRatio = _stockUnderEvaluation.Financials[i].EarningsPerShareGrowthRatio;
                    ++years;
                }

                average = average / years;

                decimal diffToAverage = currentRatio - average;
                decimal delta = 0;
                if (currentRatio != 0)
                {
                    delta = Math.Abs(diffToAverage) / Math.Abs(currentRatio);
                }

                if (0 < diffToAverage)
                {
                    // ratio difference is positive -> good
                    result = (float)(100 * delta);
                }
                else
                {
                    result = -(float)(100 * delta);
                }

                if (currentRatio < 0)
                {
                    result -= 25;
                }

                // cap at -100 & 100
                if (result < -100)
                {
                    result = -100;
                }
                else if (100 < result)
                {
                    result = 100;
                }

                return result;
            }
            catch (Exception x)
            {
                Log.Error(x);
                return 0f;
            }
        }
    }
}
