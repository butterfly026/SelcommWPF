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

namespace SelcommWPF.dialogs.charge
{
    /// <summary>
    /// Interaction logic for ChargeInstanceDetail.xaml
    /// </summary>
    public partial class ChargeInstanceDetail : UserControl
    {

        private long ProfileId;

        public ChargeInstanceDetail(long id, bool isServices)
        {
            ProfileId = id;
            InitializeComponent();
            LabelDialogTitle.Content = isServices ? Properties.Resources.Service_Charge_Detail : Properties.Resources.Account_Charge_Detail;
            ShowChargeDetail(0, 10);
        }


        private void ListChargeDetail_Scroll(object sender, ScrollChangedEventArgs e)
        {
            ListView listView = (ListView)e.Source;
            int totalCount = listView.Items.Count;
            int limit = Convert.ToInt32(listView.Tag);
            if (totalCount == 0 || totalCount >= limit) return;
            if (e.VerticalOffset + e.VerticalChange >= totalCount - 6)
            {
                ShowChargeDetail(totalCount, 10);
            }
        }


        private void ShowChargeDetail(int skip, int take)
        {
            if (skip == 0) ListChargeDetail.ItemsSource = new List<AccountCharge.History>();
            if (Global.ContactCode == "")
            {
                MessageUtils.ShowErrorMessage(Properties.Resources.Error, Properties.Resources.Enter_Account_No);
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.3);
                query.Add("SkipRecords", skip);
                query.Add("TakeRecords", take);
                query.Add("countRecords", "Y");
                AccountCharge result = Global.RequestAPI<AccountCharge>(Constants.ChargesInstanceDetailURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ProfileId", ProfileId), query, "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    List<AccountCharge.History> list = (List<AccountCharge.History>)ListChargeDetail.ItemsSource;
                    ListChargeDetail.ItemsSource = new List<AccountCharge.History>();

                    if (list == null) list = new List<AccountCharge.History>();
                    int count = result == null ? 0 : result.Items.Count;
                    ListChargeDetail.Tag = result.Count;

                    for (int i = 0; i < count; i++)
                    {
                        result.Items[i].Created = StringUtils.ConvertDateTime(result.Items[i].Created);
                        result.Items[i].LastUpdated = StringUtils.ConvertDateTime(result.Items[i].LastUpdated);
                        result.Items[i].From = StringUtils.ConvertDateTime(result.Items[i].From);
                        result.Items[i].To = StringUtils.ConvertDateTime(result.Items[i].To);

                        result.Items[i].PriceTaxExText = StringUtils.ConvertCurrency(result.Items[i].PriceTaxEx);
                        result.Items[i].PriceTaxIncText = StringUtils.ConvertCurrency(result.Items[i].PriceTaxInc);
                        result.Items[i].UndiscountedPriceTaxExText = StringUtils.ConvertCurrency(result.Items[i].UndiscountedPriceTaxEx);
                        result.Items[i].UndiscountedPriceTaxIncText = StringUtils.ConvertCurrency(result.Items[i].UndiscountedPriceTaxInc);
                        result.Items[i].CostText = StringUtils.ConvertCurrency(result.Items[i].Cost.Value);

                        list.Add(result.Items[i]);
                    }

                    ListChargeDetail.ItemsSource = list;
                });

            }, 10);
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("DetailDialog");
        }
    }
}
