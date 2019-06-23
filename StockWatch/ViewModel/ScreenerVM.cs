using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading;
using System.Windows;
using System.IO;
using System.Reflection;

using LogWriterLib;
using StockWatch.Model;
using StockWatch.Logic;


namespace StockWatch.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public class ScreenerVM : BaseVM
    {
        private readonly ObservableCollection<StockDataVM> _stockData = new ObservableCollection<StockDataVM>();
        
        bool _isUpdating = false;

        /// <summary>
        /// 
        /// </summary>
        public ScreenerVM()
        {
            try
            {
                SetLogFile();
                Log.Info("Start up");
                foreach (Stock stock in DataAccess.Db.FullRead())
                {
                    _stockData.Add(new StockDataVM(stock));
                }
                IsUpdating = false;

                if (_stockData.Count == 0)
                {
                    CreateInitialStockList();
                    foreach (Stock stock in DataAccess.Db.FullRead())
                    {
                        _stockData.Add(new StockDataVM(stock));
                    }
                    UpdateAll();
                }
                UpdateRankings();
                _stockData.CollectionChanged += StockData_CollectionChanged;
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
        }

        private void SetLogFile()
        {
            try
            {
                string logFile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "StockWatch.log");
                Log.LogFile = logFile;
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
        }

        public bool IsUpdating
        {
            get
            {
                return _isUpdating;
            }
            set
            {
                _isUpdating = value;
                RaisePropertyChangedEvent("IsUpdating");
            }
        }

        public IEnumerable<StockDataVM> StockList
        {
            get
            {
                return _stockData;
            }
        }

        /// <summary>
        /// Relay update command to update function
        /// </summary>
        public ICommand UpdateCommand
        {
            get
            {
                return new RelayCommand<object>(x => Update(x));
            }
        }

        /// <summary>
        /// Relay command function
        /// </summary>
        public ICommand UpdateAllCommand
        {
            get
            {
                return new RelayCommand<object>(x => UpdateAll());
            }
        }

        /// <summary>
        /// Relay add command to add function
        /// </summary>
        public ICommand AddCommand
        {
            get
            {
                return new RelayCommand<object>(x => Add());
            }
        }

        /// <summary>
        /// Relay add command to add function
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                return new RelayCommand<object>(x => Delete(x));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand ChartsCommand
        {
            get
            {
                return new RelayCommand<object>(x => ShowChartWindow(x));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand NewDividendCommand
        {
            get
            {
                return new RelayCommand<object>(x => NewDividend());
            }
        }

        /// <summary>
        /// Chart window
        /// </summary>
        /// <param name="stockData"></param>
        private void ShowChartWindow(object stockData)
        {
            if (stockData is StockDataVM selectedData)
            {
                try
                {
                    ChartWindow chart = new ChartWindow(selectedData.StockData);
                    chart.Show();
                }
                catch (Exception x)
                {
                    Log.Error(x);
                }
            }
            else
            {
                try
                {
                    if (SelectedItem != null)
                    {
                        ChartWindow chart = new ChartWindow(SelectedItem.StockData);
                        chart.Show();
                    }
                }
                catch (Exception x)
                {
                    Log.Error(x);
                }
            }
        }

        /// <summary>
        /// Update stock
        /// </summary>
        private void Update(object stockData)
        {
            if (stockData is StockDataVM selectedData)
            {
                ThreadPool.QueueUserWorkItem(task =>
                {
                    selectedData.Update();
                    UpdateRankings();
                });
            }

            // context menu commands do not provide the selected item,
            // therefore we use our own SelectedItem
            if (SelectedItem != null)
            {
                ThreadPool.QueueUserWorkItem(task =>
                {
                    SelectedItem.Update();
                    UpdateRankings();
                });
            }
        }

        /// <summary>
        /// Change dividend (morningstars infos lag) 
        /// </summary>
        private void NewDividend()
        {
            AddDividendWindow addWin = new AddDividendWindow
            {
                Owner = Application.Current.MainWindow
            };

            bool? result = addWin.ShowDialog();
            if (result.HasValue && result == true)
            {
                // context menu commands do not provide the selected item,
                // therefore we use our own SelectedItem
                if (SelectedItem != null)
                {
                    SelectedItem.Dividend = addWin.GetDividend();
                    DataAccess.Db.UpdateStock(SelectedItem.StockData);
                }
            }
        }

        /// <summary>
        /// Update all stocks
        /// </summary>
        private void UpdateAll()
        {
            if (_stockData != null && 0 < _stockData.Count)
            {
                ThreadPool.QueueUserWorkItem(task =>
                  {
                      foreach (StockDataVM stockData in _stockData)
                      {
                          stockData.Update();
                      }
                  });
            }
            UpdateRankings();
        }

        /// <summary>
        /// Delete stock
        /// </summary>
        private void Delete(object stockData)
        {
            if (stockData is StockDataVM selectedData)
            {
                if (DataAccess.Db.RemoveStock(selectedData.StockData))
                {
                    _stockData.Remove(selectedData);
                }
            }
            // context menu commands do not provide the selected item,
            // therefore we use our own SelectedItem
            if (SelectedItem != null)
            {
                if (DataAccess.Db.RemoveStock(SelectedItem.StockData))
                {
                    _stockData.Remove(SelectedItem);
                }
            }
        }

        /// <summary>
        /// Add new stock (updated in background)
        /// </summary>
        private void Add()
        {
            try
            {
                AddTickerWindow addWin = new AddTickerWindow
                {
                    Owner = Application.Current.MainWindow
                };

                bool? result = addWin.ShowDialog();
                if (result.HasValue && result == true)
                {
                    // make sure ticker is unknown
                    string ticker = addWin.GetTicker();
                    if (_stockData.FirstOrDefault(x => x.StockData != null && 
                            x.StockData.TickerM.Equals(ticker, StringComparison.OrdinalIgnoreCase)) != null)
                    {
                        return;
                    }

                    Stock newStock = new Stock()
                    {
                        TickerM = ticker
                    };

                    if (newStock != null)
                    {
                        StockDataVM newData = new StockDataVM(newStock);
                        newData.Insert();
                        _stockData.Add(newData);
                        ThreadPool.QueueUserWorkItem(task =>
                        {
                            newData.Update();
                            UpdateRankings();
                        });
                    }
                }
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                _selectedIndex = value;
                RaisePropertyChangedEvent("SelectedIndex");
            }
        }

        private StockDataVM _currentItem;
        public StockDataVM CurrentItem
        {
            get
            {
                return _currentItem;
            }
            set
            {
                _currentItem = value;
                RaisePropertyChangedEvent("CurrentItem");
            }
        }

        private StockDataVM _selectedItem;
        public StockDataVM SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void CreateInitialStockList()
        {
            try
            {
                List<string> daxTickers = new List<string>() {"ADS", "ALV", "BAS", "BAYN", "BEI", "BMW", "CON", "1COV", "DAI", "DBK",
                                                                "DB1", "LHA", "DPW", "DTE", "EOAN", "FRE", "FME","HEI", "HEN3", "IFX",
                                                                "LIN", "MRK", "MUV2", "RWE", "SAP", "SIE", "TKA", "VOW3", "VNA", "WDI"};
                List<Stock> stocks = new List<Stock>();
                foreach (string ticker in daxTickers)
                {
                    stocks.Add(new Stock() { TickerM = ticker });
                }

                DataAccess.Db.AddStocks(stocks);
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
        }

        /// <summary>
        /// If stock collection changes we must recalculate the ranking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StockData_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateRankings();
        }

        private void UpdateRankings()
        {
            StocksRanking stockRanking = new StocksRanking();
            stockRanking.SetRanks(_stockData.ToList());
        }
    }
}
