using Newtonsoft.Json;
using RestSharp;
using SelcommWPF.clients.models.payment;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace SelcommWPF.dialogs.trans
{
    /// <summary>
    /// Interaction logic for ProductDialog.xaml
    /// </summary>
    public partial class ProductDialog : UserControl
    {

        private List<ProductModel.Item> ProductList;
        private ProductModel.Item SelectedProduct;
        private ProductModel.Data SelectedData;
        private List<string> IdsList = new List<string>();

        private bool IsSelectedProduct = false;
        private IDisposable RequestTimer = null;

        private string CheckString = "";
        private double TotalAmount = 0;

        public ProductDialog(string data = "")
        {
            if (data != "") SelectedData = JsonConvert.DeserializeObject<ProductModel.Data>(data);
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            if (SelectedData == null)
            {
                TextUnitPrice.Text = "0";
                TextTax.Text = "0";
            }
            else
            {
                SelectedProduct = new ProductModel.Item();
                SelectedProduct.Id = SelectedData.ProductId;
                SelectedProduct.Name = SelectedData.ProductName;
                IsSelectedProduct = true;
                TextBoxProduct.Text = SelectedData.ProductName;
                CheckString = SelectedData.UnitPriceText;
                TextUnitPrice.Text = SelectedData.UnitPriceText;
                TextQuantity.Text = SelectedData.Quantity + "";
                CheckString = StringUtils.ConvertCurrency(SelectedData.Tax);
                TextTax.Text = CheckString;
                IdsList = SelectedData.SerialsText.Split(',').ToList();
                TotalAmount = SelectedData.TotalPrice;
                LabelTotalAmount.Content = string.Format("Total Price {0}", SelectedData.TotalPriceText);
                TextOverrideDescription.Text = SelectedData.OverrideDescription;
                TextNote.Text = SelectedData.Note;
                PanelContent.Visibility = Visibility.Visible;
            }
            
        }

        private void TextBoxProduct_KeyUp(object sender, KeyEventArgs e)
        {
            string search = TextBoxProduct.Text;

            if (search == "" || RequestTimer != null || IsSelectedProduct)
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

            RequestTimer = EasyTimer.SetTimeout(() =>
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    search = TextBoxProduct.Text;
                    ListDefinitions.ItemsSource = new List<string>();
                    ProgressDetail.Visibility = Visibility.Visible;
                    GridDefinitions.Visibility = Visibility.Collapsed;
                });

                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                query.Add("SkipRecords", 0);
                query.Add("TakeRecords", 20);
                query.Add("CountRecords", "Y");
                query.Add("SearchString", search);
                ProductModel result = Global.RequestAPI<ProductModel>(Constants.ProductListURL, Method.Get, Global.GetHeader(Global.CustomerToken), null, query, "");
                Application.Current.Dispatcher.Invoke(delegate
                {

                    if (result != null && result.Count != 0 && !IsSelectedProduct)
                    {
                        List<string> items = new List<string>();
                        foreach (ProductModel.Item item in result.Items) items.Add(item.Name);
                        ProductList = result.Items;
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

        private void TextBoxProduct_GotFocus(object sender, RoutedEventArgs e)
        {
            GridDefinitions.Visibility = ListDefinitions != null && ListDefinitions.Items.Count > 0 ? Visibility.Visible : Visibility.Hidden;
        }

        private void TextQuantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void TextPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string str = textBox.Text;

            if (str == CheckString)
            {
                textBox.SelectionStart = str.Length;
                CheckString = "";
                CalculateAmount();
                ButtonSave.IsEnabled = TextUnitPrice.Text != "" && TextBoxProduct.Text != "";
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
                str = str.Replace("A$", "");
                str = str.Replace(",", "");
                double price = Convert.ToDouble(str) * 10;
                if (price > 10000)
                {
                    MessageUtils.ShowErrorMessage("", string.Format(Properties.Resources.Min_Max_String, 0, 10000));
                    return;
                }
                str = StringUtils.ConvertCurrency(price);
            }
            else
            {
                str = StringUtils.ConvertCurrency(Convert.ToDouble(str) / 100);
            }

            CheckString = str;
            textBox.Text = str;
        }
        
        private void TextQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateAmount();
        }

        private void CalculateAmount()
        {
            try
            {
                int quantity = Convert.ToInt32(TextQuantity.Text);
                double price = Convert.ToDouble(TextUnitPrice.Text.Replace("A$", "").Replace(",", ""));
                double tax = Convert.ToDouble(TextTax.Text.Replace("A$", "").Replace(",", ""));

                TotalAmount = quantity * price + tax;
                LabelTotalAmount.Content = string.Format("Total Price {0}", StringUtils.ConvertCurrency(TotalAmount));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            ProductModel.Data item = new ProductModel.Data();
            item.ProductId = SelectedProduct.Id;
            item.ProductName = SelectedProduct.Name;
            item.OverrideDescription = TextOverrideDescription.Text;
            item.Quantity = Convert.ToInt32(TextQuantity.Text);
            item.UnitPriceText = TextUnitPrice.Text;
            item.UnitPrice = Convert.ToDouble(item.UnitPriceText.Replace("A$", "").Replace(",", ""));
            item.TotalPrice = TotalAmount;
            item.TotalPriceText = StringUtils.ConvertCurrency(TotalAmount);
            item.Tax = Convert.ToDouble(TextTax.Text.Replace("A$", "").Replace(",", ""));

            string serialsText = "";
            List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();

            foreach(string id in IdsList) {
                Dictionary<string, string> value = new Dictionary<string, string>();
                value.Add("Id", id);
                list.Add(value);
                serialsText += "," + id;
            }

            item.Serials = list;
            item.SerialsText = serialsText == "" ? "" : serialsText.Substring(1);
            item.Note = TextNote.Text;

            Global.NewInvoiceDialog.InsertProductData(JsonConvert.SerializeObject(item), SelectedData != null);
            Global.CloseDialog("DetailDialog");
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog("DetailDialog");
        }

        private void ButtonAddIds_Click(object sender, RoutedEventArgs e)
        {
            string searial = TextSerial.Text;
            if (string.IsNullOrEmpty(searial) || IdsList.Contains(searial)) return;

            IdsList.Add(searial);
            TextSerial.Text = "";
        }

        private void ListDefinitions_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SelectProduct();
        }

        private void ListDefinitions_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SelectProduct();
        }

        private void SelectProduct()
        {
            if (ListDefinitions.SelectedIndex == -1) return;

            SelectedProduct = ProductList[ListDefinitions.SelectedIndex];
            IsSelectedProduct = true;

            CheckString = StringUtils.ConvertCurrency(SelectedProduct.DefaultPrice);
            TextUnitPrice.Text = CheckString;

            CheckString = StringUtils.ConvertCurrency(SelectedProduct.DefaultTax);
            TextTax.Text = CheckString;

            //GridSerial.Visibility = SelectedProduct.Serialised ? Visibility.Visible : Visibility.Collapsed;
            PanelContent.Visibility = Visibility.Visible;
            ListDefinitions.Visibility = Visibility.Collapsed;
            TextBoxProduct.Text = SelectedProduct.Name;
        }

    }
}
