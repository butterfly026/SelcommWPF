using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace SelcommWPF.dialogs.charge
{
    /// <summary>
    /// Interaction logic for ChargeHistoryDialog.xaml
    /// </summary>
    public partial class ChargeHistoryDialog : UserControl
    {

        private bool IsServices = false;
        private string ServiceReference = "";

        public ChargeHistoryDialog(bool isServices = false, string serviceRef = "")
        {
            IsServices = isServices;
            if (IsServices) ServiceReference = serviceRef;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl ()
        {
            Global.ChargeDialog = this;
            LabelDialogTitle.Content = IsServices ? Properties.Resources.Service_Changes : Properties.Resources.Account_Charges;
            CheckBoxAccount.Visibility = IsServices ? Visibility.Collapsed : Visibility.Visible;

            if (IsServices)
            {
                GridView grid = (GridView)ListChargeHistory.View;
                for (int i = 1; i < 6; i++)
                {
                    GridViewColumnHeader column = (GridViewColumnHeader)grid.Columns[i].Header;
                    column.Visibility = Visibility.Collapsed;
                }
            }
            
            ShowChargeHistory(0, 20);
        }

        private void ListCharges_Scroll(object sender, ScrollChangedEventArgs e)
        {
            ListView listView = (ListView)e.Source;
            int totalCount = listView.Items.Count;
            int limit = Convert.ToInt32(listView.Tag);
            if (totalCount == 0 || totalCount >= limit) return;
            if (e.VerticalOffset + e.VerticalChange >= totalCount - 9)
            {
                ShowChargeHistory(totalCount, 20);
            }
        }

        private void CheckBoxAccount_Checked(object sender, RoutedEventArgs e)
        {
            ShowChargeHistory(0, 10);
        }

        private void TextChargeSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowChargeHistory(0, 10);
        }

        private void ListChargeHistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListChargeHistory == null || ListChargeHistory.Items.Count == 0) return;
            AccountCharge.History item = (AccountCharge.History)ListChargeHistory.SelectedItem;

            if (ListChargeHistory.SelectedItem != null)
            {
                ButtonChargeUpdate.IsEnabled = true;
                ButtonInstanceDetail.IsEnabled = true;
                ButtonChargeEnd.IsEnabled = item.To != Properties.Resources.On_going;
                ButtonChargeDelete.IsEnabled = item.Status == Properties.Resources.New;
            }
            else
            {
                ButtonChargeUpdate.IsEnabled = false;
                ButtonInstanceDetail.IsEnabled = false;
                ButtonChargeEnd.IsEnabled = false;
                ButtonChargeDelete.IsEnabled = false;
            }
        }

        public void ShowChargeHistory(int skip, int take)
        {
            if (skip == 0) ListChargeHistory.ItemsSource = new List<AccountCharge.History>();
            if (Global.ContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;
            bool isAccountOnly = CheckBoxAccount.IsChecked.Value;
            string url = IsServices ? Constants.ChargesServiceURL : Constants.ChargesHistoryURL;
            string code = IsServices ? ServiceReference : Global.ContactCode;
            string search = TextChargeSearch.Text == "" ? "**" : TextChargeSearch.Text;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.3);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("countRecords", "Y");
                query.Add("AccountOnly", isAccountOnly);
                query.Add("SearchString", search);
                AccountCharge result = Global.RequestAPI<AccountCharge>(url, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary(IsServices ? "ServiceReference" : "ContactCode", code), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    List<AccountCharge.History> list = (List<AccountCharge.History>)ListChargeHistory.ItemsSource;
                    ListChargeHistory.ItemsSource = new List<AccountCharge.History>();

                    if (list == null) list = new List<AccountCharge.History>();
                    int count = result == null ? 0 : result.Items.Count;
                    ListChargeHistory.Tag = result == null ? 0 : result.Count;

                    for (int i = 0; i < count; i++)
                    {
                        result.Items[i].Created = StringUtils.ConvertDateTime(result.Items[i].Created);
                        result.Items[i].LastUpdated = StringUtils.ConvertDateTime(result.Items[i].LastUpdated);
                        result.Items[i].From = StringUtils.ConvertDateTime(result.Items[i].From);
                        result.Items[i].To = result.Items[i].To.Contains("9999") ? Properties.Resources.On_going : StringUtils.ConvertDateTime(result.Items[i].To);

                        result.Items[i].PriceText = StringUtils.ConvertCurrency(result.Items[i].Price);
                        result.Items[i].DiscountAmountText = StringUtils.ConvertCurrency(result.Items[i].DiscountAmount);
                        result.Items[i].CostText = StringUtils.ConvertCurrency(result.Items[i].Cost == null ? 0 : result.Items[i].Cost.Value);
                        result.Items[i].OverRidePriceText = result.Items[i].OverRidePrice == null ? "" : StringUtils.ConvertCurrency(result.Items[i].OverRidePrice.Value);

                        list.Add(result.Items[i]);
                    }

                    ListChargeHistory.ItemsSource = list;
                });

            }, 10);
        }

        public void UpdateChargeEnd(long profileId, string dateTime)
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(Constants.ChargesEndURL, Method.Patch, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ProfileId", profileId), Global.BuildDictionary("api-version", 1.3), Global.BuildDictionary("To", dateTime));

                if (result == HttpStatusCode.OK)
                {
                    Application.Current.Dispatcher.Invoke(delegate
                    {

                        List<AccountCharge.History> list = (List<AccountCharge.History>)ListChargeHistory.ItemsSource;
                        int count = list.Count;

                        for (int i = 0; i < count; i++)
                        {
                            if (profileId == list[i].Id)
                            {
                                list[i].To = StringUtils.ConvertDateTime(dateTime);
                                break;
                            }
                        }

                        ListChargeHistory.ItemsSource = new List<AccountCharge.History>();
                        ListChargeHistory.ItemsSource = list;
                        LoadingPanel.Visibility = Visibility.Hidden;

                    });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        MessageUtils.ShowErrorMessage("", Properties.Resources.End_Charge_Fail);
                        LoadingPanel.Visibility = Visibility.Hidden;
                    });
                }

            }, 10);

        }

        private async void ButtonChargeEnd_Click(object sender, RoutedEventArgs e)
        {
            AccountCharge.History selectedItem = (AccountCharge.History)ListChargeHistory.SelectedItem;
            string code = IsServices ? ServiceReference : Global.ContactCode;
            
            await MaterialDesignThemes.Wpf.DialogHost.Show(new EndChargeDialog(selectedItem.Id, code, IsServices, ServiceReference), "DetailDialog");
        }

        private async void ButtonChargeNew_Click(object sender, RoutedEventArgs e)
        {
            await MaterialDesignThemes.Wpf.DialogHost.Show(new AccountChargeDialog(false, IsServices, ServiceReference), "DetailDialog");
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private async void ButtonInstanceDetail_Click(object sender, RoutedEventArgs e)
        {
            AccountCharge.History item = (AccountCharge.History)ListChargeHistory.SelectedItem;
            if (item == null || item.ProfileId == 0) return;
            
            await MaterialDesignThemes.Wpf.DialogHost.Show(new ChargeInstanceDetail(item.Id, IsServices), "DetailDialog");
        }
    }
}
