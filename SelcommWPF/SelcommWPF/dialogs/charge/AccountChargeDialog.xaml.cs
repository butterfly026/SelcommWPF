using Newtonsoft.Json;
using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.dialogs.accounts;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
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

namespace SelcommWPF.dialogs
{
    /// <summary>
    /// Interaction logic for AccountCharge.xaml
    /// </summary>
    public partial class AccountChargeDialog : UserControl
    {

        private bool IsFromMenu;
        private bool IsFromInvoice;
        private bool IsServices;
        private bool IsFromAccount;

        private bool IsSelectedDefinition = false;
        private IDisposable RequestTimer = null;

        private string ServiceReference = "";
        private AccountCharge.History SelectedData;

        private List<Dictionary<string, object>> ChargeDefinitions;
        private Dictionary<string, object> SelectedDefinition;
        private Dictionary<string, object> CurrentDefinition;

        private string checkString = "";
        private double TaxAmount = 0;

        private double DiscountPercentage = 0;
        private double DiscountPrice = 0;
        private double UnitPrice = 0;
        private double Amount = 0;

        public AccountChargeDialog(bool isFromMenu = true, bool isServices = false, string reference = "", bool isFromInvoice = false, string data = "", bool isFromAccount = false)
        {
            IsFromMenu = isFromMenu;
            IsFromInvoice = isFromInvoice;
            IsServices = isServices;
            IsFromAccount = isFromAccount;
            if (isServices) ServiceReference = reference;
            if (data != "") SelectedData = JsonConvert.DeserializeObject<AccountCharge.History>(data);
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            ComboDiscount.ItemsSource = new string[] { "None", "Percentage", "Fixed" };
            ComboDiscount.SelectedIndex = 0;

            if (SelectedData == null) return;
            LoadingPanel.Visibility = Visibility;
            IsSelectedDefinition = true;

            EasyTimer.SetTimeout(() =>
            {
                CurrentDefinition = Global.RequestAPI<Dictionary<string, object>>(Constants.ChargesDetailDefURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("DefinitionId", SelectedData.DefinitionId), Global.BuildDictionary("api-version", 1.3), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (CurrentDefinition == null)
                    {
                        Global.CloseDialog(IsFromMenu ? "MyDialogHost" : "DetailDialog");
                        return;
                    }

                    TextBoxCharge.Text = SelectedData.Description;
                    checkString = StringUtils.ConvertCurrency(SelectedData.Price);
                    TextUnitPrice.Text = checkString;
                    TextAddtionalDescription.Text = SelectedData.Description;
                    Amount = SelectedData.Cost.Value;
                    ComboDiscount.SelectedIndex = new List<string>() { "None", "Percentage", "Fixed" }.IndexOf(SelectedData.DiscountType);
                    if (ComboDiscount.SelectedIndex == 2)
                    {
                        checkString = SelectedData.DiscountAmountText;
                        TextFixedDiscount.Text = SelectedData.DiscountAmountText;
                    }
                    else if (ComboDiscount.SelectedIndex == 1)
                    {
                        checkString = SelectedData.DiscountAmountText;
                        TextPercentageDiscount.Text = SelectedData.DiscountAmountText;
                    }

                    TextQuantity.Text = SelectedData.Quantity + "";
                    LabelTotalAmount.Content = string.Format(Properties.Resources.Total_Charge, StringUtils.ConvertCurrency(Amount));
                    ListDefinitions.ItemsSource = new List<string>();
                    GridDefinitions.Visibility = Visibility.Hidden;
                    PanelContent.Visibility = Visibility.Visible;
                    DatePickerStart.SelectedDate = DateTime.Now;
                    TimePickerStart.SelectedTime = DateTime.Now;
                });

            }, 10);

        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("DefinitionId", CurrentDefinition["Id"].ToString());
            body.Add("ContraIfSuspended", false);
            body.Add("RevenueAccount", CurrentDefinition["RevenueAccount"] == null ? "" : CurrentDefinition["RevenueAccount"].ToString());
            body.Add("OtherReference", TextOtherReference.Text);
            body.Add("Reference", TextReference.Text);
            body.Add("CustomerReference", TextCustomerReference.Text);
            body.Add("Note", TextNote.Text);
            body.Add("PlanInstanceId", 0);
            body.Add("AdvancePeriods", CurrentDefinition["AdvancePeriods"].ToString());
            body.Add("ChargeInAdvance", CurrentDefinition["ChargeInAdvance"].ToString());
            body.Add("Prorated", CurrentDefinition["Prorated"].ToString());
            body.Add("ETF", false);
            body.Add("Frequency", CurrentDefinition["Frequency"].ToString().Replace("-", ""));
            body.Add("OverrideDescription", TextAddtionalDescription.Text);
            body.Add("InvoiceDescription", TextAddtionalDescription.Text);
            body.Add("AdjustToDisconnection", false);
            body.Add("From", DatePickerStart.SelectedDate.Value.ToString("yyyy-MM-dd") + "T" + TimePickerStart.SelectedTime.Value.ToString("HH:mm:ss"));
            body.Add("To", "9999-01-01T00:00:00");
            body.Add("Cost", TextCost.Text == "" ? 0 : Math.Round(Convert.ToDouble(TextCost.Text.Replace("A$", "").Replace(",", "")), 2));
            body.Add("UnitDiscountPercentage", Math.Round(DiscountPercentage, 2));
            body.Add("UnitDiscount", Math.Round(DiscountPrice, 2));
            body.Add("UnitPrice", Math.Round(UnitPrice, 2));
            body.Add("Quantity", Convert.ToInt32(TextQuantity.Text));
            body.Add("Units", CurrentDefinition["Unit"] == null ? "" : CurrentDefinition["Unit"].ToString());
            body.Add("Amount", Amount);
            body.Add("InvoiceGroup", CurrentDefinition["InvoiceGroup"].ToString());
            body.Add("CheckLimits", false);

