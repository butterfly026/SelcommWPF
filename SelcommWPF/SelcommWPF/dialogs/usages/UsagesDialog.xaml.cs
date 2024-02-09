using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.bills;
using SelcommWPF.clients.models.messages;
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

namespace SelcommWPF.dialogs
{
    /// <summary>
    /// Interaction logic for UsagesDialog.xaml
    /// </summary>
    public partial class UsagesDialog : UserControl
    {

        private string ServiceReference = "";
        private bool IsService = false;

        public UsagesDialog(bool isService = false, string reference = "")
        {
            IsService = isService;
            ServiceReference = reference;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {

            GridView grid = (GridView)ListUsages.View;
            for (int i = 1; i < 4; i++)
            {
                GridViewColumnHeader header = (GridViewColumnHeader)grid.Columns[i].Header;
                header.Visibility = IsService ? Visibility.Collapsed : Visibility.Visible;
            }

            LabelDialogTitle.Content = IsService ? Properties.Resources.Service_Usages : Properties.Resources.Account_Usages;
            ShowUsageTransactions(0, 10);
        }

        private void ShowUsageTransactions(int skip, int take, bool isScroll = false)
        {
            if (skip == 0) ListUsages.ItemsSource = new List<BillTransModel.Item>();
            if (Global.ContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;
            string search = TextUsagesSearch.Text;
            string url = IsService ? Constants.ServiceUsagesURL : Constants.AccountUsagesURL;
            string code = IsService ? ServiceReference : Global.ContactCode;
            string from = DatePickerFrom.SelectedDate == null ? "" : DatePickerFrom.SelectedDate.Value.ToString("yyyy-MM-dd");
            string to = DatePickerTo.SelectedDate == null ? "" : DatePickerTo.SelectedDate.Value.ToString("yyyy-MM-dd");
            bool unInvoiced = CheckBoxUninvoiced.IsChecked.Value;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                if (from != "") query.Add("From", from);
                if (to != "") query.Add("To", to);
                query.Add("Uninvoiced", unInvoiced);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("countRecords", "Y");
                query.Add("SearchString", search);
                UsageModel result = Global.RequestAPI<UsageModel>(url, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary(IsService ? "ServiceReference" : "ContactCode", code), Global.BuildDictionary("api-version", 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    List<BillTransModel.Item> list = isScroll ? (List<BillTransModel.Item>)ListUsages.ItemsSource : new List<BillTransModel.Item>();
                    ListUsages.ItemsSource = new List<BillTransModel.Item>();

                    if (list == null) list = new List<BillTransModel.Item>();
                    int count = result == null ? 0 : result.Transactions.Count;
                    ListUsages.Tag = result.Count;

                    for (int i = 0; i < count; i++)
                    {
                        result.Transactions[i].PriceText = StringUtils.ConvertCurrency(result.Transactions[i].Price);
                        result.Transactions[i].TaxText = StringUtils.ConvertCurrency(result.Transactions[i].Tax);
                        result.Transactions[i].NonDiscountedTaxText = StringUtils.ConvertCurrency(result.Transactions[i].NonDiscountedTax);
                        result.Transactions[i].NonDiscountedPriceText = StringUtils.ConvertCurrency(result.Transactions[i].NonDiscountedPrice);
                        result.Transactions[i].CostText = StringUtils.ConvertCurrency(result.Transactions[i].Cost);
                        result.Transactions[i].CostTaxText = StringUtils.ConvertCurrency(result.Transactions[i].CostTax);

                        result.Transactions[i].StartDateTime = StringUtils.ConvertDateTime(result.Transactions[i].StartDateTime);

                        list.Add(result.Transactions[i]);
                    }

                    ListUsages.ItemsSource = list;
                });

            }, 10);
        }

        private void CheckBoxUninvoiced_Checked(object sender, RoutedEventArgs e)
        {
            ShowUsageTransactions(0, 10);
        }

        private void TextUsagesSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowUsageTransactions(0, 10);
        }

        private void ListUsages_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ListView listView = (ListView)e.Source;
            int totalCount = listView.Items.Count;
            int limit = Convert.ToInt32(listView.Tag);
            if (totalCount == 0 || totalCount >= limit) return;
            if (e.VerticalOffset + e.VerticalChange >= totalCount - 6)
            {
                ShowUsageTransactions(totalCount, 10, true);
            }
        }

        private void ListUsages_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonUsageServices.IsEnabled = ListUsages.SelectedItem != null;
            ButtonUsageDetail.IsEnabled = ListUsages.SelectedItem != null;
        }

        private void ButtonUsageServices_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonUsageDetail_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowUsageTransactions(0, 10);
        }
    }
}
