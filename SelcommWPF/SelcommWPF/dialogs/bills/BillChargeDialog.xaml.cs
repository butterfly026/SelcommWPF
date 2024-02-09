using RestSharp;
using SelcommWPF.clients.models;
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

namespace SelcommWPF.dialogs.bills
{
    /// <summary>
    /// Interaction logic for BillChargeDialog.xaml
    /// </summary>
    public partial class BillChargeDialog : UserControl
    {

        public long BillId;
        public string BillNumber;

        public BillChargeDialog(long id, string number)
        {
            BillId = id;
            BillNumber = number;
            InitializeComponent();
            LabelDialogTitle.Content = string.Format(Properties.Resources.Bill_Title, BillNumber);
            ShowBillCharges(0, 10);
        }

        private void ShowBillCharges(int skip, int take, bool isScroll = false)
        {
            if (skip == 0) ListBillCharges.ItemsSource = new List<AccountCharge.History>();
            if (Global.ContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;
            string search = TextChargeSearch.Text;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("CountRecords", "Y");
                query.Add("SearchString", search);
                AccountCharge result = Global.RequestAPI<AccountCharge>(Constants.BillChargesURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("BillId", BillId), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    List<AccountCharge.History> list = isScroll ? (List<AccountCharge.History>)ListBillCharges.ItemsSource : new List<AccountCharge.History>();
                    ListBillCharges.ItemsSource = new List<AccountCharge.History>();

                    if (list == null) list = new List<AccountCharge.History>();
                    int count = result == null ? 0 : result.Items.Count;
                    ListBillCharges.Tag = result.Count;

                    for (int i = 0; i < count; i++)
                    {
                        result.Items[i].PriceTaxExText = StringUtils.ConvertCurrency(result.Items[i].PriceTaxEx);
                        result.Items[i].PriceTaxIncText = StringUtils.ConvertCurrency(result.Items[i].PriceTaxInc);
                        result.Items[i].UndiscountedPriceTaxExText = StringUtils.ConvertCurrency(result.Items[i].UndiscountedPriceTaxEx);
                        result.Items[i].UndiscountedPriceTaxIncText = StringUtils.ConvertCurrency(result.Items[i].UndiscountedPriceTaxInc);
                        result.Items[i].CostText = StringUtils.ConvertCurrency(result.Items[i].Cost.Value);

                        result.Items[i].From = StringUtils.ConvertDateTime(result.Items[i].From);
                        result.Items[i].To = StringUtils.ConvertDateTime(result.Items[i].To);

                        list.Add(result.Items[i]);
                    }

                    ListBillCharges.ItemsSource = list;
                });

            }, 10);
        }


        private void TextChargeSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowBillCharges(0, 10);
        }

        private void ListBillCharges_Scroll(object sender, ScrollChangedEventArgs e)
        {
            ListView listView = (ListView)e.Source;
            int totalCount = listView.Items.Count;
            int limit = Convert.ToInt32(listView.Tag);
            if (totalCount == 0 || totalCount >= limit) return;
            if (e.VerticalOffset + e.VerticalChange >= totalCount - 6)
            {
                ShowBillCharges(totalCount, 10, true);
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }
    }
}