            if (IsFromInvoice)
            {
                body["To"] = "On going";
                body.Add("Description", CurrentDefinition["Name"].ToString());
                body.Add("DiscountType", ComboDiscount.SelectedItem.ToString());
                body.Add("Price", Math.Round(UnitPrice, 2));
                body.Add("PriceTaxInc", TaxAmount);
                body["Cost"] = Amount;
                body.Add("DiscountAmount", new double[] { 0, Math.Round(DiscountPercentage, 2) , Math.Round(DiscountPrice, 2) }[ComboDiscount.SelectedIndex]);
                body.Add("NumberOfInstances", TextInstancesNumber.Text == "" ? 0 : Convert.ToInt32(TextInstancesNumber.Text));
                Global.NewInvoiceDialog.InsertChargeData(JsonConvert.SerializeObject(body), SelectedData != null);
                Global.CloseDialog("DetailDialog");
                return;
            }

            if (IsFromAccount)
            {
                body["To"] = "On going";
                body.Add("Description", CurrentDefinition["Name"].ToString());
                body.Add("DiscountType", ComboDiscount.SelectedItem.ToString());
                NewAccountDialog.Instance.InsertChargeData(JsonConvert.SerializeObject(body), SelectedData != null);
                Global.CloseDialog("DetailDialog");
                return;
            }

