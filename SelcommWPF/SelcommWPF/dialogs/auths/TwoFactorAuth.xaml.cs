using Microsoft.Win32;
using RestSharp;
using SelcommWPF.dialogs.auths;
using SelcommWPF.global;
using SelcommWPF.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
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
    /// Interaction logic for TwoFactorAuth.xaml
    /// </summary>
    public partial class TwoFactorAuth : UserControl
    {

        private string Email = "";
        private string Mobile = "";
        private string PIN = "";
        private DateTime PINExpireTime = new DateTime();

        public TwoFactorAuth()
        {
            InitializeComponent();
            InitializeControl();
        }

        private void InitializeControl()
        {
            LoadingPanel.Visibility = Visibility.Visible;
            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> header = Global.GetHeader(Global.CustomerToken);
                Dictionary<string, object> path = Global.BuildDictionary("ContactCode", Global.ContactId);
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                Dictionary<string, string> address = Global.RequestAPI<Dictionary<string, string>>(Constants.AddressForPINURL, Method.Get, header, path, query, "");

                Application.Current.Dispatcher.Invoke(async delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (address == null)
                    {
                        Global.CloseDialog();
                        return;
                    }

                    CheckBoxEmail.Visibility = Visibility.Visible;
                    CheckBoxMobile.Visibility = Visibility.Visible;

                    if (address == null || (address["Email"] == null && address["Mobile"] == null))
                    {
                        Global.CloseDialog();
                        await MessageUtils.ConfirmMessageAsync(Properties.Resources.Error, Properties.Resources.Not_Register, 1);
                        Application.Current.Shutdown();
                    }
                    else
                    {
                        if (address["Email"] == null)
                        {
                            Mobile = address["Mobile"];
                            CheckBoxEmail.Visibility = Visibility.Collapsed;
                            CheckBoxMobile.Content = string.Format("{0} : {1}", Properties.Resources.Mobile, Mobile);
                        }
                        else if (address["Mobile"] == null)
                        {
                            Email = address["Email"];
                            CheckBoxMobile.Visibility = Visibility.Collapsed;
                            CheckBoxEmail.Content = string.Format("{0} : {1}", Properties.Resources.Email, Email);
                        }
                        else
                        {
                            Email = address["Email"];
                            Mobile = address["Mobile"];
                            CheckBoxEmail.Content = string.Format("{0} : {1}", Properties.Resources.Email, Email);
                            CheckBoxMobile.Content = string.Format("{0} : {1}", Properties.Resources.Mobile, Mobile);
                        }
                    }
                });

            }, 10);

        }

        private void Button_Send_Click(object sender, RoutedEventArgs e)
        {
            string email = CheckBoxEmail.IsChecked.Value ? Email : "";
            string mobile = CheckBoxMobile.IsChecked.Value ? Mobile : "";
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> header = Global.GetHeader(Global.CustomerToken);
                Dictionary<string, object> path = Global.BuildDictionary("ContactCode", Global.ContactId);
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                Dictionary<string, object> body = Global.BuildDictionary("Application", "ESS");
                if (email != "") body.Add("email", email);
                if (mobile != "") body.Add("Mobile", mobile);
                Dictionary<string, string> result = Global.RequestAPI<Dictionary<string, string>>(Constants.GeneratePINURL, Method.Post, header, path, query, body);

                if (result != null)
                {
                    PIN = result["PIN"];
                    PINExpireTime = DateTime.Parse(result["Expiry"]);
                    MessageUtils.ShowMessage("", PIN);
                    if (PIN != null && PINExpireTime != null)
                    {
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            LoadingPanel.Visibility = Visibility.Hidden;
                            LabelExpireTime.Content = DateTime.Now.AddMinutes(4).ToString("h:mm tt");

                            Grid2FAStep1.Visibility = Visibility.Collapsed;
                            Grid2FAStep2.Visibility = Visibility.Visible;
                        });
                    }
                }

            }, 10);

        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Global.CloseDialog();
        }

        private void Button_Confirm_Click(object sender, RoutedEventArgs e)
        {
            ConfirmPIN();
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            Grid2FAStep1.Visibility = Visibility.Visible;
            Grid2FAStep2.Visibility = Visibility.Collapsed;
        }

        private void TextPINCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            string pin = TextPINCode.Text;
            ButtonConfirm.IsEnabled = pin.Length > 0;
        }

        private void TextPINCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) ConfirmPIN();
        }

        private void ConfirmPIN()
        {
            string pin = TextPINCode.Text;
            LoadingPanel.Visibility = Visibility.Visible;

            EasyTimer.SetTimeout(() =>
            {
                Dictionary<string, string> header = Global.GetHeader(Global.CustomerToken);
                Dictionary<string, object> path = Global.BuildDictionary("PIN", pin);
                path.Add("ContactCode", Global.ContactId);
                Dictionary<string, object> query = Global.BuildDictionary("api-version", 1.0);
                Dictionary<string, object> body = Global.BuildDictionary("Application", "ESS");
                body.Add("ContactId", Global.ContactId);
                HttpStatusCode result = Global.RequestAPI(Constants.ConsumePINURL, Method.Patch, header, path, query, body);

                Application.Current.Dispatcher.Invoke(delegate
                {
                    LoadingPanel.Visibility = Visibility.Hidden;
                    if (result == HttpStatusCode.OK) Global.loginWindow.CheckPasswordMustChange();
                    else MessageUtils.ShowErrorMessage("", Properties.Resources.Invalid_PIN);
                });

            }, 10);
        }
    }
}
