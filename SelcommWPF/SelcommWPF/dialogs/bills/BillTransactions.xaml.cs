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

namespace SelcommWPF.dialogs.bills
{
    /// <summary>
    /// Interaction logic for BillTransactions.xaml
    /// </summary>
    public partial class BillTransactions : UserControl
    {

        public long BillId;
        public string BillNumber;

        public BillTransactions(long id, string number)
        {
            BillId = id;
            BillNumber = number;
            InitializeComponent();
            LabelDialogTitle.Content = string.Format(Properties.Resources.Bill_Trans_Title, BillNumber);
            ShowBillTransactions(0, 10);
        }

        private void ShowBillTransactions(int skip, int take, bool isScroll = false)
        {
            if (skip == 0) ListBillTransaction.ItemsSource = new List<BillTransModel.Item>();
            if (Global.ContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;
            string search = TextTransactionSearch.Text;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("CountRecords", "Y");
                query.Add("SearchString", search);
                BillTransModel result = Global.RequestAPI<BillTransModel>(Constants.BillTransURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("BillId", BillId), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    List<BillTransModel.Item> list = isScroll ? (List<BillTransModel.Item>)ListBillTransaction.ItemsSource : new List<BillTransModel.Item>();
                    ListBillTransaction.ItemsSource = new List<BillTransModel.Item>();

                    if (list == null) list = new List<BillTransModel.Item>();
                    int count = result == null ? 0 : result.Items.Count;
                    ListBillTransaction.Tag = result.Count;

                    for (int i = 0; i < count; i++)
                    {
                        result.Items[i].PriceText = StringUtils.ConvertCurrency(result.Items[i].Price);
                        result.Items[i].TaxText = StringUtils.ConvertCurrency(result.Items[i].Tax);
                        result.Items[i].NonDiscountedTaxText = StringUtils.ConvertCurrency(result.Items[i].NonDiscountedTax);
                        result.Items[i].NonDiscountedPriceText = StringUtils.ConvertCurrency(result.Items[i].NonDiscountedPrice);
                        result.Items[i].CostText = StringUtils.ConvertCurrency(result.Items[i].Cost);
                        result.Items[i].CostTaxText = StringUtils.ConvertCurrency(result.Items[i].CostTax);
                        result.Items[i].StartDateTime = StringUtils.ConvertDateTime(result.Items[i].StartDateTime);
                        list.Add(result.Items[i]);
                    }

                    ListBillTransaction.ItemsSource = list;
                    ShowHideVerticalScroll(Global.HasVerticalScroll);
                });

            }, 10);
        }

        private void ShowHideVerticalScroll(bool isChecked)
        {
            try
            {
                ScrollViewer sv = VisualTreeHelper.GetChild(ListBillTransaction, 0) as ScrollViewer;
                sv.VerticalScrollBarVisibility = isChecked ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
            }
            catch{}
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void TextTransactionSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowBillTransactions(0, 10);
        }

        private void ListBillTransaction_Scroll(object sender, ScrollChangedEventArgs e)
        {
            ListView listView = (ListView)e.Source;
            int totalCount = listView.Items.Count;
            int limit = Convert.ToInt32(listView.Tag);
            if (totalCount == 0 || totalCount >= limit) return;
            if (e.VerticalOffset + e.VerticalChange >= totalCount - 6)
            {
                ShowBillTransactions(totalCount, 10, true);
            }
        }
    }
}