            string url = IsServices ? Constants.ChargesServiceURL : Constants.ChargesHistoryURL;
            string code = IsServices ? ServiceReference : Global.ContactCode;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(url, Method.Post, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary(IsServices ? "ServiceReference" : "ContactCode", code), Global.BuildDictionary("api-version", 1.3), body);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK || result == HttpStatusCode.Created)
                    {
                        MessageUtils.ShowMessage("", Properties.Resources.Account_Charge_Create);
                        if (!IsFromMenu) Global.ChargeDialog.ShowChargeHistory(0, 10);
                        Global.CloseDialog(IsFromMenu ? "MyDialogHost" : "DetailDialog");
                    }
                });

            }, 10);
            
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog(IsFromMenu ? "MyDialogHost" : "DetailDialog");
        }

        private void TextCharge_KeyUp(object sender, KeyEventArgs e)
        {
            string search = TextBoxCharge.Text;

            if (search == "" || RequestTimer != null || IsSelectedDefinition)
            {
                ListDefinitions.ItemsSource = new List<string>();
                GridDefinitions.Visibility = Visibility.Collapsed;
                return;
            }

            if (e.Key == Key.Down)
            {
                ListDefinitions.Focus();
                ListDefinitions.SelectedIndex = 0;
                return;
            }

            string url = IsFromAccount ? Constants.ChargesDefinitionURL : Constants.ChargesDefinedURL;

            RequestTimer = EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    search = TextBoxCharge.Text;
                    ListDefinitions.ItemsSource = new List<string>();
                    ProgressDetail.Visibility = Visibility.Visible;
                    GridDefinitions.Visibility = Visibility.Collapsed;
                });

                List<Dictionary<string, object>> result = null;
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.3);
                query.Add("SearchString", search);

                if (IsFromAccount)
                {
                    Dictionary<string, object> value = Global.RequestAPI<Dictionary<string, object>>(url, Method.Get, Global.GetHeader(Global.CustomerToken), null, query, "");
                    result = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(value["Items"].ToString());
                }
                else
                {
                    result = Global.RequestAPI<List<Dictionary<string, object>>>(url, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("ContactCode", Global.ContactCode), query, "");
                }

                Application.Current.Dispatcher.Invoke(delegate
                {

                    if (result != null && result.Count != 0 && !IsSelectedDefinition)
                    {
                        List<string> items = new List<string>();
                        foreach (Dictionary<string, object> item in result) items.Add(item["Name"].ToString());
                        ChargeDefinitions = result;
                        ListDefinitions.ItemsSource = items;
                        GridDefinitions.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ListDefinitions.ItemsSource = new List<string>();
                        GridDefinitions.Visibility = Visibility.Hidden;
                    }

                    RequestTimer.Dispose();
                    RequestTimer = null;
                    ProgressDetail.Visibility = Visibility.Collapsed;
                });

            }, 2000);
        }

        private void TextBoxCharge_GotFocus(object sender, RoutedEventArgs e)
        {
            GridDefinitions.Visibility = ListDefinitions != null && ListDefinitions.Items.Count > 0 ? Visibility.Visible : Visibility.Hidden;
        }
        
        private void TextBoxCharge_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back) IsSelectedDefinition = false;
        }

        private void TextQuantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void ButtonMore_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if (button.Content.ToString() == Properties.Resources.More)
            {
                GridMore.Visibility = Visibility.Visible;
                button.Content = Properties.Resources.Less;
            }
            else
            {
                GridMore.Visibility = Visibility.Collapsed;
                button.Content = Properties.Resources.More;
            }
        }

        private void TextPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string str = textBox.Text;

            if (str == checkString)
            {
                textBox.SelectionStart = str.Length;
                GetTaxInfoFromAmount();
                checkString = "";
                ButtonSave.IsEnabled = TextUnitPrice.Text != "";
                return;
            }

            if (str.Length > 19)
            {
                textBox.Text = str.Substring(0, 19);
                textBox.SelectionStart = str.Length;
                checkString = textBox.Text;
                return;
            }

            if (str.Contains("A$"))
            {
                TextChange change = e.Changes.First();
                str = str.Replace("A$", "").Replace(",", "");
                if (change.AddedLength == 0 && change.RemovedLength > 0)
                {

                    double price = Convert.ToDouble(str) / 10;
                    str = StringUtils.ConvertCurrency(price);
                }
                else
                {

                    double price = Convert.ToDouble(str) * 10;
                    str = StringUtils.ConvertCurrency(price);
                }
            }
            else
            {
                str = StringUtils.ConvertCurrency(Convert.ToDouble(str) / 100);
            }

            checkString = str;
            textBox.Text = str;
        }

        private void TextQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalcTotalAmount();
        }

        private void CalcTotalAmount()
        {
            try
            {
                UnitPrice = Convert.ToDouble(TextUnitPrice.Text.Replace("A$", "").Replace(",", ""));
                double price = UnitPrice * Convert.ToInt32(TextQuantity.Text);
                double discount = 0;

                if (ComboDiscount.SelectedIndex == 1)
                {
                    DiscountPercentage = Convert.ToDouble(TextPercentageDiscount.Text.Replace("%", ""));
                    discount = price * DiscountPercentage / 100;
                }
                else if (ComboDiscount.SelectedIndex == 2) discount = Convert.ToDouble(TextFixedDiscount.Text.Replace("A$", "").Replace(",", ""));

                DiscountPrice = discount;
                Amount = price - discount + TaxAmount;
                LabelTotalAmount.Content = string.Format(Properties.Resources.Total_Charge, StringUtils.ConvertCurrency(Amount));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox == null || GridDiscount == null || TextFixedDiscount == null || TextPercentageDiscount == null) return;

            switch (comboBox.SelectedIndex)
            {
                case 0:
                    GridDiscount.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    GridDiscount.Visibility = Visibility.Visible;
                    TextFixedDiscount.Visibility = Visibility.Collapsed;
                    TextPercentageDiscount.Visibility = Visibility.Visible;
                    break;
                case 2:
                    GridDiscount.Visibility = Visibility.Visible;
                    TextFixedDiscount.Visibility = Visibility.Visible;
                    TextPercentageDiscount.Visibility = Visibility.Collapsed;
                    break;
            }

            CalcTotalAmount();
        }

        private void TextFixedDiscount_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string str = textBox.Text;
            if (str == checkString)
            {
                textBox.SelectionStart = str.Length;
                CalcTotalAmount();
                checkString = "";
                return;
            }

            if (str.Length > 19)
            {
                textBox.Text = str.Substring(0, 19);
                textBox.SelectionStart = str.Length;
                checkString = textBox.Text;
                return;
            }

            if (str.Contains("%"))
            {
                str = str.Replace("%", "");
                str = str.Replace(".", "");
                if (str.Length > 2) str = str.Substring(0, 2) + "." + str.Substring(2) + "%";
                else str = str + "%";
            }
            else
            {
                str = str + "%";
            }

            checkString = str;
            textBox.Text = str;
        }

        private void TextUnitPrice_LostFocus(object sender, RoutedEventArgs e)
        {
            GetTaxInfoFromAmount();
        }

        private void GetTaxInfoFromAmount()
        {
            if (IsFromAccount)
            {
                CalcTotalAmount();
                return;
            }

            string amount = TextUnitPrice.Text.Replace("A$", "").Replace(",", "");
            amount = Convert.ToString(Math.Round(Convert.ToDouble(amount)));
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> path = Global.BuildDictionary("ContactCode", Global.ContactCode);
                path.Add("DefinitionId", CurrentDefinition["Id"].ToString());
                path.Add("Amount", amount);
                Dictionary<string, object> result = Global.RequestAPI<Dictionary<string, object>>(Constants.ChargesTaxURL, Method.Get, Global.GetHeader(Global.CustomerToken), path, Global.BuildDictionary("api-version", 1.3), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    try
                    {
                        TaxAmount = Math.Round(Convert.ToDouble(result["Tax"].ToString()), 2);
                        CalcTotalAmount();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        MessageUtils.ShowErrorMessage("", Properties.Resources.Tax_Info_Fail);
                    }
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);
        }

        private void ListDefinitions_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SelectDefinition();
        }

        private void ListDefinitions_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectDefinition();
        }

        private void SelectDefinition()
        {
            if (ListDefinitions.SelectedIndex == -1) return;

            SelectedDefinition = ChargeDefinitions[ListDefinitions.SelectedIndex];
            IsSelectedDefinition = true;

            LoadingPanel.Visibility = Visibility;
            EasyTimer.SetTimeout(() =>
            {
                CurrentDefinition = Global.RequestAPI<Dictionary<string, object>>(Constants.ChargesDetailDefURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("DefinitionId", SelectedDefinition["Id"].ToString()), Global.BuildDictionary("api-version", 1.3), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (CurrentDefinition == null) return;
                    TextBoxCharge.Text = SelectedDefinition["Name"].ToString();
                    checkString = StringUtils.ConvertCurrency(Math.Round(Convert.ToDouble(SelectedDefinition["DefaultPrice"].ToString()), 2));
                    TextUnitPrice.Text = checkString;
                    TextAddtionalDescription.Text = SelectedDefinition["Name"].ToString();
                    ListDefinitions.ItemsSource = new List<string>();
                    GridDefinitions.Visibility = Visibility.Hidden;
                    PanelContent.Visibility = Visibility.Visible;
                    DatePickerStart.SelectedDate = DateTime.Now;
                    TimePickerStart.SelectedTime = DateTime.Now;
                });

            }, 10);
        }
    }
}
