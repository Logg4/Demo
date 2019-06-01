using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using System.IO;

using StockWatch.Model;
using LogWriterLib;

namespace StockWatch.Logic
{
    public class MorningstarCsvToStock
    {
        private readonly string _keyRatioCSV;
        private readonly string _financialStatementCSV;
        private string _companyName;
        private List<Financials> _financialData;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="keyRatioCSV"></param>
        /// <param name="financialStatementCSV"></param>
        public MorningstarCsvToStock(string keyRatioCSV, string financialStatementCSV)
        {
            _keyRatioCSV = keyRatioCSV;
            _financialStatementCSV = financialStatementCSV;
        }

        /// <summary>
        /// 
        /// </summary>
        public string CompanyName
        {
            get
            {
                if (String.IsNullOrEmpty(_companyName))
                {
                    RawdataToFinancials();
                }
                return _companyName;
            }
            private set
            {
                if (value != null)
                {
                    _companyName = value.Trim();
                }
                
            }
        }

        /// <summary>
        /// Financials csv file pressed into objects
        /// </summary>
        public List<Financials> FinancialData
        {
            get
            {
                if (_financialData == null || _financialData.Count == 0)
                {
                    RawdataToFinancials();
                }
                return _financialData;
            }
            private set
            {
                _financialData = value;
            }
        }

        /// <summary>
        /// Parser entry point
        /// </summary>
        /// <returns></returns>
        private void RawdataToFinancials()
        {
            List<Financials> result = new List<Financials>();
            try
            {
                result = KeyRatioCsvToFinancials();
                FinancialsCsvToFinancials(result);
            }
            catch (Exception x)
            {
                Log.Error(x);
            }

            FinancialData = result;
        }

        private List<Financials> KeyRatioCsvToFinancials()
        {
            List<Financials> result = new List<Financials>();
            string companyName = "";
            try
            {
                // key ratios parser
                int rowCount = 1;
                StringReader stringReader = new StringReader(_keyRatioCSV);
                using (TextFieldParser parser = new TextFieldParser(stringReader))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    while (!parser.EndOfData)
                    {
                        //Processing row
                        List<string> fields = parser.ReadFields().ToList();

                        switch (rowCount)
                        {
                            case 1:
                                FillCompany(fields, ref companyName);
                                CompanyName = companyName;
                                break;
                            case 3:
                                FillFinancialsTemplate(fields, result);
                                break;
                            default:
                                FillFinancials(fields, result);
                                break;
                        }
                        rowCount++;
                    }
                }
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
            return result;
        }

        private void FillCompany(List<string> csvRowFields, ref string companyName)
        {
            string row = csvRowFields[0];
            try
            {
                if (row.Contains("We’re sorry. There is no available information in our database to display."))
                {
                    return;
                }
                companyName = row.Substring(row.LastIndexOf("Ratios for") + "Ratios for".Length);
            }
            catch (Exception x)
            {
                Console.WriteLine(x.Message);
            }


        }

