using RestSharp;
using SelcommWPF.global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SelcommWPF.dialogs.bills.options
{
    /// <summary>
    /// Interaction logic for BillProofing.xaml
    /// </summary>
    public partial class BillProofing : UserControl
    {
        public BillProofing()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> result = Global.RequestAPI<Dictionary<string, object>>(Constants.BillProofingURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog("DetailDialog");
                        return;
                    }

                    TextProofingAccount.Text = result["Note"] == null ? "" : result["Note"].ToString();
                    ButtonCreate.IsEnabled = !string.IsNullOrEmpty(TextProofingAccount.Text);
                });

            }, 10);
        }

        private void ButtonCreate_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("Note", TextProofingAccount.Text);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Global.RequestAPI(Constants.BillProofingURL, Method.Put, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), 
                    Global.BuildDictionary("api-version", 1.0), body);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    Global.CloseDialog("DetailDialog");
                });

            }, 10);

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("DetailDialog");
        }

        private void TextProofingAccount_TextChanged(object sender, TextChangedEventArgs e)
        {
            ButtonCreate.IsEnabled = !string.IsNullOrEmpty(TextProofingAccount.Text);
        }
    }
}
