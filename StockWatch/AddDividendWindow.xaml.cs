using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using LogWriterLib;

namespace StockWatch
{
    /// <summary>
    /// Interaction logic for AddDividendWindow.xaml
    /// </summary>
    public partial class AddDividendWindow : Window
    {
        public AddDividendWindow()
        {
            InitializeComponent();
            TxtDividend.Focus();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public decimal GetDividend()
        {
            try
            {
                if (TxtDividend != null && !String.IsNullOrWhiteSpace(TxtDividend.Text))
                {
                    Decimal.TryParse(TxtDividend.Text, out decimal result);
                    return result;
                }
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
            return 0;
        }
    }
}
