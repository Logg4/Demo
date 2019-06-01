using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LiteDB;
using StockWatch.Model;
using StockWatch.Logic;
using LogWriterLib;

namespace StockWatch.ViewModel
{
    public class StockDataVM : BaseVM
    {
        private readonly Stock _stockData;
        private string _task = "";
        private float? _rating = null;
        private float _rank = 0;

        public StockDataVM(Stock stock)
        {
            _stockData = stock;
        }

        public Stock StockData
        {
            get
            {
                return _stockData;
            }
        }

        public string CompanyName
        {
            get
            {
                return StockData.Company;
            }
            set
            {
                StockData.Company = value;
                RaisePropertyChangedEvent("CompanyName");
            }
        }

        public string Ticker
        {
            get
            {
                return (StockData?.TickerM ?? "").ToUpper();
            }
            set
            {
                StockData.TickerM = value;
                RaisePropertyChangedEvent("Ticker");
            }
        }

        public float SharePrice
        {
            get
            {
                if(StockData.WeeklyQuotes != null && 0 < StockData.WeeklyQuotes.Count)
                {
                    return StockData?.WeeklyQuotes?.First()?.Close ?? 0;
                }
                return 0;
            }
        }

        public decimal BookValuePerShare
        {
            get
            {
                if (StockData?.Financials != null)
                {
                    return StockData?.Financials?.Last()?.BookValuePerShare ?? 0;
                }
                return 0;
            }
        }

        public decimal EarningsPerShare
        {
            get
            {
                if (StockData?.Financials != null)
                {
                    return StockData?.Financials?.Last()?.EarningsPerShare ?? 0;
                }
                return 0;
            }
        }

        public decimal PriceToEarnings
        {
            get
            {
                if (EarningsPerShare == 0)
                {
                    return 0;
                }
                decimal result = (decimal)SharePrice / EarningsPerShare;
                return Math.Round(result, 2);
            }
        }

        public decimal Dividend
        {
            get
            {
                decimal result = StockData?.Financials?.Last()?.Dividends ?? 0;
                return Math.Round(result, 2);
            }
            set
            {
                try
                {
                    StockData.Financials.Last().Dividends = value;
                }
                catch (Exception x)
                {
                    Log.Error(x);
                }
                RaisePropertyChangedEvent("Dividend");
            }
        }

        public decimal DividendYield
        {
            get
            {
                if (SharePrice == 0)
                {
                    return 0;
                }
                decimal result = ((StockData?.Financials?.Last()?.Dividends ?? 0) / (decimal)SharePrice);
                return Math.Round(result, 2);
            }
        }

        public decimal PriceToEarningsToGrowth
        {
            get
            {
                if (EarningsPerShare == 0)
                {
                    return 0;
                }
                decimal result = PriceToEarnings / EpsGrowth;
                return Math.Round(result, 4);
            }
        }

        public decimal EpsGrowth
        {
            get
            {
                try
                {
                    if (EarningsPerShare == 0)
                    {
                        return 0;
                    }

                    decimal epsGrowth = (StockData.Financials.Last().EarningsPerShare -
                                            StockData.Financials[StockData.Financials.Count - 2].EarningsPerShare)
                                            / StockData.Financials[StockData.Financials.Count - 2].EarningsPerShare;
                    if (epsGrowth == 0)
                    {
                        // TTM and last years data is often the same, so with this we make sure always getting the 'real' eps growth 
                        epsGrowth = (StockData.Financials[StockData.Financials.Count - 2].EarningsPerShare -
                                            StockData.Financials[StockData.Financials.Count - 3].EarningsPerShare)
                                            / StockData.Financials[StockData.Financials.Count - 3].EarningsPerShare;
                    }

                    epsGrowth = epsGrowth * 100;
                    return Math.Round(epsGrowth, 4);
                }
                catch (Exception x)
                {
                    Log.Error(x);
                    return 0;
                }
            }
        }

        public decimal Revenue
        {
            get
            {
                if (EarningsPerShare == 0)
                {
                    return 0;
                }
                decimal result = StockData?.Financials?.Last()?.Revenue ?? 0;
                return Math.Round(result, 2);
            }
        }

        public decimal NetIncome
        {
            get
            {
                if (EarningsPerShare == 0)
                {
                    return 0;
                }
                decimal result = StockData?.Financials?.Last()?.NetIncome ?? 0;
                return Math.Round(result, 2);
            }
        }

        
        public float DebtToEquityRatio
        {
            get
            {
                float result = StockData?.Financials?.Last()?.DebtToEquityRatio ?? 0;
                return (float)Math.Round(result, 2);
            }
        }

        public string LastUpdated
        {
            get
            {
                return StockData.LastUpdate.ToString();
            }
        }

        public float Score
        {
            get
            {
                if (_rating == null)
                {
                    StockEvaluator stockEval = new StockEvaluator(StockData);
                    List<string> tooltipList = new List<string>();
                    _rating = stockEval.GrahamPoints(tooltipList);

                    ScoreTooltip = "";
                    int count = tooltipList.Count;
                    foreach (string posEvalResult in tooltipList)
                    {
                        ScoreTooltip += posEvalResult;
                        --count;
                        if (0 < count)
                        {
                            ScoreTooltip += Environment.NewLine;
                        }
                    }
                    
                    RaisePropertyChangedEvent("ScoreTooltip");
                }

                return _rating ?? 0f;
            }
        }

        public string ScoreTooltip
        {
            get;set;
        }

        public float Rank
        {
            get
            {
                return _rank;
            }
            set
            {
                _rank = value;
                RaisePropertyChangedEvent("Rank");
            }
        }

        public decimal OperatingIncome
        {
            get
            {
                decimal result = StockData?.Financials?.Last()?.OperatingIncome ?? 0;
                return Math.Round(result, 2);
            }
        }

        public decimal ReturnOnCapital
        {
            get
            {
                decimal result = (StockData?.Financials?.Last()?.ReturnOnCapital ?? 0);
                return Math.Round(result, 2);
            }
        }

        public decimal EnterpriseValue
        {
            get
            {
                decimal result = StockData?.Financials?.Last()?.EnterpriseValue ?? 0;
                return Math.Round(result, 2);
            }
        }

        public decimal EarningsYield
        {
            get
            {
                decimal result = (StockData?.Financials?.Last()?.EerningsYield ?? 0);
                return Math.Round(result, 2);
            }
        }

        public string CurrentTask
        {
            get
            {
                return _task;
            }
            set
            {
                _task = value;
                RaisePropertyChangedEvent("CurrentTask");
            }
        }

        public void Update()
        {
            try
            {
                CurrentTask = "Updating...";
                Updater updater = new Updater();
                updater.Update(StockData);
                _rating = null;
                // make sure all properties are updated
                RaisePropertyChangedEvent(null);
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
            finally
            {
                CurrentTask = "";
            }
        }

        public void Insert()
        {
            try
            {
                // write to db
                DataAccess.Db.AddStock(StockData);
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
            finally
            {
                CurrentTask = "";
            }
        }
    }
}
