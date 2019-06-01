using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using System.Windows.Navigation;

using LogWriterLib;

namespace StockWatch
{
    /// <summary>
    /// Interaction logic for AddTickerWindow.xaml
    /// </summary>
    public partial class AddTickerWindow : Window
    {
        public AddTickerWindow()
        {
            InitializeComponent();
            TxtTicker.Focus();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public string GetTicker()
        {
            if (TxtTicker != null && !String.IsNullOrWhiteSpace(TxtTicker.Text))
            {
                return TxtTicker.Text.Trim();
            }
            return "";
        }

        private void HyperlinkSearch_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            }
            catch (Exception x)
            {
                Log.Error(x);
            }
        }

    }
}
