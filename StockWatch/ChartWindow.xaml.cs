using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Input;

using LogWriterLib;
using StockWatch.Model;
using StockWatch.ViewModel;

namespace StockWatch
{
    /// <summary>
    /// 
    /// </summary>
    public partial class ChartWindow : Window
    {
        readonly Stock _stockData;

        /// <summary>
        /// Interaction logic for ChartWindow.xaml
        /// </summary>
        /// <param name="stockData"></param>
        public ChartWindow(Stock stockData)
        {
            try
            {
                InitializeComponent();
                _stockData = stockData;
                Title = _stockData.Company;

                Logic.StockInfo stockInfo = new Logic.StockInfo(_stockData);
                // intrinsic valuation initialization & calculation
                TabIntrinsicValue.DataContext = new IntrinsicOverviewVM(stockInfo);
                GBDivDiscModel.DataContext = new DivDiscModelVM(stockInfo);
                GBMpDivDiscModel.DataContext = new MpDivDiscModelVM(stockInfo);
                GBCashFlowDiscModelVM.DataContext = new CashFlowDiscModelVM(stockInfo);
                GBResidualIncomeModelVM.DataContext = new ResidualIncomeModelVM(stockInfo);

                // chart data composition
                Compose();
            }
            catch (Exception x)
            {
                Log.Error(x);
            }

        }

