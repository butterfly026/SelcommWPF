using RestSharp;
using SelcommWPF.clients.models.bills;
using SelcommWPF.global;
using SelcommWPF.utils;
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
    /// Interaction logic for BillTermsDialog.xaml
    /// </summary>
    public partial class BillTermsDialog : UserControl
    {
        public BillTermsDialog()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                BillTerms result = Global.RequestAPI<BillTerms>(Constants.TermsAccountURL, Method.Get, Global.GetHeader(Global.CustomerToken),
                    Global.BuildDictionary("ContactCode", Global.ContactCode), Global.BuildDictionary("api-version", 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog("DetailDialog");
                        return;
                    }

                    List<BillTerms.History> list = result.TermsProfilesHistory;
                    int count = list.Count;
                    for (int i = 0; i < count; i++) list[i].CreditLimitText = StringUtils.ConvertCurrency(list[i].CreditLimit.Value);

                    ListBillCycles.ItemsSource = list;
                    ListBillCycles.SelectedIndex = 0;
                });

            }, 10);

        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            BillTerms.History item = (BillTerms.History)ListBillCycles.SelectedItem;
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("CreditLimit", item.CreditLimit);
            body.Add("TermsId", item.Id);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Global.RequestAPI(Constants.TermsAccountURL, Method.Put, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode),
                    Global.BuildDictionary("api-version", 1.0), body, false);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    Global.BillOptionDialog.LabelTerm.Content = Properties.Resources.Credit_Limit + " " + item.CreditLimitText + " - " + item.Name;
                    LoadingPanel.Visibility = Visibility.Hidden;
                    Global.CloseDialog("DetailDialog");
                });

            }, 10);

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("DetailDialog");
        }

    }
}
