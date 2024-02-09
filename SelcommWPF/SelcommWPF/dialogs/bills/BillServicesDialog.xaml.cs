using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.bills;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections;
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
    /// Interaction logic for BillServicesDialog.xaml
    /// </summary>
    public partial class BillServicesDialog : UserControl
    {
        public long BillId;
        public string BillNumber;

        public BillServicesDialog(long id, string number)
        {
            BillId = id;
            BillNumber = number;
            InitializeComponent();
            LabelDialogTitle.Content = string.Format(Properties.Resources.Bill_Service_Title, BillNumber);
            ShowBillServicesSummaries(0, 10);
        }

        private void ShowBillServicesSummaries(int skip, int take, bool isScroll = false)
        {
            if (skip == 0) ListBillServices.ItemsSource = new List<BillServices.Item>();
            if (Global.ContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;
            string search = TextServiceSearch.Text;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("CountRecords", "Y");
                query.Add("SearchString", search);
                BillServices result = Global.RequestAPI<BillServices>(Constants.BillServicesURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("BillId", BillId), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    List<BillServices.Item> list = isScroll ? (List<BillServices.Item>)ListBillServices.ItemsSource : new List<BillServices.Item>();
                    ListBillServices.ItemsSource = new List<BillServices.Item>();

                    if (list == null) list = new List<BillServices.Item>();
                    int count = result == null ? 0 : result.ServiceSummaries.Count;
                    ListBillServices.Tag = result.Count;

                    for (int i = 0; i < count; i++)
                    {
                        result.ServiceSummaries[i].ChargeAmountText = StringUtils.ConvertCurrency(result.ServiceSummaries[i].ChargeAmount);
                        result.ServiceSummaries[i].ChargeAmountIncText = StringUtils.ConvertCurrency(result.ServiceSummaries[i].ChargeAmountInc);
                        result.ServiceSummaries[i].UsageAmountText = StringUtils.ConvertCurrency(result.ServiceSummaries[i].UsageAmount);
                        result.ServiceSummaries[i].UsageAmountIncText = StringUtils.ConvertCurrency(result.ServiceSummaries[i].UsageAmountInc);

                        list.Add(result.ServiceSummaries[i]);
                    }
                    
                    ListBillServices.ItemsSource = list;
                });

            }, 10);
        }

        private void TextServiceSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowBillServicesSummaries(0, 10);
        }

        private void ListBillServices_Scroll(object sender, ScrollChangedEventArgs e)
        {
            ListView listView = (ListView)e.Source;
            int totalCount = listView.Items.Count;
            int limit = Convert.ToInt32(listView.Tag);
            if (totalCount == 0 || totalCount >= limit) return;
            if (e.VerticalOffset + e.VerticalChange >= totalCount - 6)
            {
                ShowBillServicesSummaries(totalCount, 10, true);
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

    }
}
