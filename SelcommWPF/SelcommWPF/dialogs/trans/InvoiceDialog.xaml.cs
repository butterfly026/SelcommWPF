using Newtonsoft.Json;
using RestSharp;
using SelcommWPF.clients.models;
using SelcommWPF.clients.models.payment;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

namespace SelcommWPF.dialogs.trans
{
    /// <summary>
    /// Interaction logic for InvoiceDialog.xaml
    /// </summary>
    public partial class InvoiceDialog : UserControl
    {

        private string CheckString = "";
        private double TotalAmount = 0;
        private double TotalTax = 0;

        private List<Dictionary<string, string>> CategoryList;
        private List<string> CategoryStrList = new List<string>();
        private List<Dictionary<string, string>> ReasonList;
        private List<string> ReasonStrList = new List<string>();

        public InvoiceDialog()
        {
            InitializeComponent();
            InitalizeControl();
        }

        private void InitalizeControl()
        {
            Global.NewInvoiceDialog = this;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                query.Add("Open", true);
                query.Add("DefaultOnly", false);
                Dictionary<string, string> receiptNumber = Global.RequestAPI<Dictionary<string, string>>(Constants.TransactionNumberURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("Type", "I"), query, "");
                CategoryList = Global.RequestAPI<List<Dictionary<string, string>>>(Constants.TransactionCategoryURL, Method.Get, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("Type", "I"), Global.BuildDictionary("api-version", 1.0), "");
                ReasonList = Global.RequestAPI<List<Dictionary<string, string>>>(Constants.TransactionReasonURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, Global.BuildDictionary("api-version", 1.0), "");

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (receiptNumber == null || CategoryList == null || ReasonList == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    TextNumber.Text = receiptNumber["Number"];
                    TextAmount.Text = "0";
                    TextTax.Text = "0";

                    foreach (Dictionary<string, string> item in CategoryList) CategoryStrList.Add(item["Name"]);
                    foreach (Dictionary<string, string> item in ReasonList) ReasonStrList.Add(item["Name"]);

                    ComboCategory.ItemsSource = CategoryStrList;
                    ComboReason.ItemsSource = ReasonStrList;
                    DatePickerInvoice.SelectedDate = DateTime.Now;
                });

            }, 10);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (ComboReason.SelectedIndex == -1)
            {
                ComboReason.Focus();
                return;
            }

            if (ComboCategory.SelectedIndex == -1)
            {
                ComboCategory.Focus();
                return;
            }

            List<object> chargeList = new List<object>();
            List<AccountCharge.History> list = (List<AccountCharge.History>)ListCharges.ItemsSource;
            foreach (AccountCharge.History item in list)
            {
                var value = new
                {
                    DefinitionId = item.DefinitionId,
                    Amount = item.Cost,
                    From = item.From,
                    To = item.To == Properties.Resources.On_going ? "9999-12-31T12:59:59" : item.To,
                    Note = item.Description
                };
                chargeList.Add(value);
            }

            List<object> productList = new List<object>();
            List<ProductModel.Data> list1 = (List<ProductModel.Data>)ListProducts.ItemsSource;
            foreach (ProductModel.Data item in list1)
            {
                var value = new
                {
                    ProductId = item.ProductId,
                    Description = item.OverrideDescription,
                    Date = DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss"),
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    UnitTax = item.Tax,
                    SerialNumber = item.SerialsText,
                    Note = item.Note,
                    Ids = item.Serials
                };
                productList.Add(value);
            }

            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("Number", TextNumber.Text);
            body.Add("Date", DatePickerInvoice.SelectedDate.Value.ToString("yyyy-MM-ddThh:mm:ss"));
            body.Add("DueDate", DatePickerDue.SelectedDate.Value.ToString("yyyy-MM-ddThh:mm:ss"));
            body.Add("Amount", TotalAmount);
            body.Add("TaxAmount", TotalTax);
            body.Add("ReasonId", ReasonList[ComboReason.SelectedIndex]["Code"]);
            body.Add("OtherReference", TextReference.Text);
            body.Add("CategoryId", CategoryList[ComboCategory.SelectedIndex]["Code"]);
            body.Add("SourceId", Global.LoggedUser.BusinessUnits[0].Id);
            body.Add("StatusId", "AP");
            body.Add("CreateDocument", true);
            body.Add("Email", false);
            body.Add("Charges", chargeList);
            body.Add("Products", productList);

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                HttpStatusCode result = Global.RequestAPI(Constants.InvoiceCreateURL, Method.Post, Global.GetHeader(Global.CustomerToken), Global.BuildDictionary("AccountCode", Global.ContactCode),
                    Global.BuildDictionary("api-version", 1.0), body);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result == HttpStatusCode.OK || result == HttpStatusCode.Created)
                    {
                        MessageUtils.ShowMessage("", Properties.Resources.Invoice_Create_Sucess);
                        Global.CloseDialog();
                    }
                    LoadingPanel.Visibility = Visibility.Hidden;
                });

            }, 10);

        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void TextAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string str = textBox.Text;

            if (str == CheckString)
            {
                if (textBox.Name == "TextAmount") TotalAmount = Convert.ToDouble(CheckString.Replace("A$", "").Replace(",", ""));
                else if (textBox.Name == "TextTax") TotalTax = Convert.ToDouble(CheckString.Replace("A$", "").Replace(",", ""));
                textBox.SelectionStart = str.Length;
                CheckString = "";
                ButtonSave.IsEnabled = TextAmount.Text != "";
                return;
            }

            if (str.Length > 19)
            {
                textBox.Text = str.Substring(0, 19);
                textBox.SelectionStart = str.Length;
                CheckString = textBox.Text;
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

            CheckString = str;
            textBox.Text = str;
        }

        private void TextAmount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        public void InsertChargeData(string chargeData, bool isUpdate = false)
        {
            List<AccountCharge.History> list = (List<AccountCharge.History>)ListCharges.ItemsSource ?? new List<AccountCharge.History>();
            AccountCharge.History item = JsonConvert.DeserializeObject<AccountCharge.History>(chargeData);

            item.PriceText = StringUtils.ConvertCurrency(item.Price);
            item.CostText = StringUtils.ConvertCurrency(item.Cost.Value);
            item.DiscountAmountText = item.DiscountType == "Percentage" ? string.Format("{0}%", item.DiscountAmount) : StringUtils.ConvertCurrency(item.DiscountAmount);
            item.Start = StringUtils.ConvertDateTime(item.Start);

            if (isUpdate) list[ListCharges.SelectedIndex] = item;
            else list.Add(item);

            ListCharges.ItemsSource = new List<AccountCharge.History>();
            ListCharges.ItemsSource = list;

            TotalAmount += item.Cost.Value;
            TotalTax += item.PriceTaxInc;
            CheckString = StringUtils.ConvertCurrency(TotalAmount);
            TextAmount.Text = CheckString;
            CheckString = StringUtils.ConvertCurrency(TotalTax);
            TextTax.Text = CheckString;
        }

        private async void ButtonAddCharge_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new AccountChargeDialog(false, false, "", true), "DetailDialog");
        }

        private void ButtonRemoveCharge_Click(object sender, RoutedEventArgs e)
        {
            if (ListCharges.SelectedItem == null) return;

            List<AccountCharge.History> list = (List<AccountCharge.History>)ListCharges.ItemsSource ?? new List<AccountCharge.History>();
            list.RemoveAt(ListCharges.SelectedIndex);
            ListCharges.ItemsSource = new List<AccountCharge.History>();
            ListCharges.ItemsSource = list;
        }

        private async void ButtonUpdateCharge_Click(object sender, RoutedEventArgs e)
        {
            if (ListCharges.SelectedItem == null) return;
            AccountCharge.History item = (AccountCharge.History)ListCharges.SelectedItem;

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new AccountChargeDialog(false, false, "", true, JsonConvert.SerializeObject(item)), "DetailDialog");
        }

        public void InsertProductData(string productData, bool isUpdate = false)
        {
            List<ProductModel.Data> list = (List<ProductModel.Data>)ListProducts.ItemsSource ?? new List<ProductModel.Data>();
            ProductModel.Data item = JsonConvert.DeserializeObject<ProductModel.Data>(productData);

            if (isUpdate) list[ListProducts.SelectedIndex] = item;
            else list.Add(item);

            ListProducts.ItemsSource = new List<AccountCharge.History>();
            ListProducts.ItemsSource = list;

            TotalAmount += item.TotalPrice;
            TotalTax += item.Tax;
            CheckString = StringUtils.ConvertCurrency(TotalAmount);
            TextAmount.Text = CheckString;
            CheckString = StringUtils.ConvertCurrency(TotalTax);
            TextTax.Text = CheckString;
        }

        private async void ButtonAddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new ProductDialog(), "DetailDialog");
        }

        private async void ButtonUpdateProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ListProducts.SelectedItem == null) return;
            ProductModel.Data item = (ProductModel.Data)ListProducts.SelectedItem;

            if (MaterialDesignThemes.Wpf.DialogHost.IsDialogOpen("DetailDialog")) Global.CloseDialog();
            await MaterialDesignThemes.Wpf.DialogHost.Show(new ProductDialog(JsonConvert.SerializeObject(item)), "DetailDialog");
        }

        private void ButtonRemoveProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ListProducts.SelectedItem == null) return;

            List<ProductModel.Data> list = (List<ProductModel.Data>)ListProducts.ItemsSource ?? new List<ProductModel.Data>();
            list.RemoveAt(ListProducts.SelectedIndex);
            ListProducts.ItemsSource = new List<ProductModel.Data>();
            ListProducts.ItemsSource = list;
        }

        private void DatePickerInvoice_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatePickerInvoice.SelectedDate == null || DatePickerDue == null) return;
            DatePickerDue.SelectedDate = DatePickerInvoice.SelectedDate.Value.AddMonths(15);
        }

        private void ListCharges_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isEnable = ListCharges.SelectedItem != null;
            ButtonUpdate.IsEnabled = isEnable;
            ButtonRemove.IsEnabled = isEnable;
        }

        private void ListProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isEnable = ListProducts.SelectedItem != null;
            ButtonProductUpdate.IsEnabled = isEnable;
            ButtonProductRemove.IsEnabled = isEnable;
        }
    }
}