        private void FillFinancialsTemplate(List<string> csvRowFields, List<Financials> financials)
        {
            if (financials == null)
            {
                financials = new List<Financials>();
            }
            foreach (string field in csvRowFields)
            {
                Financials financeEntry = new Financials();

                if (field == "TTM")
                {
                    financeEntry.Date = new DateTime();
                    financeEntry.IsTTM = true;
                }
                else if (DateTime.TryParse(field, out DateTime date))
                {
                    financeEntry.Date = date;
                }
                else
                {
                    // unknown field do not add
                    continue;
                }
                financials.Add(financeEntry);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="csvRowFields"></param>
        /// <param name="financials"></param>
        private void FillFinancials(List<string> csvRowFields, List<Financials> financials, bool keyRatioFile = true)
        {
            if (csvRowFields.Count < 1)
            {
                return;
            }

            string rowName = csvRowFields[0];
            List<string> rowSplitted = rowName.Split().ToList();

            if (3 == rowSplitted.Count && rowSplitted[0] == "Revenue")
            {
                string currency = rowSplitted[1];
                List<decimal> values = StringToDecimal(csvRowFields);
                for (int i = 0; i < values.Count; i++)
                {
                    if (i < financials.Count)
                    {
                        financials[i].Revenue = values[i] * 1000000;
                        financials[i].Currency = currency;
                    }
                }
            }

            if (rowSplitted[0] == "Net" && rowSplitted[1] == "Income" && rowSplitted[2] != "%")
            {
                List<decimal> values = StringToDecimal(csvRowFields);
                for (int i = 0; i < values.Count; i++)
                {
                    if (i < financials.Count)
                    {
                        financials[i].NetIncome = values[i] * 1000000;
                    }
                }
            }

            if (rowName.Contains("Earnings Per Share"))
            {
                List<decimal> values = StringToDecimal(csvRowFields);
                for (int i = 0; i < values.Count; i++)
                {
                    if (i < financials.Count)
                    {
                        financials[i].EarningsPerShare = values[i];
                    }
                }
            }

            if (rowName.Contains("Free Cash Flow EUR Mil"))
            {
                List<decimal> values = StringToDecimal(csvRowFields);
                for (int i = 0; i < values.Count; i++)
                {
                    if (i < financials.Count)
                    {
                        financials[i].FreeCashFlow = values[i] * 1000000;
                    }
                }
            }

            if (rowName.Contains("Free Cash Flow Per Share"))
            {
                List<decimal> values = StringToDecimal(csvRowFields);
                for (int i = 0; i < values.Count; i++)
                {
                    if (i < financials.Count)
                    {
                        financials[i].FreeCashFlowPerShare = values[i];
                    }
                }
            }

            if (rowName.Contains("Working Capital"))
            {
                List<decimal> values = StringToDecimal(csvRowFields);
                for (int i = 0; i < values.Count; i++)
                {
                    if (i < financials.Count)
                    {
                        financials[i].WorkingCapital = values[i] * 1000000;
                    }
                }
            }

            if (rowName.Contains("Dividends"))
            {
                List<decimal> values = StringToDecimal(csvRowFields);
                for (int i = 0; i < values.Count; i++)
                {
                    if (i < financials.Count)
                    {
                        financials[i].Dividends = values[i];
                    }
                }
            }

            if (rowName.Contains("Shares"))
            {
                List<decimal> values = StringToDecimal(csvRowFields);
                for (int i = 0; i < values.Count; i++)
                {
                    if (i < financials.Count)
                    {
                        financials[i].Shares = (long)values[i] * 1000000;
                    }
                }
            }

            if (rowName.Contains("Book Value Per Share"))
            {
                List<decimal> values = StringToDecimal(csvRowFields);
                for (int i = 0; i < values.Count; i++)
                {
                    if (i < financials.Count)
                    {
                        financials[i].BookValuePerShare = (long)values[i];
                    }
                }
            }

            if (rowName.Contains("Operating Income") && !rowName.Contains("Operating Income %"))
            {
                List<decimal> values = StringToDecimal(csvRowFields);
                for (int i = 0; i < values.Count; i++)
                {
                    if (i < financials.Count)
                    {
                        financials[i].OperatingIncome = values[i] * 1000000;
                    }
                }
            }

            if (rowName.Contains("Debt/Equity"))
            {
                List<decimal> values = StringToDecimal(csvRowFields);
                for (int i = 0; i < values.Count; i++)
                {
                    if (i < financials.Count)
                    {
                        financials[i].DebtToEquityRatio = (float)values[i];
                    }
                }
            }

            if (!keyRatioFile)
            {
                if (rowName.Contains("Total assets"))
                {
                    List<decimal> values = StringToDecimal(csvRowFields);

                    // start at the end to make sure we write into the right year
                    values.Reverse();
                    for (int i = 0; i < values.Count; i++)
                    {
                        if (i < financials.Count)
                        {
                            // 'Total assets'-line has no TTM, therefore -1
                            financials[financials.Count-1-i-1].TotalAssets = values[i] * 1000000;
                        }
                    }
                    if (1 < financials.Count)
                    {
                        // set ttm values to the last year
                        financials[financials.Count - 1].TotalAssets = financials[financials.Count - 2].TotalAssets;
                    }
                }

                if (rowName.Contains("Total liabilities"))
                {
                    List<decimal> values = StringToDecimal(csvRowFields);

                    // start at the end to make sure we write into the right year
                    values.Reverse();
                    for (int i = 0; i < values.Count; i++)
                    {
                        if (i < financials.Count)
                        {
                            // line has no TTM, therefore -1
                            financials[financials.Count - 1 - i - 1].TotalLiabilities = values[i] * 1000000;
                        }
                    }
                    if (1 < financials.Count)
                    {
                        // set ttm values to the last year
                        financials[financials.Count - 1].TotalLiabilities = financials[financials.Count - 2].TotalLiabilities;
                    }
                }

                if (rowName.Contains("Short-term debt"))
                {
                    List<decimal> values = StringToDecimal(csvRowFields);

                    // start at the end to make sure we write into the right year
                    values.Reverse();
                    for (int i = 0; i < values.Count; i++)
                    {
                        if (i < financials.Count)
                        {
                            // line has no TTM, therefore -1
                            financials[financials.Count - 1 - i - 1].ShortTermDebt = values[i] * 1000000;
                        }
                    }
                    if (1 < financials.Count)
                    {
                        // set ttm values to the last year
                        financials[financials.Count - 1].ShortTermDebt = financials[financials.Count - 2].ShortTermDebt;
                    }
                }

                if (rowName.Contains("Long-term debt"))
                {
                    List<decimal> values = StringToDecimal(csvRowFields);

                    // start at the end to make sure we write into the right year
                    values.Reverse();
                    for (int i = 0; i < values.Count; i++)
                    {
                        if (i < financials.Count)
                        {
                            // line has no TTM, therefore -1
                            financials[financials.Count - 1 - i - 1].LongTermDebt = values[i] * 1000000;
                        }
                    }
                    if (1 < financials.Count)
                    {
                        // set ttm values to the last year
                        financials[financials.Count - 1].LongTermDebt = financials[financials.Count - 2].LongTermDebt;
                    }
                }

                if (rowName.Contains("Total cash"))
                {
                    List<decimal> values = StringToDecimal(csvRowFields);

                    // start at the end to make sure we write into the right year
                    values.Reverse();
                    for (int i = 0; i < values.Count; i++)
                    {
                        if (i < financials.Count)
                        {
                            // line has no TTM, therefore -1
                            financials[financials.Count - 1 - i - 1].TotalCash = values[i] * 1000000;
                        }
                    }
                    if (1 < financials.Count)
                    {
                        // set ttm values to the last year
                        financials[financials.Count - 1].TotalCash = financials[financials.Count - 2].TotalCash;
                    }
                }

                if (rowName.Contains("Cash and cash equivalents"))
                {
                    List<decimal> values = StringToDecimal(csvRowFields);

                    // start at the end to make sure we write into the right year
                    values.Reverse();
                    for (int i = 0; i < values.Count; i++)
                    {
                        if (i < financials.Count)
                        {
                            // line has no TTM, therefore -1
                            financials[financials.Count - 1 - i - 1].CashAndCashEquivalents = values[i] * 1000000;
                        }
                    }
                    if (1 < financials.Count)
                    {
                        // set ttm values to the last year
                        financials[financials.Count - 1].CashAndCashEquivalents = financials[financials.Count - 2].CashAndCashEquivalents;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="financials"></param>
        private void FinancialsCsvToFinancials(List<Financials> financials)
        {
            try
            {
                StringReader stringReader = new StringReader(_financialStatementCSV);
                using (TextFieldParser parser = new TextFieldParser(stringReader))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    while (!parser.EndOfData)
                    {
                        List<string> fields = parser.ReadFields().ToList();
                        FillFinancials(fields, financials, false);
                    }
                }
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<decimal> StringToDecimal(List<string> list)
        {
            List<decimal> result = new List<decimal>();
            for (int i = 1; i < list.Count; i++)
            {
                if (decimal.TryParse(list[i], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out decimal value))
                {
                    result.Add(value);
                }
                else
                {
                    result.Add(0);
                }
            }
            return result;
        }
    }
}
