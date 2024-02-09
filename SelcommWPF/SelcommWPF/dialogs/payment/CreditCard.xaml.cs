using SelcommWPF.global;
using SelcommWPF.utils;
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

namespace SelcommWPF.dialogs
{
    /// <summary>
    /// Interaction logic for CreditCard.xaml
    /// </summary>
    public partial class CreditCard : UserControl
    {

        private string PayCode = "";
        private string ContactCode = "";
        private string DialogHost = "";

        public CreditCard(string code, string dialog = "MyDialogHost")
        {
            ContactCode = code;
            DialogHost = dialog;
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            string[] yearArr = new string[3];
            for (int i = 0; i < 3; i++) yearArr[i] = (DateTime.Now.Year + i) + "";

            string[] monthArr = new string[12];
            for (int i = 0; i < 12; i++) monthArr[i] = (i + 1) + "";

            ComboYear.ItemsSource = yearArr;
            ComboYear.SelectedIndex = -1;
            ComboMonth.ItemsSource = monthArr;
            ComboMonth.SelectedIndex = -1;
        }

        private void TextCreditCardNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = TextCreditCardNumber.Text;
            ButtonSend.IsEnabled = TextCreditCardNumber.Text != "" && TextCardholderName.Text != "" && ComboMonth.SelectedItem != null && ComboYear.SelectedItem != null;

            if (text.Length > 19)
            {
                TextCreditCardNumber.Text = text.Substring(0, 19);
                return;
            }

            text = text.Replace("-", "");
            int length = text.Length;
            string formattedText = "";

            if (TextCreditCardNumber.Text.Length == 19 && length == 16)
            {
                ValidateCreditCardNumber(text);
                return;
            }

            for (int i = 0; i < length; i++)
            {
                if (i % 4 == 0) formattedText += "-" + text.Substring(i, 1);
                else formattedText += text.Substring(i, 1);
            }

            if (formattedText == "") return;
            formattedText = formattedText.Substring(1);

            if (text != formattedText)
            {
                TextCreditCardNumber.Text = formattedText;
                TextCreditCardNumber.SelectionStart = formattedText.Length;
            }
        }

        private void TextCreditCardNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }


        private void ValidateCreditCardNumber(string cardNumber)
        {

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {

                Dictionary<string, string> result = Global.PayMethodClient.ValidateCreditCard(Constants.ValidateURL, cardNumber);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result != null && result["ValidationStatus"] != null && result["ValidationStatus"] == "Ok") PayCode = result["PayCode"];
                    else MessageUtils.ShowErrorMessage("", result["Message"]);

                    LoadingPanel.Visibility = Visibility.Hidden;
                    TextCardholderName.Focus();
                });

            }, 10);

        }

        private void Button_CreditCard_Add_Click(object sender, RoutedEventArgs e)
        {
            string accountNumber = TextCreditCardNumber.Text.Replace("-", "");
            string accountName = TextCardholderName.Text;
            string expiryDate = new DateTime(Convert.ToInt32(ComboYear.Text), Convert.ToInt32(ComboMonth.Text), 1).ToString("yyyy-MM-ddThh:mm:ss.000Z");
            bool isDefault = CheckDefault.IsChecked.Value;

            if (accountNumber == "")
            {
                MessageUtils.ShowErrorMessage("", string.Format(Properties.Resources.Please_Enter, Properties.Resources.Credit_Number1));
                return;
            }

            if (accountName == "")
            {
                MessageUtils.ShowErrorMessage("", string.Format(Properties.Resources.Please_Enter, Properties.Resources.Account_Name));
                return;
            }

            if (ComboYear.SelectedIndex < 0)
            {
                MessageUtils.ShowErrorMessage("", Properties.Resources.Choose_Year);
                return;
            }

            if (ComboMonth.SelectedIndex < 0)
            {
                MessageUtils.ShowErrorMessage("", Properties.Resources.Choose_Month);
                return;
            }

            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("payCode", PayCode);
            body.Add("accountNumber", accountNumber);
            body.Add("accountName", accountName);
            body.Add("expiryDate", expiryDate);
            body.Add("cvv", "");
            body.Add("customerOwned", true);
            body.Add("token", null);
            body.Add("tokenise", false);
            body.Add("exported", false);
            body.Add("protectNumber", true);
            body.Add("default", isDefault);
            body.Add("allowExisting", true);
            body.Add("checkConfiguration", true);
            body.Add("name", "");
            body.Add("companyName", "");
            body.Add("addressLine1", "");
            body.Add("addressLine2", "");
            body.Add("city", "");
            body.Add("state", "");
            body.Add("country", "");
            body.Add("phoneNumber", "");

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> result = Global.PayMethodClient.AddCreditCard(Constants.AddCreditCardURL, ContactCode, body);
                Application.Current.Dispatcher.Invoke(delegate
                {
                    if (result != null)
                    {
                        try
                        {
                            if (result["PaymentMethodId"] != null)
                            {
                                LoadingPanel.Visibility = Visibility.Hidden;
                                MaterialDesignThemes.Wpf.DialogHost.Close("MyDialogHost");
                                Global.mainWindow.ShowPaymentMethods();
                            }
                        } catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            MessageUtils.ShowErrorMessage("", result["ErrorMessage"].ToString());
                            LoadingPanel.Visibility = Visibility.Hidden;
                        }

                    }
                    else
                    {
                        MessageUtils.ShowErrorMessage("", result["ErrorMessage"].ToString());
                        LoadingPanel.Visibility = Visibility.Hidden;
                    }
                    
                });

            }, 10);

        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog(DialogHost);
        }

        private void TextCardholderName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ButtonSend.IsEnabled = TextCreditCardNumber.Text != "" && TextCardholderName.Text != "" && ComboMonth.SelectedItem != null && ComboYear.SelectedItem != null;
        }

        private void ComboMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ButtonSend.IsEnabled = TextCreditCardNumber.Text != "" && TextCardholderName.Text != "" && ComboMonth.SelectedItem != null && ComboYear.SelectedItem != null;
        }
    }
}
