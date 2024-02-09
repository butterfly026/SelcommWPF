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
    /// Interaction logic for BankAccount.xaml
    /// </summary>
    public partial class BankAccount : UserControl
    {

        private string ContactCode = "";
        private string DialogHost = "";

        public BankAccount(string code, string dialog = "MyDialogHost")
        {
            ContactCode = code;
            DialogHost = dialog;
            InitializeComponent();
        }

        private void TextAccount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
        }
        
        private void Button_Bank_Add_Click(object sender, RoutedEventArgs e)
        {
            string accountNumber = TextAccountNumber.Text;
            string accountName = TextAccountName.Text;
            string BSB = TextBSB.Text;
            bool isDefault = CheckDefault.IsChecked.Value;

            if (accountNumber == "")
            {
                MessageUtils.ShowErrorMessage("", string.Format(Properties.Resources.Please_Enter, Properties.Resources.Account_Number));
                return;
            }

            if (accountName == "")
            {
                MessageUtils.ShowErrorMessage("", string.Format(Properties.Resources.Please_Enter, Properties.Resources.Account_Number1));
                return;
            }

            if (BSB == "")
            {
                MessageUtils.ShowErrorMessage("", string.Format(Properties.Resources.Please_Enter, Properties.Resources.BSB));
                return;
            }

            Dictionary<string, object> body = new Dictionary<string, object>();
            body.Add("AccountNumber", accountNumber);
            body.Add("AccountName", accountName);
            body.Add("BSB", BSB);
            body.Add("CheckConfiguration", true);
            body.Add("default", isDefault);

            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, object> result = Global.PayMethodClient.AddBankAccount(Constants.AddBankAccountURL, ContactCode, body);
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
                        }
                        catch (Exception ex)
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

        private void TextAccountName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ButtonSend.IsEnabled = TextAccountName.Text != "" && TextBSB.Text != "" && TextAccountNumber.Text != "";
        }
    }
}
