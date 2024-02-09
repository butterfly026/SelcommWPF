using MaterialDesignColors;
using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.bills;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// Interaction logic for BillDisputeDialog.xaml
    /// </summary>
    public partial class BillDisputeDialog : UserControl
    {

        public long BillId;
        public string BillNumber;

        public BillDisputeDialog(long id, string number)
        {
            BillId = id;
            BillNumber = number;
            InitializeComponent();
            LabelDialogTitle.Content = string.Format("Bill Disputes [{0}]", BillNumber);
            ShowBillDisputes(0, 10);
        }

        private void ShowBillDisputes(int skip, int take)
        {
            if (skip == 0) ListBillDisputes.ItemsSource = new List<BillDisputeModel.Item>();
            if (Global.ContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("CountRecords", "Y");
                BillDisputeModel result = Global.RequestAPI<BillDisputeModel>(Constants.BillDisputesListURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("BillId", BillId), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    List<BillDisputeModel.Item> list = (List<BillDisputeModel.Item>)ListBillDisputes.ItemsSource;
                    ListBillDisputes.ItemsSource = new List<BillDisputeModel.Item>();

                    if (list == null) list = new List<BillDisputeModel.Item>();
                    int count = result == null ? 0 : result.BillDisputes.Count;
                    ListBillDisputes.Tag = result.Count;

                    for (int i = 0; i < count; i++)
                    {
                        result.BillDisputes[i].DisputedAmountText = StringUtils.ConvertCurrency(result.BillDisputes[i].DisputedAmount.Value);
                        result.BillDisputes[i].SettlementAmountText = StringUtils.ConvertCurrency(result.BillDisputes[i].SettlementAmount ?? 0);
                        result.BillDisputes[i].SettlementTaxText = StringUtils.ConvertCurrency(result.BillDisputes[i].SettlementTax ?? 0);

                        result.BillDisputes[i].Date = StringUtils.ConvertDateTime(result.BillDisputes[i].Date);
                        result.BillDisputes[i].BillDate = StringUtils.ConvertDateTime(result.BillDisputes[i].BillDate);
                        result.BillDisputes[i].Created = StringUtils.ConvertDateTime(result.BillDisputes[i].Created);
                        result.BillDisputes[i].Updated = StringUtils.ConvertDateTime(result.BillDisputes[i].Updated);

                        result.BillDisputes[i].Details = StringUtils.Remove_Escape_Sequences(result.BillDisputes[i].Details);
                        list.Add(result.BillDisputes[i]);
                    }
                    
                    ListBillDisputes.ItemsSource = list;
                });

            }, 10);
        }

        private void ListBillDisputes_Scroll(object sender, ScrollChangedEventArgs e)
        {
            ListView listView = (ListView)e.Source;
            int totalCount = listView.Items.Count;
            int limit = Convert.ToInt32(listView.Tag);
            if (totalCount == 0 || totalCount >= limit) return;
            if (e.VerticalOffset + e.VerticalChange >= totalCount - 6)
            {
                ShowBillDisputes(totalCount, 10);
            }
        }

        private void ListBillDisputes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBillDisputes == null) return;

            ButtonUpdate.IsEnabled = ListBillDisputes.SelectedItem != null;
            ButtonDelete.IsEnabled = ListBillDisputes.SelectedItem != null;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private async void ButtonNew_Click(object sender, RoutedEventArgs e)
        {
            await MaterialDesignThemes.Wpf.DialogHost.Show(new BillNewDispute(BillId, BillNumber), "DetailDialog");
        }

        private async void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            BillDisputeModel.Item item = (BillDisputeModel.Item)ListBillDisputes.SelectedItem;
            if (item == null || item.Id == 0) return;
            
            await MaterialDesignThemes.Wpf.DialogHost.Show(new BillNewDispute(BillId, BillNumber, item.Id), "DetailDialog");
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            BillDisputeModel.Item item = (BillDisputeModel.Item)ListBillDisputes.SelectedItem;
            if (item == null || item.Id == 0) return;

            List<BillDisputeModel.Item> list = (List<BillDisputeModel.Item>)ListBillDisputes.ItemsSource;
            LoadingPanel.Visibility = Visibility.Hidden;

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(Constants.BillDisputesDetailURL, Method.Delete, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("DisputeId", item.Id), Global.BuildDictionary("api-version", 1.0), null);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result == HttpStatusCode.OK)
                    {
                        list.Remove(item);
                        ListBillDisputes.ItemsSource = new List<BillDisputeModel.Item>();
                        ListBillDisputes.ItemsSource = list;
                        LoadingPanel.Visibility = Visibility.Hidden;
                    } 
                    else
                    {
                        MessageUtils.ShowErrorMessage("", "Bill Dispute delete failed.");
                        LoadingPanel.Visibility = Visibility.Hidden;
                    }
                });

            }, 10);

        }

    }
}