        private void Compose()
        {
            ComposePriceAndBookValue();
            ComposePerShareData();
            ComposePriceToEarningsData();
            ComposeRevenueAndNetIncomeData();
            ComposeRawFinancials();
            ComposeEvData();
            ComposeYieldData();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ComposePriceAndBookValue()
        {
            try
            {
                List<Dictionary<DateTime, float>> printMe = new List<Dictionary<DateTime, float>>();
                Dictionary<DateTime, float> quotes = new Dictionary<DateTime, float>();
                Dictionary<DateTime, float> bookValue = new Dictionary<DateTime, float>();
                DateTime startDate = DateTime.Now;
                float maximum = 0;
                float min = 100;

                foreach (Financials finance in _stockData.Financials)
                {
                    if (finance.IsTTM)
                    {
                        bookValue[DateTime.Now] = (float)finance.BookValuePerShare;
                    }
                    else
                    {
                        bookValue[finance.Date] = (float)finance.BookValuePerShare;
                        if (finance.Date < startDate)
                        {
                            startDate = finance.Date;
                        }
                    }

                    if (maximum < (float)finance.BookValuePerShare)
                    {
                        maximum = (float)finance.BookValuePerShare;
                    }
                    if ((float)finance.BookValuePerShare < min)
                    {
                        min = (float)finance.BookValuePerShare;
                    }
                }

                if (_stockData.WeeklyQuotes == null)
                {
                    return;
                }
                foreach (Quote quote in _stockData.WeeklyQuotes)
                {
                    if (startDate < quote.Date)
                    {
                        quotes[quote.Date] = quote.Close;
                        if (maximum < quote.Close)
                        {
                            maximum = quote.Close;
                        }
                        if (quote.Close < min)
                        {
                            min = quote.Close;
                        }
                    }
                }

                printMe.Add(quotes);
                printMe.Add(bookValue);

                ChartSharePrice.DataContext = printMe;
                LinearAxis linearAxis = (LinearAxis)ChartSharePrice.Axes[0];

                double correction = maximum * 0.1;
                linearAxis.Maximum = maximum + correction;
                linearAxis.Minimum = min - correction;

                LegendItem legend = (LegendItem)ChartSharePrice.LegendItems[0];
                legend.Content = "Share price";

                legend = (LegendItem)ChartSharePrice.LegendItems[1];
                legend.Content = "Book val. p.share";
                var style = new Style();
                style.TargetType = typeof(LineDataPoint);
                style.Setters.Add(new Setter(BackgroundProperty, Brushes.LightBlue));
                LineSeriesBookValue.DataPointStyle = style;
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ComposePerShareData()
        {
            try
            {
                List<Dictionary<DateTime, decimal>> printMe = new List<Dictionary<DateTime, decimal>>();
                Dictionary<DateTime, decimal> earningsPerShare = new Dictionary<DateTime, decimal>();
                Dictionary<DateTime, decimal> freeCashFlowPerShare = new Dictionary<DateTime, decimal>();
                Dictionary<DateTime, decimal> dividendsPerShare = new Dictionary<DateTime, decimal>();

                foreach (Financials finance in _stockData.Financials)
                {
                    if (finance.IsTTM)
                    {
                        earningsPerShare[DateTime.Now] = finance.EarningsPerShare;
                        if (!(0 < dividendsPerShare.Count && dividendsPerShare.Last().Value == finance.Dividends))
                        {
                            dividendsPerShare[DateTime.Now] = finance.Dividends;
                        }
                    }
                    else
                    {
                        earningsPerShare[finance.Date] = finance.EarningsPerShare;
                        freeCashFlowPerShare[finance.Date] = finance.FreeCashFlowPerShare;
                        dividendsPerShare[finance.Date] = finance.Dividends;
                    }
                }
                printMe.Add(earningsPerShare);
                printMe.Add(freeCashFlowPerShare);
                printMe.Add(dividendsPerShare);

                ChartPerShare.DataContext = printMe;

                LegendItem legend = (LegendItem)ChartPerShare.LegendItems[0];
                legend.Content = "Earnings per share";
                var style = new Style
                {
                    TargetType = typeof(LineDataPoint)
                };
                style.Setters.Add(new Setter(BackgroundProperty, Brushes.GreenYellow));
                CharLineEarningsPerShare.DataPointStyle = style;

                legend = (LegendItem)ChartPerShare.LegendItems[1];
                legend.Content = "Free cashflow p.share";
                style = new Style
                {
                    TargetType = typeof(LineDataPoint)
                };
                style.Setters.Add(new Setter(BackgroundProperty, Brushes.CornflowerBlue));
                CharLineFreeCashFlowPerShare.DataPointStyle = style;

                legend = (LegendItem)ChartPerShare.LegendItems[2];
                legend.Content = "Dividends p.share";
                style = new Style
                {
                    TargetType = typeof(LineDataPoint)
                };
                style.Setters.Add(new Setter(BackgroundProperty, Brushes.Green));
                CharLineDividendsPerShare.DataPointStyle = style;

            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
                Log.Error(x);
            }
        }

        /// <summary>
        /// PriceToEarnings =  (decimal)SharePrice / EarningsPerShare;
        /// </summary>
        private void ComposePriceToEarningsData()
        {
            try
            {
                List<Dictionary<DateTime, decimal>> printMe = new List<Dictionary<DateTime, decimal>>();
                Dictionary<DateTime, decimal> peRatio = new Dictionary<DateTime, decimal>();
                Dictionary<DateTime, decimal> pbRatio = new Dictionary<DateTime, decimal>();

                foreach (Financials finance in _stockData.Financials)
                {
                    if (finance.IsTTM)
                    {
                        peRatio[DateTime.Now] = finance.PriceToEarningsRatio;
                        pbRatio[DateTime.Now] = finance.PriceToBookRatio;
                    }
                    else
                    {
                        peRatio[finance.Date] = finance.PriceToEarningsRatio;
                        pbRatio[finance.Date] = finance.PriceToBookRatio;
                    }
                }

                printMe.Add(peRatio);
                printMe.Add(pbRatio);

                ChartPriceRatios.DataContext = printMe;

                LegendItem legend = (LegendItem)ChartPriceRatios.LegendItems[0];
                legend.Content = "Price to earnings";
                var style = new Style
                {
                    TargetType = typeof(LineDataPoint)
                };
                style.Setters.Add(new Setter(BackgroundProperty, Brushes.GreenYellow));
                CharLinePriceToEarnings.DataPointStyle = style;

                legend = (LegendItem)ChartPriceRatios.LegendItems[1];
                legend.Content = "Price to book value";
                style = new Style
                {
                    TargetType = typeof(LineDataPoint)
                };
                style.Setters.Add(new Setter(BackgroundProperty, Brushes.LightBlue));
                CharLinePriceToBookValue.DataPointStyle = style;
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
                Log.Error(x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ComposeRevenueAndNetIncomeData()
        {
            try
            {
                List<Dictionary<DateTime, decimal>> printMe = new List<Dictionary<DateTime, decimal>>();
                Dictionary<DateTime, decimal> revenue = new Dictionary<DateTime, decimal>();
                Dictionary<DateTime, decimal> netIncome = new Dictionary<DateTime, decimal>();
                Dictionary<DateTime, decimal> freeCashflow = new Dictionary<DateTime, decimal>();

                foreach (Financials finance in _stockData.Financials)
                {
                    //  (decimal)SharePrice / EarningsPerShare;
                    if (finance.IsTTM)
                    {
                        revenue[DateTime.Now] = finance.Revenue;
                        netIncome[DateTime.Now] = finance.NetIncome;
                        freeCashflow[DateTime.Now] = finance.FreeCashFlow;
                    }
                    else
                    {
                        revenue[finance.Date] = finance.Revenue;
                        netIncome[finance.Date] = finance.NetIncome;
                        freeCashflow[finance.Date] = finance.FreeCashFlow;
                    }
                }
                printMe.Add(revenue);
                printMe.Add(netIncome);
                printMe.Add(freeCashflow);

                ChartRevenue.DataContext = printMe;

                LegendItem legend = (LegendItem)ChartRevenue.LegendItems[0];
                legend.Content = "Revenue";
                var style = new Style
                {
                    TargetType = typeof(LineDataPoint)
                };
                style.Setters.Add(new Setter(BackgroundProperty, Brushes.DeepSkyBlue));
                CharLineRevenue.DataPointStyle = style;


                legend = (LegendItem)ChartRevenue.LegendItems[1];
                legend.Content = "Net income";
                style = new Style
                {
                    TargetType = typeof(LineDataPoint)
                };
                style.Setters.Add(new Setter(BackgroundProperty, Brushes.LightGreen));
                CharLineNetIncome.DataPointStyle = style;

                legend = (LegendItem)ChartRevenue.LegendItems[2];
                legend.Content = "Free cashflow";
                style = new Style
                {
                    TargetType = typeof(LineDataPoint)
                };
                style.Setters.Add(new Setter(BackgroundProperty, Brushes.CornflowerBlue));
                CharLineFreeCashFlow.DataPointStyle = style;
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
                Log.Error(x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ComposeRawFinancials()
        {
            try
            {
                if (_stockData.KeyRatiosRawCSV == null || _stockData.FinancialsRawCSV == null)
                {
                    return;
                }
                richTxtBoxFinancials.DataContext = null;
                richTxtBoxFinancials.AppendText(_stockData.KeyRatiosRawCSV.Last());
                richTxtBoxFinancials.AppendText(Environment.NewLine + 
                                                Environment.NewLine + "-------- END of Key Ratios" + 
                                                Environment.NewLine + 
                                                Environment.NewLine );
                richTxtBoxFinancials.AppendText(_stockData.FinancialsRawCSV.Last());
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
                Log.Error(x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ComposeEvData()
        {
            try
            {
                List<Dictionary<DateTime, decimal>> printMe = new List<Dictionary<DateTime, decimal>>();
                Dictionary<DateTime, decimal> ev = new Dictionary<DateTime, decimal>();
                Dictionary<DateTime, decimal> mc = new Dictionary<DateTime, decimal>();
                Dictionary<DateTime, decimal> debt = new Dictionary<DateTime, decimal>();
                Dictionary<DateTime, decimal> cash = new Dictionary<DateTime, decimal>();
                Dictionary<DateTime, decimal> capital = new Dictionary<DateTime, decimal>();

                foreach (Financials finance in _stockData.Financials)
                {
                    //  (decimal)SharePrice / EarningsPerShare;
                    if (finance.IsTTM)
                    {
                        ev[DateTime.Now] = finance.EnterpriseValue;
                        mc[DateTime.Now] = finance.SharePrice * finance.Shares;
                        debt[DateTime.Now] = finance.LongTermDebt + finance.ShortTermDebt;
                        cash[DateTime.Now] = finance.CashAndCashEquivalents;
                        capital[DateTime.Now] = finance.TotalAssets - finance.CurrentLiabilities;
                    }
                    else
                    {
                        ev[finance.Date] = finance.EnterpriseValue;
                        mc[finance.Date] = finance.SharePrice * finance.Shares;
                        debt[finance.Date] = finance.LongTermDebt + finance.ShortTermDebt;
                        cash[finance.Date] = finance.CashAndCashEquivalents;
                        capital[finance.Date] = finance.TotalAssets - finance.CurrentLiabilities;
                    }
                }
                printMe.Add(ev);
                printMe.Add(capital);
                printMe.Add(mc);
                printMe.Add(debt);
                printMe.Add(cash);
                

                ChartEnterpriseValue.DataContext = printMe;

                LegendItem legend = (LegendItem)ChartEnterpriseValue.LegendItems[0];
                legend.Content = "Enterprise Value";
                var style = new Style();
                style.TargetType = typeof(LineDataPoint);
                style.Setters.Add(new Setter(BackgroundProperty, Brushes.Blue));
                CharLineEv.DataPointStyle = style;


                legend = (LegendItem)ChartEnterpriseValue.LegendItems[1];
                legend.Content = "Capital";
                style = new Style();
                style.TargetType = typeof(LineDataPoint);
                style.Setters.Add(new Setter(BackgroundProperty, Brushes.Black));
                CharLineCapital.DataPointStyle = style;

                legend = (LegendItem)ChartEnterpriseValue.LegendItems[2];
                legend.Content = "Market Cap.";
                style = new Style();
                style.TargetType = typeof(LineDataPoint);
                style.Setters.Add(new Setter(BackgroundProperty, Brushes.BlueViolet));
                CharLineMc.DataPointStyle = style;

                legend = (LegendItem)ChartEnterpriseValue.LegendItems[3];
                legend.Content = "Debt";
                style = new Style();
                style.TargetType = typeof(LineDataPoint);
                style.Setters.Add(new Setter(BackgroundProperty, Brushes.Red));
                CharLineDebt.DataPointStyle = style;

                legend = (LegendItem)ChartEnterpriseValue.LegendItems[4];
                legend.Content = "Cash & Cash eq.";
                style = new Style();
                style.TargetType = typeof(LineDataPoint);
                style.Setters.Add(new Setter(BackgroundProperty, Brushes.Yellow));
                CharLineCash.DataPointStyle = style;
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
                Log.Error(x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ComposeYieldData()
        {
            try
            {
                List<Dictionary<DateTime, decimal>> printMe = new List<Dictionary<DateTime, decimal>>();
                Dictionary<DateTime, decimal> returnOnCapital = new Dictionary<DateTime, decimal>();
                Dictionary<DateTime, decimal> dividendYield = new Dictionary<DateTime, decimal>();
                Dictionary<DateTime, decimal> returnOnEV = new Dictionary<DateTime, decimal>();
                Dictionary<DateTime, decimal> earningsToPrice = new Dictionary<DateTime, decimal>();

                foreach (Financials finance in _stockData.Financials)
                {
                    if (finance.IsTTM)
                    {
                        returnOnCapital[DateTime.Now] = finance.ReturnOnCapital * 100;
                        returnOnEV[DateTime.Now] = finance.EerningsYield * 100;
                        dividendYield[DateTime.Now] = (decimal)finance.DividendYield * 100;
                        earningsToPrice[DateTime.Now] = (finance.EarningsPerShare / finance.SharePrice) * 100;
                    }
                    else
                    {
                        returnOnCapital[finance.Date] = finance.ReturnOnCapital * 100;
                        returnOnEV[finance.Date] = finance.EerningsYield * 100;
                        dividendYield[finance.Date] = (decimal)finance.DividendYield * 100;
                        earningsToPrice[finance.Date] = (finance.EarningsPerShare / finance.SharePrice) * 100;
                    }
                }
                
                printMe.Add(returnOnEV);
                printMe.Add(returnOnCapital);
                printMe.Add(earningsToPrice);
                printMe.Add(dividendYield);
                

                ChartYields.DataContext = printMe;

                LegendItem legend = (LegendItem)ChartYields.LegendItems[0];
                legend.Content = "Ret. on Enterpr.V. %";
                var style = new Style();
                style.TargetType = typeof(LineDataPoint);
                style.Setters.Add(new Setter(BackgroundProperty, Brushes.Blue));
                CharLineReturnOnEV.DataPointStyle = style;

                legend = (LegendItem)ChartYields.LegendItems[1];
                legend.Content = "Ret. on Capital %";
                style = new Style();
                style.TargetType = typeof(LineDataPoint);
                style.Setters.Add(new Setter(BackgroundProperty, Brushes.Black));
                CharLineReturnOnCapital.DataPointStyle = style;

                legend = (LegendItem)ChartYields.LegendItems[2];
                legend.Content = "Earnings yield p.share %";
                style = new Style();
                style.TargetType = typeof(LineDataPoint);
                style.Setters.Add(new Setter(BackgroundProperty, Brushes.GreenYellow));
                CharLineEarningsToPrice.DataPointStyle = style;

                legend = (LegendItem)ChartYields.LegendItems[3];
                legend.Content = "Dividend yield %";
                style = new Style();
                style.TargetType = typeof(LineDataPoint);
                style.Setters.Add(new Setter(BackgroundProperty, Brushes.Green));
                CharLineDividendsYield.DataPointStyle = style;
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
                Log.Error(x);
            }
        }

        /// <summary>
        /// Mouse wheel changes Y-axis scaling
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Chart_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            try
            {
                Chart chart = (Chart)sender;

                LinearAxis ax = (LinearAxis)chart.Axes[0];
                double currentMin = ax.ActualMinimum ?? 0;
                double currentMax = ax.ActualMaximum ?? 0;
                double diffDistance = currentMax - currentMin;
                if (diffDistance == 0)
                {
                    diffDistance = 1;
                }

                double moveDelta = 0;
                if (0 < e.Delta)
                {
                    moveDelta = diffDistance / 20;
                }
                else
                {
                    moveDelta = -diffDistance / 20;
                }

                if (currentMin < currentMax)
                {
                    if (Keyboard.Modifiers != ModifierKeys.Control)
                    {
                        ax.Maximum = currentMax + moveDelta;
                    }
                    else
                    {
                        ax.Minimum = currentMin + moveDelta;
                    }
                }
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
        }
    }
}
