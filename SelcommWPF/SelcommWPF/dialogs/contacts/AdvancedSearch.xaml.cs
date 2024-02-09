using Newtonsoft.Json;
using RestSharp;
using SelcommWPF.global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace SelcommWPF.dialogs.contacts
{
    /// <summary>
    /// Interaction logic for AdvancedSearch.xaml
    /// </summary>
    public partial class AdvancedSearch : UserControl
    {
        private List<Dictionary<string, object>> ServiceTypeList = new List<Dictionary<string, object>>();
        private List<Dictionary<string, object>> BillingCycleList = new List<Dictionary<string, object>>();
        private List<Dictionary<string, object>> SubTypeList = new List<Dictionary<string, object>>();
        private List<Dictionary<string, object>> BussinessUnitList = new List<Dictionary<string, object>>();
        private List<Dictionary<string, object>> SearchPlanList = new List<Dictionary<string, object>>();
        private List<Dictionary<string, object>> SearchStatusList = new List<Dictionary<string, object>>();

        public AdvancedSearch()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            if (Global.contactClient == null) Global.contactClient = new clients.ContactClient();
            ShowHideVerticalScroll(Global.HasVerticalScroll);
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> header = Global.GetHeader(Global.CustomerToken);
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                ServiceTypeList = Global.RequestAPI<List<Dictionary<string, object>>>(Constants.SearchTypeURL, Method.Get, header, null, query, "", false);
                BillingCycleList = Global.RequestAPI<List<Dictionary<string, object>>>(Constants.BillingCycleURL, Method.Get, header, null, query, "", false);
                BussinessUnitList = Global.RequestAPI<List<Dictionary<string, object>>>(Constants.BussinessUnitURL, Method.Get, header, null, query, "", false);
                SubTypeList = Global.RequestAPI<List<Dictionary<string, object>>>(Constants.SubTypeURL, Method.Get, header, null, query, "", false);
                SearchPlanList = Global.RequestAPI<List<Dictionary<string, object>>>(Constants.SearchPlanURL, Method.Get, header, null, query, "", false);
                SearchStatusList = Global.RequestAPI<List<Dictionary<string, object>>>(Constants.SearchStatusURL, Method.Get, header, null, query, "", false);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (ServiceTypeList == null || BillingCycleList == null || BussinessUnitList == null || SubTypeList == null || SearchPlanList == null || SearchStatusList == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    List<string> list1 = new List<string>();
                    foreach (Dictionary<string, object> item in ServiceTypeList) list1.Add(item["Name"].ToString());
                    ComboServiceType.ItemsSource = list1;

                    List<string> list2 = new List<string>();
                    foreach (Dictionary<string, object> item in BillingCycleList) list2.Add(item["Name"].ToString());
                    ComboBillCyle.ItemsSource = list2;

                    List<string> list3 = new List<string>();
                    foreach (Dictionary<string, object> item in BussinessUnitList) list3.Add(item["Name"].ToString());
                    ComboBussinessUnit.ItemsSource = list3 ;

                    List<string> list4 = new List<string>();
                    foreach (Dictionary<string, object> item in SubTypeList) list4.Add(item["Name"].ToString());
                    ComboType.ItemsSource = list4;

                    List<string> list5 = new List<string>();
                    foreach (Dictionary<string, object> item in SearchPlanList) list5.Add(item["Name"].ToString());
                    ComboPlan.ItemsSource = list5;

                    List<string> list6 = new List<string>();
                    foreach (Dictionary<string, object> item in SearchStatusList) list6.Add(item["Name"].ToString());
                    ComboStatus.ItemsSource = list6;

                    ComboAccountsOnly.ItemsSource = new string[] { "Yes", "No" };
                });

            }, 10);

        }

        private void ShowHideVerticalScroll(bool isChecked)
        {
            ScrollViewer[] scrolls = new ScrollViewer[] { ScrollAdvancedSearch};
            int count = scrolls.Length;

            for (int i = 0; i < count; i++) if (scrolls[i] == null) return;
            for (int i = 0; i < count; i++) scrolls[i].VerticalScrollBarVisibility = isChecked ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
        }

        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("BusinessUnitCodes", ComboBussinessUnit.SelectedIndex == -1 ? new string[] { } : new string[] { BussinessUnitList[ComboBussinessUnit.SelectedIndex]["Id"].ToString() });
            body.Add("Postcode", TextPostalCode.Text);
            body.Add("ContactPhone", TextContactPhone.Text);
            body.Add("PaymentAccountNumber", TextPaymentAccountNumber.Text);
            body.Add("PaymentAccountName", TextPaymentAccountName.Text);
            body.Add("BillNumber", TextBillNumber.Text);
            body.Add("FinancialTransactionNumber", TextFinancial.Text);
            body.Add("State", TextState.Text);
            body.Add("ChequeNumber", TextCheque.Text);
            body.Add("CostCentre", TextCostCentre.Text);
            body.Add("BillCycleCode", ComboBussinessUnit.SelectedIndex == -1 ? "" : BillingCycleList[ComboBillCyle.SelectedIndex]["Id"].ToString());
            body.Add("PlanCodes", ComboPlan.SelectedIndex == -1 ? new int[] { } : new int[] { Convert.ToInt32(SearchPlanList[ComboPlan.SelectedIndex]["Id"]) });
            body.Add("PlanName", TextPlanName.Text);
            body.Add("Dealer", TextDealer.Text);
            body.Add("SIM", TextSIM.Text);
            body.Add("CostCentreCode", TextCostCentreCode.Text);
            body.Add("AccountsOnly", ComboAccountsOnly.SelectedIndex == 0);
            body.Add("Suburb", TextSuburb.Text);
            body.Add("FirstName", TextFirstName.Text);
            body.Add("ContactCode", TextContactCode.Text);
            body.Add("Corporate", true);
            body.Add("SubTypeCode", ComboType.SelectedIndex == -1 ? "" : SubTypeList[ComboType.SelectedIndex]["Id"].ToString());
            body.Add("ContactStatusCode", ComboStatus.SelectedIndex == -1 ? "" : SearchStatusList[ComboStatus.SelectedIndex]["Id"].ToString());
            body.Add("Key", TextKey.Text);
            body.Add("Name", TextName.Text);
            body.Add("Address", TextAddress.Text);
            body.Add("CreatedDays", TextCreatedDays.Text == "" ? 0 : Convert.ToInt32(TextCreatedDays.Text));
            body.Add("ServiceName", TextServiceLable.Text);
            body.Add("ServiceReference", TextServiceAttr.Text == "" ? 0 : Convert.ToInt32(TextServiceAttr.Text));
            body.Add("ServiceTypeCode", ComboServiceType.SelectedIndex == -1 ? "" : ServiceTypeList[ComboServiceType.SelectedIndex]["Id"].ToString());
            body.Add("Email", TextEmail.Text);
            body.Add("Alias", TextAlias.Text);
            body.Add("CompanyId", TextCompany.Text);
            body.Add("Service", TextSerivceNumber.Text);
            body.Add("DeclinedReceiptsDays", TextDeclinedReceipts.Text == "" ? 0 : Convert.ToInt32(TextDeclinedReceipts.Text));

            Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new SearchDialog(JsonConvert.SerializeObject(body), false), "MyDialogHost");
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
